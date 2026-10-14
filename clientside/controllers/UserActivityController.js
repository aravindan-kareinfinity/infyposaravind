
var app = angular.module('InfinityLab');

app.controller('UserActivityController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.SearchModes = [{ 'id': 1, 'name': 'Search' }, { 'id': 2, 'name': 'Issue Multiple' }, { 'id': 3, 'name': 'Issue Single' }]
        
        $scope.Entity = {
            "id": 0,
            "employeeid": 0,
            "isreceivable": true,
            "schedulemonth": new Date(),
            "salaryheaderid": 0,
            "configuration": [],
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": new Date(),
            year: new Date().getFullYear(),
            monthid: new Date().getMonth() + 1
        };

        $scope.Advance = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        
    }]);