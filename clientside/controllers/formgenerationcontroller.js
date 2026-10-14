
var app = angular.module('InfinityLab');

app.controller('FormGenerationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Forms = [{ 'id': 'DownLoadFormP', 'name': 'P Form' }, { 'id': 'DownLoadFormQ', 'name': 'Q Form' }, { 'id': 'DownLoadFormR', 'name': 'R Form' }, { 'id': 'DownLoadFormS', 'name': 'S Form' }, { 'id': 'DownLoadESI', 'name': 'ESI Form' }];

        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });


        $scope.companyid = "";

        $scope.formid = 'DownLoadFormQ';

        $scope.year = new Date().getFullYear();

        $scope.month = 1;


        $scope.ShowNew = true;

        $scope.Edit = function (item) {
           
            $scope.ShowEdit = false;
            $scope.ShowNew = false;
        }


        $scope.SwitchPanel = function () {
            $scope.ShowEdit = true;
            $scope.ShowNew = true;
        }

        $scope.MonthList = [{ 'id': '1', 'name': 'JANUARY' }, { 'id': '2', 'name': 'FEBRUARY' },
                          { 'id': '3', 'name': 'MARCH' }, { 'id': '4', 'name': 'APRIL' },
                          { 'id': '5', 'name': 'MAY' }, { 'id': '6', 'name': 'JUNE' },
                          { 'id': '7', 'name': 'JULY' }, { 'id': '8', 'name': 'AUGUST' },
                          { 'id': '9', 'name': 'SEPTEMBER' }, { 'id': '10', 'name': 'OCTOBER' },
                          { 'id': '11', 'name': 'NOVEMBER' }, { 'id': '12', 'name': 'DECEMBER' }];

        $scope.Generate = function () {
            $rootScope.Download("ExcelExportService/" + $scope.UserInfo.OrganizationID + "/" + $scope.formid + "/" +
                UserDetails.institutionid + '/' + $scope.month + '/' + $scope.year, 'application.pdf', 'application/pdf');
            return;
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "application.pdf";
                link.href = "ExcelExportService/" + $scope.UserInfo.OrganizationID + "/" + $scope.formid + "/" +
                    UserDetails.institutionid + '/' + $scope.month + '/' + $scope.year;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);


        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);