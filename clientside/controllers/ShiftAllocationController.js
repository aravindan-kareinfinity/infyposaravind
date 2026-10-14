
var app = angular.module('InfinityLab');

app.controller('ShiftAllocationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {

        $scope.WorkingModes = [{ 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.SalaryModes = [{ 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];

        $scope.SearchRef = restService.SearchReference;
        $scope.EmployeeList = [];

        $scope.Departments = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.Departments = results;
        });

        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
        });
        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });


        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });

        $scope.MonthList = [{ 'id': '1', 'name': 'JANUARY' }, { 'id': '2', 'name': 'FEBRUARY' },
                           { 'id': '3', 'name': 'MARCH' }, { 'id': '4', 'name': 'APRIL' },
                           { 'id': '5', 'name': 'MAY' }, { 'id': '6', 'name': 'JUNE' },
                           { 'id': '7', 'name': 'JULY' }, { 'id': '8', 'name': 'AUGUST' },
                           { 'id': '9', 'name': 'SEPTEMBER' }, { 'id': '10', 'name': 'OCTOBER' },
                           { 'id': '11', 'name': 'NOVEMBER' }, { 'id': '12', 'name': 'DECEMBER' }];


        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";
        $scope.Entity = {
            "isactive": true,
            "createdby": "",
            "modifiedby": "",
            "version": "",
            "createdon": "",
            "modifiedon": "",
            departmentid: "",
            floorid: "",
            sectionid:"",
            companyid: "",
            monthid: "",
            year: new Date().getFullYear()

        };


        $scope.ShiftAllocation = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.Delete = function () {

            request = {
                "Item": $scope.ShiftAllocation,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ShiftAllocationService/Delete",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = [];
            }).error(function (result) {
            });
        };

        $scope.Generate = function () {


            if ($scope.ShiftAllocation.regenerate)
            {
                var reason = '';
                if ($scope.ShiftAllocation.floorid > 0)
                    reason = 'floor';
                if ($scope.ShiftAllocation.departmentid > 0)
                    reason = 'department';
                if ($scope.ShiftAllocation.companyid > 0)
                    reason = 'company';
                if ($scope.ShiftAllocation.salarymode > 0)
                    reason = 'salary mode';
                if ($scope.ShiftAllocation.workingmode > 0)
                    reason = 'working mode';
                if(reason != '')
                {
                    $rootScope.Toaster.Error("Please remove "+reason+', to generate the shift');
                    return;
                }
            }

            request = {
                "Item": $scope.ShiftAllocation,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ShiftAllocationService/Generate",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;

                var dayList = [];
                $.each(result.days,
                    function (k, v) {
                        dayList.push(new Date(v));
                    }
                );
                $scope.dayList = dayList;
            }).error(function (result) {
            });
        }

        $scope.HasChecked = function (employee, date) {
            var exist = employee.shifts.find(function (item) {
                return date == item.date;
            })
            return (typeof (exist) != 'undefined' && exist != null);
        }

        $scope.printDiv = function (div, title) {

            var titlele = angular.element('#apptitle').context.title = title;

            var docHead = document.head.outerHTML;
            var printContents = document.getElementById(div).outerHTML;
            var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            var newWin = window.open("", "_blank", winAttr);
            var writeDoc = newWin.document;
            writeDoc.open();
            writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            writeDoc.close();
            newWin.focus();
        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);