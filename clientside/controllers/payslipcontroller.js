
var app = angular.module('InfinityLab');

app.controller('PayslipController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService','$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Payslips = [{ 'id': 0, 'name': 'Payslip' },
          { 'id': 1, 'name': 'Season Staff Payslip' },
         { 'id': 2, 'name': 'Final Settlement' },
        { 'id': 3, 'name': 'Hold Salary Settlement' }];

        $scope.employee_payslip_title = 'Form - T';
        $scope.employee_payslip_subtitle = '[See Sub-Rule(6) of Rule 11 of the Tamil Nadu Shops and Establishments Rule, 1948]';

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
            $scope.Company = results;
        });

        $scope.CompanyName = $rootScope.UserInfo.CompanyName;

        $scope.SalaryMonth = new Date();

        $scope.HoldSalaryResult = [];
        $scope.WorkingModes = [{ 'id': 0, 'name': 'ALL' }, { 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.ShowAll = true;
        $scope.MonthList = [{ 'id': '1', 'name': 'JANUARY' }, { 'id': '2', 'name': 'FEBRUARY' },
                           { 'id': '3', 'name': 'MARCH' }, { 'id': '4', 'name': 'APRIL' },
                           { 'id': '5', 'name': 'MAY' }, { 'id': '6', 'name': 'JUNE' },
                           { 'id': '7', 'name': 'JULY' }, { 'id': '8', 'name': 'AUGUST' },
                           { 'id': '9', 'name': 'SEPTEMBER' }, { 'id': '10', 'name': 'OCTOBER' },
                           { 'id': '11', 'name': 'NOVEMBER' }, { 'id': '12', 'name': 'DECEMBER' }];

        $scope.year = new Date().getFullYear();

        $scope.refpaycategorylist = restService.Get('ReferencelistService/FULL-PAYCATEGORY', true).then(function (results) {
            $scope.refpaycategorylist = results;
        });

        $scope.Departments = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.Departments = results;
        });

        $scope.Designations = restService.Get('ReferencelistService/DESIGNATION', true).then(function (results) {
            $scope.Designations = results;
        });

        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
        });

        $scope.PayslipProcess = function () {
            if ($scope.payslipid == 0) {
                $scope.ShowPayslip = true;
                $scope.SeasonStaff = false;
                $scope.ShowFinal = false;
                $scope.ShowHold = false;

                
            }
            else if ($scope.payslipid == 1) {
                $scope.ShowPayslip = false;
                $scope.SeasonStaff = true;
                $scope.ShowFinal = false;
                $scope.ShowHold = false;

            }
            else if ($scope.payslipid == 2) {
                $scope.ShowPayslip = false;
                $scope.SeasonStaff = false;
                $scope.ShowFinal = true;
                $scope.ShowHold = false;


            }
            else if ($scope.payslipid == 3) {
                $scope.ShowPayslip = false;
                $scope.SeasonStaff = false;
                $scope.ShowFinal = false;
                $scope.ShowHold = true;

            }
        }


        $scope.employeeid = 0;
        $scope.payslipmonth = new Date();
        $scope.SearchResult = [];
        $scope.IsSingleSlip = false;
        $scope.Search = function () {
            request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    month: $scope.month, year: $scope.year,
                    locationid: $scope.locationid,
                    floorid: $scope.floorid,
                    companyid: $scope.companyid,
                    workingmode: $scope.workingmode,
                    departmentid: $scope.departmentid
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/Search",
                data: srequest
            }).success(function (result) {
                $scope.IsSingleSlip = false;
                if (result != null) {



                    $.each(result, function (k, v) {
                        v.detailearnings = [];
                        v.detaildeduction = [];
                        v.totalearnings = 0;
                        v.totaldeduction = 0;
                        $.each(v.detail, function (kk, vv) {
                            if (vv.isdeduction) {
                                v.detaildeduction.push(vv);
                                v.totaldeduction += vv.amount;
                            }
                            else {
                                v.detailearnings.push(vv);
                                v.totalearnings += vv.amount;
                            }
                        });
                        if (result.length == 1) {
                            v.detaildeductiondummy = $scope.GetDummyRows(v.detailearnings.length - v.detaildeduction.length);
                        }
                    })

                    $scope.SalaryMonth = new Date(parseInt($scope.year), parseInt($scope.month) - 1, 1);

                    if (result.length == 1 && $scope.employeeid > 0) {
                        $scope.ShowAll = false;
                        $scope.SearchResult = result[0]
                    } else {
                        $scope.ShowAll = true;
                        $scope.SearchResultAll = result;
                    }

                }
                else {
                    $scope.SearchResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }

                if (result.length == 0)
                    $scope.SearchResultMessage = "No Records found!";
                else if (result.length > 1000)
                    $scope.SearchResultMessage = "More than 1000 records available!";
                else
                    $scope.SearchResultMessage = "" + result.length + " Records available";

            }).error(function (result) {
            });
        }

        $scope.SearchDailwages = function () {
            var request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    month: $scope.month,
                    year: $scope.year,
                    departmentid: $scope.departmentid,
                    divisionid: $scope.divisionid,
                    companyid: $scope.companyid,
                    locationid: $scope.locationid,
                    fromdate: $scope.fromdate,
                    todate: $scope.todate
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/GenerateDailywages",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    
                    if (result.length == 1 && $scope.employeeid > 0) {
                        $scope.ShowAll = false;
                        $scope.SearchResult = result[0]
                    } else {
                        $scope.ShowAll = true;
                        $scope.SearchResultAll = result;
                    }

                }
                else {
                    $scope.SearchResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }



            }).error(function (result) {
            });
        }

        $scope.GetAdvancePart = function (items, starting) {
            var list = [];
            for(var i=starting;i<items.length;i=i+3)
            {
                list.push(items[i]);
            }
            return list;
        };

        $scope.GetDummyRows = function (rows) {
            var result = [];
            for (var i = 0; i < rows; i++)
                result.push(i);
            return result;
        }

        $scope.Delete = function () {
            request = {
                "Item": $scope.SearchResult,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/Delete",
                data: srequest
            }).success(function (result) {

            });
        }

        $scope.CompanyChanged = function (id) {
            $.each($scope.Locations, function (k, v) {
                if (v.id == id) {
                    $scope.CompanyName = v.name;
                }
            });
        };


        $scope.SearchHoldSalary = function () {
            $scope.HoldSalaryResult = [];
            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/SearchHoldsalary",
                data: srequest
            }).success(function (result) {
                $scope.HoldSalaryResult = result;
                $scope.employeecount = $scope.HoldSalaryResult.length;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
            }).error(function (result) {
            });
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

        $scope.GetPart = function (arr, index, length) {
            var items = [];
            if (typeof (arr) == 'undefined' || arr == null) return items;
            $.each(arr, function (k, v) {
                if (k % length == index) {
                    items.push(v);
                }
            });
            return items;
        }

        $scope.LoadApplicationSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo != null &&
                $rootScope.UserInfo.appsettings && $rootScope.UserInfo.appsettings != null) {
                if ($rootScope.UserInfo.appsettings.employee_payslip_title &&
                    $rootScope.UserInfo.appsettings.employee_payslip_title != null && 
                    $rootScope.UserInfo.appsettings.employee_payslip_title != '')
                    $scope.employee_payslip_title = $rootScope.UserInfo.appsettings.employee_payslip_title;

                if ($rootScope.UserInfo.appsettings.employee_payslip_subtitle &&
                    $rootScope.UserInfo.appsettings.employee_payslip_subtitle != null &&
                    $rootScope.UserInfo.appsettings.employee_payslip_subtitle != '')
                    $scope.employee_payslip_subtitle = $rootScope.UserInfo.appsettings.employee_payslip_subtitle;
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }

        $timeout(function () {
            $scope.LoadApplicationSetting();
        }, 10);

    }]);