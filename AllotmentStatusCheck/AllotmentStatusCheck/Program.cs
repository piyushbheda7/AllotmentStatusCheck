using AllotmentStatusCheck;
using System.Diagnostics;
using System.Text;

class IPOAllotmentChecker
{
    private static async Task Main(string[] args)
    {
        string apiUrl;
        bool isLinkIntimeRegistrar = false, isBigShareRegistrar = false;
        Dictionary<string, (string ClientId, string CompanyName)> LinkIntimeList = new();
        Dictionary<string, (string ClientId, string CompanyName)> BigShareList = new();
        string selectedCompanyClientId = "", selectedCompanyName = "";

        while (true)
        {
            Console.WriteLine("1. Link Intime India Pvt. Ltd.");
            Console.WriteLine("2. Bigshare Services Pvt. Ltd.");
            Console.Write("Please Select the Registrar: ");

            string selectedRegistrar = Console.ReadLine();

            if (selectedRegistrar == "1")
            {
                isLinkIntimeRegistrar = true;
                apiUrl = "https://linkintime.co.in/initial_offer/IPO.aspx/SearchOnPan";
                LinkIntimeList = await LinkIntimeRegistrar.GetLinkIntimeCompanyList();
                string selectedItem;
                while (true)
                {

                    DisplayCompanyOptions(LinkIntimeList);
                    selectedItem = Console.ReadLine();
                    int selectedItemInt;

                    if (int.TryParse(selectedItem, out selectedItemInt) && selectedItemInt > 0 && selectedItemInt <= LinkIntimeList.Count)
                        break;
                }
                (selectedCompanyClientId, selectedCompanyName) = LinkIntimeList[selectedItem];
                break;
            }
            else if (selectedRegistrar == "2")
            {
                isBigShareRegistrar = true;
                apiUrl = "https://ipo.bigshareonline.com/Data.aspx/FetchIpodetails";
                BigShareList = await BigShareRegistrar.GetBigShareCompanyList();
                string selectedItem;
                while (true)
                {

                    DisplayCompanyOptions(BigShareList);
                    selectedItem = Console.ReadLine();
                    int selectedItemInt;

                    if (int.TryParse(selectedItem, out selectedItemInt) && selectedItemInt > 0 && selectedItemInt <= BigShareList.Count)
                        break;
                }
                (selectedCompanyClientId, selectedCompanyName) = BigShareList[selectedItem];
                break;
            }
            else
            {
                Console.WriteLine("\nPlease Select a Valid Registrar\n");
            }
        }

        Dictionary<string, string> namePanNumbers = GetNamePanNumbers();
        List<string> allotedList = new(), notAllotedList = new(), noRecordFoundList = new();

        foreach (var (name, pan) in namePanNumbers)
        {
            if (isLinkIntimeRegistrar)
            {
                await LinkIntimeRegistrar.CheckIPOAllotment(apiUrl, pan, selectedCompanyClientId, allotedList, notAllotedList, noRecordFoundList);
            }

            if (isBigShareRegistrar)
            {
                await BigShareRegistrar.CheckIPOAllotment(apiUrl, pan, selectedCompanyClientId, allotedList, notAllotedList, noRecordFoundList);
            }
        }

        SaveResultsToFile(selectedCompanyName, namePanNumbers, allotedList, notAllotedList, noRecordFoundList);
        Console.WriteLine("\nResults have been written to the file.");
    }

    private static void DisplayCompanyOptions(Dictionary<string, (string ClientId, string CompanyName)> companyList)
    {
        Console.WriteLine();
        foreach (var (key, value) in companyList)
        {
            Console.WriteLine($"{key}. {value.CompanyName}");
        }
        Console.Write("Please Select a Company: ");
    }

    private static Dictionary<string, string> GetNamePanNumbers()
    {
        return new()
        {
            { "Piyush Bheda", "BXNPV4450Q" },
            { "Hardik Bheda", "DPGPB8243L" },
            { "Kanji Bheda", "DVUPB9904G" },
            { "Badur Bheda", "CVKPB3766P" },
            { "Kanji Bhammar", "DEYPB5197A" },
            { "Ghanshyam Bhammar", "CTWPB3059B" },
            { "Bhumiben", "JTPPD1541G" },
            { "Dhaval Der", "CLIPD4361C" },
            { "Vishal Bhammar", "CQFPB4532D" },
            { "Shubham Dobariya", "HKHPD6259J" },
            { "Sagar Gondaliya", "DKDPG7069B" },
            { "Sagar Devera", "OZNPK4875B" },
            { "Harkesh Dobariya", "JXOPD3443J" },
            { "Bhargav Dobariya", "JYPPD8598J" },
            { "Bhargav Friend", "CXJPV7321M" },
            { "Kevin Nasit", "CRWPN2848C" },
            { "Shiv Gajipara", "BNLPG1113P" },
            { "Jayvir Bhai", "AQCPV7840D" },
            { "Munna Gamara", "RWLPS1378K" },
            { "Champu Jajada", "CNFPJ3965D" },
            { "Vishvas Tapaniya", "CPQPT2320M" },
            { "Piyush Makvana", "JFAPM7940R" },
            { "Nitesh Solanki", "LZPPS4368J" },
            { "Hiren Doctor", "ASRPK3538L" },
            { "Bholo Maraj", "DNCPG0018J" }
        };
    }

    private static void SaveResultsToFile(string companyName, Dictionary<string, string> namePanNumbers, List<string> allotedList, List<string> notAllotedList, List<string> noRecordFoundList)
    {
        string filePath = "C:\\Vedant\\Allotment\\LinkinTime.txt";

        using StreamWriter writer = new(filePath);
        writer.WriteLine($"{companyName}:");

        writer.WriteLine("\nAlloted:");
        foreach (string person in allotedList)
        {
            writer.WriteLine(person);
        }

        writer.WriteLine("\nNot Alloted:");
        foreach (string person in notAllotedList)
        {
            writer.WriteLine(person);
        }

        writer.WriteLine("\nNo Record Found:");
        foreach (string person in noRecordFoundList)
        {
            string key = namePanNumbers.FirstOrDefault(x => x.Value == person).Key;
            writer.WriteLine($"Name: {key}, PAN: {person}");
        }
    }
}