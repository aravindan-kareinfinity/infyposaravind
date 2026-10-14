
var app = angular.module('InfinityLab');

app.controller('TraceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.SearchResult = [];

        $scope.OrganizationSelected = false;
        $scope.SearchOrganization = function () {
            $scope.OrganizationSelected = false;
            $scope.OrganizationName = "";
            restService.Get('OrganisationService/Search/' + $scope.organizationcode, true).then(function (results) {
                $scope.CurrentOrganization = results;
                if ($scope.CurrentOrganization != null) {
                    $scope.OrganizationSelected = true;
                    $scope.OrganizationName = $scope.CurrentOrganization.name;

                    restService.Get('TraceService/Log/null/' + $scope.CurrentOrganization.id).then(function (results) {
                        $scope.Trace.enable = results;
                    });
                }
            });
        }

        $scope.reftypelist = [{ 'code': 'INFO', 'value': 'Info' },
            { 'code': 'ERROR', 'value': 'Error' }, { 'code': 'REQUEST', 'value': 'Request' }, { 'code': 'RESPONSE', 'value': 'Response' }];

        $scope.Entity = {
            "type": "",
            "loggedat": new Date(),
            "methodName": "",
            "requesturl": "",
            "info": ""
        }
        $scope.Trace = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.Setconfig = function (value)
        {
            $scope.Trace.enable = value;
            var organizationid = $scope.OrganizationSelected && $scope.UserInfo.RootUser ? $scope.CurrentOrganization.id : 0;
            var url = 'TraceService/Log/' + $scope.Trace.enable + '/' + organizationid;
            restService.Get(url).then(function (results) {

            });
        }


        restService.Get('TraceService/Log').then(function (results) {
            $scope.Trace.enable = results;
        });

        $scope.Search = function () {
            $scope.Trace.organizationid = $scope.OrganizationSelected && $scope.UserInfo.RootUser ? $scope.CurrentOrganization.id : 0,
            request = {
                "Item": $scope.Trace,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "TraceService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0)
                    $scope.SearchResult = result;
                else
                    $scope.SearchResult = [];
            }).error(function (result) {
            });

        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

}]);