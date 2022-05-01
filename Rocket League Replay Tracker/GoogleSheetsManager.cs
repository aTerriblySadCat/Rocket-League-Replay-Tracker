
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace Rocket_League_Replay_Tracker
{
    internal class GoogleSheetsManager
    {
        /// <summary>
        /// The Google Sheets service with which to communicate to Google Sheets.
        /// </summary>
        private static SheetsService? service;
        /// <summary>
        /// What services are available to the service.
        /// </summary>
        private static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        /// <summary>
        /// The name of this application.
        /// </summary>
        private static string ApplicationName = "Rocket League Replay Tracker";

        /// <summary>
        /// Creates the SheetsService by getting the local credentials.json file and using it to request the authorization needed to access the user's Spreadsheets.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static SheetsService CreateService()
        {
            if(service != null)
            {
                service.Dispose();
                service = null;
            }

            UserCredential userCredential;

            using (FileStream fileStream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                Task<UserCredential> userCredentialTask = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(fileStream).Secrets, Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true));
                userCredentialTask.Wait();
                if(!userCredentialTask.IsCompletedSuccessfully)
                {
                    throw new InvalidOperationException("Could not verify access to Google account!");
                }
                userCredential = userCredentialTask.Result;
                Console.WriteLine("Credentials saved to: " + credPath);
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = ApplicationName,
            });

            return service;
        }

        /// <summary>
        /// Does the a Spreadsheet with the given ID exist?
        /// </summary>
        /// <param name="spreadSheetId">The Spreadsheet ID to check.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool DoesSpreadSheetExist(string spreadSheetId)
        {
            if (service != null)
            {
                SpreadsheetsResource.GetRequest getRequest = service.Spreadsheets.Get(spreadSheetId);
                try
                {
                    getRequest.Execute();
                    return true;
                }
                catch(Exception)
                {
                    return false;
                }
            }

            throw new InvalidOperationException("Service is null! Start the service first using the CreateService() function!");
        }

        /// <summary>
        /// Does a Sheet with the given sheetName exist in the Spreadsheet with the given spreadSheetId?
        /// </summary>
        /// <param name="spreadSheetId">The ID of the Spreadsheet to look into.</param>
        /// <param name="sheetName">The name of the Sheet to check.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool DoesSheetExist(string spreadSheetId, string sheetName)
        {
            if(service != null)
            {
                sheetName += "!A1";
                SpreadsheetsResource.ValuesResource.GetRequest getRequest = service.Spreadsheets.Values.Get(spreadSheetId, sheetName);
                try
                {
                    getRequest.Execute();
                    return true;
                }
                catch(Exception)
                {
                    return false;
                }
            }

            throw new InvalidOperationException("Service is null! Start the service first using the CreateService() function!");
        }

        /// <summary>
        /// Creates a new Sheet with the given sheetName in the Spreadsheet with the given spreadSheetId. Returns the reponse from Google.
        /// </summary>
        /// <param name="spreadSheetId">The ID of the Spreadsheet to add the new Sheet to.</param>
        /// <param name="sheetName">The name of the Sheet.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static BatchUpdateSpreadsheetResponse CreateNewSheet(string spreadSheetId, string sheetName)
        {
            if(service != null)
            {
                List<Request> requests = new List<Request>();
                AddSheetRequest addSheetRequest = new AddSheetRequest();
                addSheetRequest.Properties = new SheetProperties();
                addSheetRequest.Properties.Title = sheetName;
                Request request = new Request();
                request.AddSheet = addSheetRequest;
                requests.Add(request);

                BatchUpdateSpreadsheetRequest body = new BatchUpdateSpreadsheetRequest();
                body.Requests = requests;
                SpreadsheetsResource.BatchUpdateRequest batchUpdateRequest = service.Spreadsheets.BatchUpdate(body, spreadSheetId);
                return batchUpdateRequest.Execute();
            }

            throw new InvalidOperationException("Service is null! Start the service first using the CreateService() function!");
        }

        /// <summary>
        /// Is the first row of the Sheet with the given sheetName in the Spreadsheet with the given spreadSheetId occupied? Checks the cell A1, and if it's empty returns false;
        /// </summary>
        /// <param name="spreadSheetId">The ID of the Spreadsheet to look into.</param>
        /// <param name="sheetName">The name of the Sheet to look into.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool IsFirstRowTaken(string spreadSheetId, string sheetName)
        {
            if(service != null)
            {
                sheetName += "!A1";
                SpreadsheetsResource.ValuesResource.GetRequest getRequest = service.Spreadsheets.Values.Get(spreadSheetId, sheetName);
                try
                {
                    ValueRange range = getRequest.Execute();
                    return (range.Values != null);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            throw new InvalidOperationException("Service is null! Start the service first using the CreateService() function!");
        }

        /// <summary>
        /// Insert the first row that is expected of a PlayerStats Sheet into the Sheet with the given sheetName in the Spreadsheet with the given spreadSheetId. Will always write into the row starting at cell A1. Returns the response from Google.
        /// </summary>
        /// <param name="spreadSheetId">The ID of the Spreadsheet in which the Sheet to insert into exists.</param>
        /// <param name="sheetName">The name of the Sheet to insert into.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static AppendValuesResponse InsertFirstPlayerStatsRow(string spreadSheetId, string sheetName)
        {
            if(service != null)
            {
                ValueRange valueRange = new ValueRange();
                valueRange.MajorDimension = "ROWS";
                List<object> firstRowValues = new List<object>() { "Name", "Online ID", "Platform", "Team ID", "Score", "Goals", "Assists", "Saves", "Shots" };
                valueRange.Values = new List<IList<object>>() { firstRowValues };
                sheetName += "!A1";
                SpreadsheetsResource.ValuesResource.AppendRequest appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadSheetId, sheetName);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                return appendRequest.Execute();
            }

            throw new InvalidOperationException("Service is null! Start the service first using the CreateService() function!");
        }

        /// <summary>
        /// Appends the given list of values, in a row, into the Sheet with the given sheetName that exists in the Spreadsheet with the given spreadSheetId. Returns the response from Google.
        /// </summary>
        /// <param name="spreadSheetId">The ID of the Spreadsheet in which the Sheet to append to exists.</param>
        /// <param name="sheetName">The name of the Sheet to append to.</param>
        /// <param name="values">The values to append to the Sheet.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static AppendValuesResponse AppendValues(string spreadSheetId, string sheetName, List<object> values)
        {
            if (service != null)
            {
                ValueRange valueRange = new ValueRange();
                valueRange.MajorDimension = "ROWS";
                valueRange.Values = new List<IList<object>>() { values };
                sheetName += "!A1";
                SpreadsheetsResource.ValuesResource.AppendRequest appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadSheetId, sheetName);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                return appendRequest.Execute();
            }

            throw new InvalidOperationException("Service is null! Start the service first using the CreateService() function!");
        }
    }
}
