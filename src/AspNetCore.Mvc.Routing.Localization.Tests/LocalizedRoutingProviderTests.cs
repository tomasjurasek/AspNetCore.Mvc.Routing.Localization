using AspNetCore.Mvc.Routing.Localization.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Mvc.Routing.Localization.Tests
{
    public class LocalizedRoutingProviderTests
    {
        private IControllerActionDescriptorProvider _controllerActionDescriptorProvider;
        private LocalizedRouteProvider _localizedRoutingProvider;

        public LocalizedRoutingProviderTests()
        {
            _controllerActionDescriptorProvider = Substitute.For<IControllerActionDescriptorProvider>();
            _localizedRoutingProvider = new LocalizedRouteProvider(_controllerActionDescriptorProvider);
        }

        [Fact]
        public async Task ProvideRouteAsync_NotAnyRoute_GetNull()
        {
            var route = await _localizedRoutingProvider.ProvideRouteAsync("en-US", "controller", "action", LocalizationDirection.TranslatedToOriginal);

            route.Should().BeNull();
            _controllerActionDescriptorProvider.Received(1).Get();
        }

        [Fact]
        public async Task ProvideRouteAsync_NotAnyRouteAndCallMoreThenOne_JustOneInit()
        {
            await _localizedRoutingProvider.ProvideRouteAsync("en-US", "controller", "action", LocalizationDirection.TranslatedToOriginal);
            await _localizedRoutingProvider.ProvideRouteAsync("en-US", "controller", "action", LocalizationDirection.TranslatedToOriginal);

            _controllerActionDescriptorProvider.Received(1).Get();
        }

        [Fact]
        public async Task ProvideRouteAsync_HasLocalizedRouteAttributes_GetsLocalizedRoute()
        {
            _controllerActionDescriptorProvider.Get()
                .Returns(new List<ControllerActionDescriptor>
                {
                    new ControllerActionDescriptor
                    {
                        RouteValues = new Dictionary<string, string>
                        {
                            { "controller", "Home"},
                            { "action", "Index"}
                        },
                        ControllerTypeInfo = typeof(HomeController).GetTypeInfo(),
                        MethodInfo = typeof(HomeController).GetMethod("Index")
                    }
                });

            var route = await _localizedRoutingProvider.ProvideRouteAsync("en-US", "Home", "Index", LocalizationDirection.OriginalToTranslated);

            route.Action.Should().Be("TranslatedIndex");
            route.Controller.Should().Be("TranslatedHome");
        }

        [Theory]
        [InlineData(typeof(NoAttributeController), "Index", "Home", "Index")]
        [InlineData(typeof(HomeController), "Index", "TranslatedHome", "TranslatedIndex")]
        [InlineData(typeof(HomeController1), "Index", "TranslatedHome", "TranslatedIndex")]
        [InlineData(typeof(HomeController2), "Index", "TranslatedHome", "Index")]
        [InlineData(typeof(HomeController3), "Index", "TranslatedHome", "TranslatedIndex")]
        [InlineData(typeof(HomeController4), "Index", "TranslatedHome", "Index")]
        public async Task ProvideRouteAsync_HasAttributes_GetsOriginalRoute(Type originalController, string originalAction,
            string controller, string action)
        {
            _controllerActionDescriptorProvider.Get()
                .Returns(new List<ControllerActionDescriptor>
                {
                    new ControllerActionDescriptor
                    {
                        RouteValues = new Dictionary<string, string>
                        {
                            { "controller", "Home"},
                            { "action", "Index"}
                        },
                        ControllerTypeInfo = originalController.GetTypeInfo(),
                        MethodInfo = originalController.GetMethod(originalAction)
                    }
                });

            var route = await _localizedRoutingProvider.ProvideRouteAsync("en-US", controller, action, LocalizationDirection.TranslatedToOriginal);

            route.Action.Should().Be("Index");
            route.Controller.Should().Be("Home");
        }

        public sealed class NoAttributeController
        {
            public ActionResult Index()
            {
                return null;
            }
        }

        [LocalizedRoute("en-US", "TranslatedHome")]
        private sealed class HomeController
        {
            [LocalizedRoute("en-US", "TranslatedIndex")]
            public ActionResult Index()
            {
                return null;
            }
        }

        [LocalizedRoute("en-US", "TranslatedHome")]
        private sealed class HomeController1
        {
            [Route("TranslatedIndex")]
            public ActionResult Index()
            {
                return null;
            }
        }

        [LocalizedRoute("en-US", "TranslatedHome")]
        private sealed class HomeController2
        {
            public ActionResult Index()
            {
                return null;
            }
        }

        [Route("TranslatedHome")]
        private sealed class HomeController3
        {
            [Route("TranslatedIndex")]
            public ActionResult Index()
            {
                return null;
            }
        }

        [Route("TranslatedHome")]
        private sealed class HomeController4
        {
            public ActionResult Index()
            {
                return null;
            }
        }
    }
}
