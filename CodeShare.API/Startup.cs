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

            //��ʽһ
            Configuration.GetSection("MongoDBConn").Bind(new MongoDBAppSetting());
            Configuration.GetSection("WeChatConfig").Bind(new WeChatAppSetting());

            //��ʽ��
            //services.Configure<T>(op => { new T() { } });
            //��ʽ�� .1
            //Controllers ���� IOption<T> _t  ע��ķ�ʽ��ȡ 


            //AutoFac �ṩ������֧��
            //1   �滻���������滻����
            //1.1 ����ָ���������� ����������
            services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CodeShare.API", Version = "v1" });
            });
        }

        /// <summary>
        /// AutoFac �Լ������������� ����ע��
        /// 1.����ע�� ���ַ���
        /// 2.ServiceCollection ע���,Ҳͬ���ǿ���ʹ�õ�
        /// 3.��֧�ֿ����� ���������ע��
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            Assembly serviceDLL = Assembly.Load(new AssemblyName("CodeShare.Service"));

            //1.Service ��׺��ķ��� ע��
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
                        .AsImplementedInterfaces()//�Զ�����ʵ�ֵ����нӿ����ͱ�¶������IDisposable�ӿڣ�
                        .InstancePerLifetimeScope()
                        .PropertiesAutowired();//֧������ע��ķ���;
                }
            }

            //2   ������Ҫ��Service,�����ṩ֧�� services.Replace(.......)
            //2.1 ������ʵ����ע�� 
            var controllerTypes = typeof(Startup).GetTypeInfo().Assembly.DefinedTypes.
                Where(x => x.IsClass && typeof(ControllerBase).GetTypeInfo().IsAssignableFrom(x)).
                Select(x => x.AsType()).
                ToArray();
            builder.RegisterTypes(controllerTypes)
                //֧������ע��ķ���;
                //CustomPropertySelector ������Щ�ǿ��Ա�ע��� (ָ����������ע���֧��)
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
