var app = angular.module('InfinityLab', ['ngRoute', 'LocalStorageModule', 'angular-loading-bar']);

var serviceBase = '';
app.constant('ngAuthSettings', {
    apiServiceBaseUri: serviceBase,
    clientId: 'InfyPOS',
    secret: 'KareInfinity'
});

app.directive('onkeyenter', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs, autoCtrl) {
            element.bind("keydown keypress", function (event) {
                if (event.which === 13) {
                    scope.$apply(function () {
                        scope.$eval(attrs.onkeyenter, { $event: event });
                    });
                    event.preventDefault();
                }
            });
        }
    }
});

app.directive('focusid', function () {
    return {
        restrict: 'A',
        controller: function ($scope, $element, $attrs) {
            if ($element[0].tagName == 'INPUT') {
                if (typeof ($attrs.focusid) == 'string' && $attrs.focusid.startsWith('{{')) {
                    var fooName = 'setFocus' + $scope.$eval($attrs.focusid.replace('{{', '').replace('}}', ''));
                    $scope.$parent[fooName] = function () {
                        $element.focus();
                    };
                }
                else {
                    var fooName = 'setFocus' + $attrs.focusid;

                    if (typeof ($scope.$parent.show_autocomplete) == 'undefined') {
                        $scope[fooName] = function () {
                            $element.focus();
                        }
                    }
                    else {
                        $scope.$parent.$parent[fooName] = function () {
                            $element.focus();
                        }
                    }
                }
            }
        },
    }
});


app.config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptorService');
});

app.run(['authService', function (authService) {
    authService.fillAuthData();
}]);

