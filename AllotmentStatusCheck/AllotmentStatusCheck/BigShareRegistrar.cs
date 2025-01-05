using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AllotmentStatusCheck
{
    public class BigShareRegistrar
    {
        private static readonly HttpClient Client = new HttpClient();

        public static async Task CheckIPOAllotment(
            string apiUrl,
            string pan,
            string clientId,
            List<string> allotedList,
            List<string> notAllotedList,
            List<string> noRecordFoundList)
        {
            // Prepare the payload
            var payload = new
            {
                Applicationno = string.Empty,
                Company = clientId,  // Using clientId here
                SelectionType = "PN",
                PanNo = pan,
                txtcsdl = string.Empty,
                txtDPID = string.Empty,
                txtClId = string.Empty,
                ddlType = string.Empty
            };

            string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);

            try
            {
                // Make the POST request
                HttpResponseMessage response = await Client.PostAsync(apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    // Deserialize the response
                    var responseObject = System.Text.Json.JsonSerializer.Deserialize<ResponseWrapperBigShare>(jsonResponse);
                    if (responseObject?.d?.DPID != "No data found")
                    {
                        ProcessResponse(responseObject.d, pan, allotedList, notAllotedList);
                    }
                    else
                    {
                        noRecordFoundList.Add(pan);
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to fetch data for PAN: {pan}. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred for PAN: {pan}. Error: {ex.Message}");
            }
        }

        private static void ProcessResponse(Data data, string pan, List<string> allotedList, List<string> notAllotedList)
        {
            string result;
            if (data.ALLOTED == "NON-ALLOTTE")
            {
                result = $"Name: {data.Name}, PAN: {pan}, AllotedShares: 0";
                notAllotedList.Add(result);
            }
            else
            {
                result = $"Name: {data.Name}, PAN: {pan}, AllotedShares: {data.APPLIED}";
                allotedList.Add(result);
            }
        }

        public static async Task<Dictionary<string, (string, string)>> GetBigShareCompanyList()
        {
            var companyList = new Dictionary<string, (string, string)>();

            const string url = "https://ipo.bigshareonline.com/IPO_Status.html";

            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            options.AddArgument("headless");

            using (var driver = new ChromeDriver(service, options))
            {
                driver.Navigate().GoToUrl(url);

                // Find the dropdown and extract options
                var dropdown = driver.FindElement(By.Id("ddlCompany"));
                var optionElements = dropdown.FindElements(By.TagName("option"));
                int index = 1;

                foreach (var option in optionElements)
                {
                    string value = option.GetAttribute("value")?.Trim();
                    string text = option.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(text) && text != "--Select Company--")
                    {
                        companyList.Add(index++.ToString(), (value, text));
                    }
                }
            }

            return companyList;
        }
    }

    public class ResponseWrapperBigShare
    {
        public Data d { get; set; }
    }

    public class Data
    {
        public string APPLICATION_NO { get; set; }
        public string DPID { get; set; }
        public string Name { get; set; }
        public string APPLIED { get; set; }
        public string ALLOTED { get; set; }
    }
}
