# IPO Allotment Checker Console Application

This is a console application for checking the IPO allotment status for multiple PAN cards. It supports two registrars: **Link Intime India Pvt. Ltd.** and **BigShare Services Pvt. Ltd.**. Based on the selected registrar and company, the application fetches allotment details, categorizing the results into allotted, not allotted, and no record found. The final results are saved to a text file.

## Features
- Supports two registrars: Link Intime and BigShare.
- Allows users to select a company under the chosen registrar.
- Fetches allotment status for multiple PAN cards.
- Categorizes results into:
  - **Allotted**
  - **Not Allotted**
  - **No Record Found**
- Outputs results to a text file for record-keeping.

## Technologies Used
- C# (.NET)
- HTTP Requests for API interaction.
- Selenium WebDriver for fetching company lists from BigShare.
- JSON and XML parsing.

## Prerequisites
- .NET runtime installed.
- Chrome browser and WebDriver for Selenium operations.
- Network access to registrar APIs.

## How to Run
1. Clone the repository to your local machine.
2. Ensure all dependencies are properly configured.
3. Compile and run the application using `dotnet run`.

## Workflow
1. The user is prompted to select a registrar:
   - **1** for Link Intime India Pvt. Ltd.
   - **2** for BigShare Services Pvt. Ltd.
2. Based on the selected registrar, the application retrieves the list of companies.
3. The user selects a company to check IPO allotment.
4. The application checks the allotment status for a predefined list of PAN cards.
5. Results are categorized and saved to a file located at `C:\Vedant\Allotment\LinkinTime.txt`.

## Sample Output
The output file contains:

Company Name:

Alloted: 
Name: John Doe, PAN: ABCDE1234F, AllottedShares: 100

Not Alloted: 
Name: Jane Doe, PAN: XYZ9876G, AllottedShares: 0

No Record Found: 
Name: Alex Doe, PAN: LMN4567H

## Configuration
- Update the PAN list in the `GetNamePanNumbers` method of the `Program.cs` file.
- Modify file paths as needed for output results.

## Limitations
- Requires internet access for API and Selenium operations.
- Only supports the registrars and companies accessible via the specified APIs.

## Future Enhancements
- Add support for additional registrars.
- Allow dynamic PAN input instead of hardcoded lists.
- Improve exception handling and logging mechanisms.

## License
This project is licensed under the MIT License.
