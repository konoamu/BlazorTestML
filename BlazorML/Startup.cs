using BlazorML.ObjectDetection.ImageFileHelpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnnxObjectDetection;
using Microsoft.Extensions.ML;
using BlazorML.ObjectDetection.Services;
using BlazorML.ObjectDetection.Utils;
using BlazorML.Model.DataModels.Sentement2;

namespace BlazorML
{
    public class Startup
    {
        private readonly string _onnxModelFilePath;
        private readonly string _mlnetModelFilePath;
        private readonly string _mlnetModelFilePath2;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _onnxModelFilePath = CommonHelpers.GetAbsolutePath(Configuration["MLModel:OnnxModelFilePath"]);
            _mlnetModelFilePath = CommonHelpers.GetAbsolutePath(Configuration["MLModel:MLNETModelFilePath"]);
            _mlnetModelFilePath2 = CommonHelpers.GetAbsolutePath(Configuration["MLModel:MLNETModelFilePath2"]);

            var onnxModelConfigurator = new OnnxModelConfigurator(new TinyYoloModel(_onnxModelFilePath));

            onnxModelConfigurator.SaveMLNetModel(_mlnetModelFilePath);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddPredictionEnginePool<ImageInputData, TinyYoloPrediction>().
                FromFile(_mlnetModelFilePath);

            services.AddPredictionEnginePool<SampleObservation, SamplePrediction>().
                FromFile(_mlnetModelFilePath2);

            services.AddTransient<IImageFileWriter, ImageFileWriter>();
            services.AddTransient<IObjectDetectionService, ObjectDetectionService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
