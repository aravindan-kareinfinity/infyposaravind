
var app = angular.module('InfinityLab');

app.controller('HRReportController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'Excel','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, Excel, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.reporttype = "GENERAL";
        $scope.ReportType = [
            { 'code': 'GENERAL', 'name': 'Employees Report' },
            { 'code': 'BIRTHDAY', 'name': 'Birth day Report' },
            { 'code': 'MARIAGE', 'name': 'Marriage day Report' },
            { 'code': 'JOINDDAY', 'name': 'Joining day Report' },
            { 'code': 'RELIEVING', 'name': 'Relieving day Report' },
            { 'code': 'SERVICECOMPLETION', 'name': 'Service Completion Report' }];

        $scope.EmployeeModes = [{ 'code': 'ALL', 'name': 'All Employees' },
            { 'code': 'LEFT', 'name': 'Resigned Employees' },
            { 'code': 'ACTIVE', 'name': 'Working Employees' }];

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });


        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });


        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });



        $scope.SearchCondition = {
            fromdate: new Date(),
            todate: new Date(),
            employeeid: 0,
            employeecode: '',
            ismonth: false,
            ismarriageday: false,
            isrelievingday: false
        };

        $scope.exporttoExcel = function (infygrid) {
            var filename = 'report.xls';
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = $scope.SearchRef($scope.SearchCondition.reporttype, $scope.$scope.ReportType);
            
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }



        $scope.SearchHRReports = function () {
            
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/SearchHRReports",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    $scope.SearchResult = result;
                    $scope.ConfigGrid();
                }
            }).error(function (result) {
            });
        }

        $scope.igrid = null;
        $scope.ConfigGrid = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddSimpleColumn('employeecode', 'Code', 80);
            igrid.AddSimpleColumn('employeename', 'Name', 200);
            igrid.AddCustomColumn('gender', 'Gender', 80, function (item, col) {
                if (item.gender != null  && item.gender == "M")
                    return "Male";
                return "Female";
            });
            igrid.AddSimpleColumn('company', 'Company', 100);
            igrid.AddSimpleColumn('department', 'Department', 100);
            igrid.AddSimpleColumn('locationname', 'Location', 100);
            igrid.AddSimpleColumn('sectionname', 'Section', 100);
            igrid.AddSimpleColumn('floor', 'Floor', 100);

            igrid.AddSimpleColumn('religion', 'Religion', 100);
            igrid.AddSimpleColumn('caste', 'Caste', 100);
            igrid.AddSimpleColumn('community', 'Community', 100);

            igrid.AddCustomColumn('dateofbirth', 'Date Of Join', 80, function (item, col) {
                if ($filter('date')(item.dateofbirth, 'dd-MM-yyyy') == '01-01-0001') return '';
                return $filter('date')(item.dateofbirth, 'dd-MM-yyyy');
            });
            
            igrid.AddCustomColumn('dateofmarriage', 'Mariage Date', 80, function (item, col) {
                if ($filter('date')(item.dateofmarriage, 'dd-MM-yyyy') == '01-01-0001') return '';
                return $filter('date')(item.dateofmarriage, 'dd-MM-yyyy');
            });

            igrid.AddCustomColumn('dateofrelieving', 'Reliving Date', 80, function (item, col) {
                if ($filter('date')(item.dateofrelieving, 'dd-MM-yyyy') == '01-01-0001') return '';
                return $filter('date')(item.dateofrelieving, 'dd-MM-yyyy');
            });

            if ($scope.SearchCondition.reporttype == 'SERVICECOMPLETION')
                igrid.AddSimpleColumn('service', 'service', 200);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

 }]);