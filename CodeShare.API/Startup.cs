using Autofac;
using CodeShare.API.AutoFacExtension;
using CodeShare.Model;
using CodeShare.MongoDBRepository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CodeShare.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //方式一
            Configuration.GetSection("MongoDBConn").Bind(new MongoDBAppSetting());
            Configuration.GetSection("WeChatConfig").Bind(new WeChatAppSetting());

            //方式二
            //services.Configure<T>(op => { new T() { } });
            //方式二 .1
            //Controllers 里面 IOption<T> _t  注入的方式获取 


            //AutoFac 提供控制器支持
            //1   替换控制器的替换规则
            //1.1 可以指定控制器让 容器来创建
            services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CodeShare.API", Version = "v1" });
            });
        }

        /// <summary>
        /// AutoFac 自己会调用这个方法 进行注册
        /// 1.负责注册 各种服务
        /// 2.ServiceCollection 注册的,也同样是可以使用的
        /// 3.还支持控制器 里面的属性注入
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            Assembly serviceDLL = Assembly.Load(new AssemblyName("CodeShare.Service"));

            //1.Service 后缀类的反射 注入
            var serviceTypes = serviceDLL.GetTypes().Where(t => t.Name.EndsWith("Service") && !t.GetTypeInfo().IsAbstract);

            foreach (var serviceType in serviceTypes)
            {
                //var asdfasf = serviceType.Name;
                foreach (var interType in serviceType.GetInterfaces())
                {
                    var sname = serviceType.Name;
                    var Iname = interType.Name;
                    Console.WriteLine($"{sname}--->{Iname}");
                    builder.RegisterType(serviceType).As(interType).InstancePerDependency()
                        .AsImplementedInterfaces()//自动以其实现的所有接口类型暴露（包括IDisposable接口）
                        .InstancePerLifetimeScope()
                        .PropertiesAutowired();//支持属性注入的方法;
                }
            }

            //2   首先需要在Service,里面提供支持 services.Replace(.......)
            //2.1 控制器实例的注入 
            var controllerTypes = typeof(Startup).GetTypeInfo().Assembly.DefinedTypes.
                Where(x => x.IsClass && typeof(ControllerBase).GetTypeInfo().IsAssignableFrom(x)).
                Select(x => x.AsType()).
                ToArray();
            builder.RegisterTypes(controllerTypes)
                //支持属性注入的方法;
                //CustomPropertySelector 设置哪些是可以被注入的 (指定特性属性注入的支持)
                .PropertiesAutowired(new CustomPropertySelector());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeShare.API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
