using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;

namespace AllotmentStatusCheck
{
    public class LinkIntimeRegistrar
    {
        /// <summary>
        /// Checks IPO allotment for a given PAN.
        /// </summary>
        public static async Task CheckIPOAllotment(
            string apiUrl,
            string pan,
            string clientId,
            List<string> allotedList,
            List<string> notAllotedList,
            List<string> noRecordFoundList)
        {
            using (HttpClient client = new HttpClient())
            {
                // Prepare the payload for the POST request
                var payload = new
                {
                    clientid = clientId,
                    PAN = pan,
                    IFSC = "",
                    CHKVAL = "1",
                    token = ""
                };

                string jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);

                try
                {
                    // Make the POST request
                    HttpResponseMessage response = await client.PostAsync(apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        // Deserialize the response to extract XML content
                        var responseObject = System.Text.Json.JsonSerializer.Deserialize<ResponseWrapperLinkIntime>(jsonResponse);

                        if (responseObject != null && !string.IsNullOrEmpty(responseObject.d) && responseObject.d != "<NewDataSet />")
                        {
                            ParseAndCategorizeXML(responseObject.d, pan, allotedList, notAllotedList);
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
        }

        /// <summary>
        /// Parses XML content and categorizes the results into allotted or not allotted lists.
        /// </summary>
        private static void ParseAndCategorizeXML(
            string xmlContent,
            string pan,
            List<string> allotedList,
            List<string> notAllotedList)
        {
            try
            {
                // Parse the XML content
                XDocument xmlDoc = XDocument.Parse(xmlContent);
                XElement tableElement = xmlDoc.Root?.Element("Table");

                if (tableElement != null)
                {
                    string name = tableElement.Element("NAME1")?.Value;
                    int allotment = int.Parse(tableElement.Element("ALLOT")?.Value ?? "0");

                    string result = $"Name: {name}, PAN: {pan}, AllotedShares: {allotment}";
                    if (allotment > 0)
                    {
                        allotedList.Add(result);
                    }
                    else
                    {
                        notAllotedList.Add(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing XML for PAN: {pan}. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Fetches the list of companies from Link Intime's API.
        /// </summary>
        public static async Task<Dictionary<string, (string, string)>> GetLinkIntimeCompanyList()
        {
            var companyList = new Dictionary<string, (string, string)>();
            string url = "https://linkintime.co.in/initial_offer/IPO.aspx/GetDetails";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Set request headers
                    client.DefaultRequestHeaders.Add("Accept", "application/json");

                    // Create an empty request body
                    StringContent requestBody = new StringContent("{}", Encoding.UTF8, "application/json");

                    // Make the POST request
                    HttpResponseMessage response = await client.PostAsync(url, requestBody);
                    response.EnsureSuccessStatusCode();

                    // Parse the response
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Dictionary<string, string> parsedData = ParseXmlFromJsonResponse(responseContent);

                    int index = 1;
                    foreach (var kvp in parsedData)
                    {
                        companyList.Add(index.ToString(), (kvp.Key, kvp.Value));
                        index++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while fetching company list: {ex.Message}");
                }
            }

            return companyList;
        }

        /// <summary>
        /// Parses the JSON response to extract XML content and return company details.
        /// </summary>
        private static Dictionary<string, string> ParseXmlFromJsonResponse(string jsonResponse)
        {
            var result = new Dictionary<string, string>();

            try
            {
                // Extract the XML content from the "d" property
                string xmlContent = System.Text.Json.JsonDocument.Parse(jsonResponse)
                                     .RootElement.GetProperty("d").GetString();

                // Parse the XML content
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                // Select all Table nodes
                XmlNodeList tableNodes = xmlDoc.SelectNodes("//Table");

                foreach (XmlNode table in tableNodes)
                {
                    string companyId = table["company_id"]?.InnerText;
                    string companyName = table["companyname"]?.InnerText;

                    if (!string.IsNullOrEmpty(companyId) && !string.IsNullOrEmpty(companyName))
                    {
                        result[companyId] = companyName;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing XML from JSON response: {ex.Message}");
            }

            return result;
        }
    }

    /// <summary>
    /// Wrapper class for Link Intime API response.
    /// </summary>
    class ResponseWrapperLinkIntime
    {
        public string d { get; set; }
    }
}
