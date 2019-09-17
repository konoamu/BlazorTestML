using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using BlazorML.ObjectDetection.ImageFileHelpers;
using BlazorML.ObjectDetection.Services;
using BlazorML.ObjectDetection.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using OnnxObjectDetection;

namespace BlazorML.ObjectDetection
{

    public class ObjectDetectionController
    {
        private readonly string _imagesTmpFolder;

        private readonly ILogger<ObjectDetectionController> _logger;
        private readonly IObjectDetectionService _objectDetectionService;

        private string base64String = string.Empty;
        public ObjectDetectionController(IObjectDetectionService ObjectDetectionService, ILogger<ObjectDetectionController> logger, IImageFileWriter imageWriter) //When using DI/IoC (IImageFileWriter imageWriter)
        {
            //Get injected dependencies
            _objectDetectionService = ObjectDetectionService;
            _logger = logger;
            _imagesTmpFolder = CommonHelpers.GetAbsolutePath(@"../../../ImagesTemp");
        }

        public class Result
        {
            public string imageString { get; set; }
        }

        public Result Get(string url)
        {
            string imageFileRelativePath = @"../../../assets" + url;
            string imageFilePath = CommonHelpers.GetAbsolutePath(imageFileRelativePath);
            try
            {
                Image image = Image.FromFile(imageFilePath);
                //Convert to Bitmap
                Bitmap bitmapImage = (Bitmap)image;

                //Set the specific image data into the ImageInputData type used in the DataView
                ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };

                //Detect the objects in the image                
                var result = DetectAndPaintImage(imageInputData, imageFilePath);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error is: " + e.Message);
                return null;
            }
        }

        public Result IdentifyObjects(IFormFile imageFile)
        {
            try
            {
                MemoryStream imageMemoryStream = new MemoryStream();
                imageFile.CopyTo(imageMemoryStream);

                //Check that the image is valid
                byte[] imageData = imageMemoryStream.ToArray();

                //Convert to Image
                Image image = Image.FromStream(imageMemoryStream);

                string fileName = string.Format("{0}.Jpeg", image.GetHashCode());
                string imageFilePath = Path.Combine(_imagesTmpFolder, fileName);
                //save image to a path
                image.Save(imageFilePath, ImageFormat.Jpeg);

                //Convert to Bitmap
                Bitmap bitmapImage = (Bitmap)image;

                _logger.LogInformation($"Start processing image...");

                //Measure execution time
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //Set the specific image data into the ImageInputData type used in the DataView
                ImageInputData imageInputData = new ImageInputData { Image = bitmapImage };

                //Detect the objects in the image                
                var result = DetectAndPaintImage(imageInputData, imageFilePath);

                //Stop measuring time
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                _logger.LogInformation($"Image processed in {elapsedMs} miliseconds");
                return result;
            }
            catch (Exception e)
            {
                _logger.LogInformation("Error is: " + e.Message);
                return null;
            }
        }

        private Result DetectAndPaintImage(ImageInputData imageInputData, string imageFilePath)
        {
            //Predict the objects in the image
            _objectDetectionService.DetectObjectsUsingModel(imageInputData);
            var img = _objectDetectionService.DrawBoundingBox(imageFilePath);

            using (MemoryStream m = new MemoryStream())
            {
                img.Save(m, img.RawFormat);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                base64String = Convert.ToBase64String(imageBytes);
                var result = new Result { imageString = base64String };
                return result;
            }
        }
    }
}
