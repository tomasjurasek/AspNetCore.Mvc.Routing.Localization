using AspNetCore.Mvc.Routing.Localization.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using NSubstitute;
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
            var route = await _localizedRoutingProvider.ProvideRouteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<LocalizationDirection>());

            route.Should().BeNull();
            _controllerActionDescriptorProvider.Received(1).Get();
        }

        [Fact]
        public async Task ProvideRouteAsync_NotAnyRouteAndCallMoreThenOne_JustOneInit()
        {
            await _localizedRoutingProvider.ProvideRouteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<LocalizationDirection>());
            await _localizedRoutingProvider.ProvideRouteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<LocalizationDirection>());

            _controllerActionDescriptorProvider.Received(1).Get();
        }

        [Fact(Skip = "does not pass")]
        public async Task ProvideRouteAsync_HasNoAttributes_GetsDefaultRoute()
        {
            _controllerActionDescriptorProvider.Get()
                .Returns(new List<ControllerActionDescriptor>
                {
                    new ControllerActionDescriptor
                    {
                        RouteValues = new Dictionary<string, string>
                        {
                            { "controller", "NoAttributeController"},
                            { "action", "NoAttributeAction"}
                        },
                        ControllerTypeInfo = typeof(NoAttributeController).GetTypeInfo(),
                        MethodInfo = typeof(NoAttributeController).GetMethod("NoAttributeAction")
                    }
                });

            var route = await _localizedRoutingProvider.ProvideRouteAsync(null, "NoAttributeController", "NoAttributeAction", LocalizationDirection.TranslatedToOriginal);

            route.Action.Should().Be("NoAttributeAction");
            route.Controller.Should().Be("NoAttributeController");
        }

        [Fact(Skip = "does not pass")]
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

        [Fact(Skip ="does not pass")]
        public async Task ProvideRouteAsync_HasLocalizedRouteAttributes_GetsOriginalRoute()
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

            var route = await _localizedRoutingProvider.ProvideRouteAsync("en-US", "TranslatedHome", "TranslatedIndex", LocalizationDirection.TranslatedToOriginal);

            route.Action.Should().Be("Index");
            route.Controller.Should().Be("Home");
        }

        public sealed class NoAttributeController
        {
            public ActionResult NoAttributeAction()
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
    }
}
