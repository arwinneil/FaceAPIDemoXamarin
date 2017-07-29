using Newtonsoft.Json;
using Plugin.Media;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Xamarin.Forms;

namespace FaceAPIDemo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private const string subscriptionKey = "* Insert Subsription Key Here*";

        private const string uriBase = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0/";

        private DetectResponse[] tempDetectResponse;
        private string faceId1;
        private string faceId2;
        private VerifyResponse results;

        private void LoadTwitterImage(object sender, System.EventArgs e)

        {
            ResultFrame.IsVisible = false;

            TwitterImageFrame.IsVisible = true;

            string url = "https://twitter.com/" + handle.Text + "/profile_image?size=original";
            TwitterImage.Source = url;

            GetFaceIdUrl(url);
        }

        public async void GetFaceIdUrl(string url)

        {
            TwitterLabel.Text = "Detecting Face...";

            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "detect?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a JSON.
            byte[] byteData = Encoding.UTF8.GetBytes("{ \"url\":\"" + url + "\"}");

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/json".
                // The other content types you can use are "application/octet-stream" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    // Get the JSON response.
                    string contentString = await response.Content.ReadAsStringAsync();

                    if (contentString != "[]")
                    {
                        faceId1 = DeserialiseFaceId(contentString);
                        TwitterLabel.Text = "faceId1 : " + faceId1;
                    }
                    else
                    {
                        CameraImageLabel.Text = "No face detected, please try again.";
                    }
                }
                else
                {
                    CameraImageLabel.Text = "Am error occured, please try again.";
                }
            }
        }

        public string DeserialiseFaceId(string json)
        {
            tempDetectResponse = JsonConvert.DeserializeObject<DetectResponse[]>(json);

            return tempDetectResponse[0].faceId;
        }

        private async void TakePhoto(object sender, System.EventArgs e)

        {
            ResultFrame.IsVisible = false;

            Plugin.Media.Abstractions.StoreCameraMediaOptions options = new Plugin.Media.Abstractions.StoreCameraMediaOptions();

            var image = await CrossMedia.Current.TakePhotoAsync(options);
            CameraImage.Source = ImageSource.FromStream(() =>
            {
                return image.GetStream();
            });

            CameraImage.HeightRequest = TwitterImage.Height;

            CameraImageFrame.IsVisible = true;

            GetFaceIdImage(image.Path);
        }

        public async void GetFaceIdImage(string imageFilePath)
        {
            CameraImageLabel.Text = "Detecting Face...";

            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true";

            // Assemble the URI for the REST API Call.
            string uri = uriBase + "detect?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    // Get the JSON response.
                    string contentString = await response.Content.ReadAsStringAsync();

                    if (contentString != "[]")
                    {
                        faceId2 = DeserialiseFaceId(contentString);

                        CameraImageLabel.Text = "faceId2 : " + faceId2;
                    }
                    else
                    {
                        CameraImageLabel.Text = "No face detected, please try again.";
                    }
                }
                else
                {
                    CameraImageLabel.Text = "Am error occured, please try again.";
                }
            }
        }

        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            /// <summary>
            /// Returns the contents of the specified file as a byte array.
            /// </summary>
            /// <param name="imageFilePath">The image file to read.</param>
            /// <returns>The byte array of the image data.</returns>

            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        private void StartVerification(object sender, System.EventArgs e)
        {
            VerifyFaces();
        }

        public async void VerifyFaces()
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
			
            // Assemble the URI for the REST API Call.
            string uri = uriBase + "verify?" + requestParameters;

            HttpResponseMessage response;

            // Request body. 
            byte[] byteData = Encoding.UTF8.GetBytes("{ \"faceId1\":\"" + faceId1 + "\",\"faceId2\":\"" + faceId2 + "\"}");

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                DeserialiseResults(contentString);
              
            }
        }

        public void DeserialiseResults(string json)
        {
            results = JsonConvert.DeserializeObject<VerifyResponse>(json);

            ResultFrame.IsVisible = true;

            if (results.isIdentical == true)
            {
                ResulLabel.Text = "Faces belong to the same person with a confidence score of " + results.confidence.ToString();

                ResultIcon.Source = "checked";
            }
            else
            {
                ResulLabel.Text = "Faces dont belong to the same person";
                ResultIcon.Source = "cancel";
            }
        }
    }
}