
var app = angular.module('InfinityLab');

app.controller('PayrollController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.ParollProcess = [{ 'id': 0, 'name': 'Payroll Process' },
        { 'id': 8, 'name': 'Payroll Uploader' },
        { 'id': 9, 'name': 'Bank Report' },
        //{ 'id': 1, 'name': 'Leave Process' },
        //{ 'id': 2, 'name': 'Incentive Process' },
        //{ 'id': 3, 'name': 'Bonus Process' },
        //{ 'id': 4, 'name': 'Beta Process' },
        //{ 'id': 5, 'name': 'OverTime Process' },
        //{ 'id': 6, 'name': 'OT-Beta-Leave' },
        { 'id': 7, 'name': 'Daily Wages' }];
        $scope.totalearning = 0;
        $scope.totaldedution = 0;
        $scope.totalnetsalary = 0;
        $scope.totalsundaysalary = 0;
        $scope.totalholidaysalary = 0;
        $scope.totalleavesalary = 0;
        $scope.totalfullbeta = 0;
        $scope.totallatebeta = 0;
        $scope.totalhalfbeta = 0;
        $scope.totalbeta = 0;
        $scope.ZeroBata = "Zero-Bata";

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });


        $scope.SpecialPayment = restService.Get('ReferencelistService/FULL-SPECIALPAYMENT', true).then(function (results) {
            $scope.SpecialPayment = results;
        });


        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.WorkingModes = [{ 'id': 0, 'name': 'ALL' }, { 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.SalaryModes = [{ 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];
        $scope.FilterModes = [{ 'id': 0, 'name': 'All Employees' }, { 'id': 1, 'name': 'Payable Employees' }, { 'id': 2, 'name': 'Left Employees' }, { 'id': 3, 'name': 'With held Employees' }];
        $scope.filtermode = 1;
        $scope.refpaycategorylistearnings = [];
        $scope.refpaycategorylistdeductions = [];

        $scope.refpaycategorylist = restService.Get('ReferencelistService/FULL-PAYCATEGORY', true).then(function (results) {
            $scope.refpaycategorylist = results;
            $.each(results, function (k, v) {
                if (v.additionalinfo.deductsalary)
                    $scope.refpaycategorylistdeductions.push(v);
                else
                    $scope.refpaycategorylistearnings.push(v);
            })
        });

        $scope.getsalarydefault = function (refitem, empitem) {
            var result = empitem.detail.find(function (item) {
                return item.id == refitem.id;
            })
            if (typeof (result) == 'undefined' || result == null)
                return 0;
            else
                return result.value;
        };

        $scope.getsalaryitem = function (refitem, empitem) {
            var result = empitem.detail.find(function (item) {
                return item.id == refitem.id;
            })
            if (typeof (result) == 'undefined' || result == null)
                return 0;
            else
                return result.amount;
        };

        $scope.MonthList = [{ 'id': 1, 'name': 'JANUARY' }, { 'id': 2, 'name': 'FEBRUARY' },
        { 'id': 3, 'name': 'MARCH' }, { 'id': 4, 'name': 'APRIL' },
        { 'id': 5, 'name': 'MAY' }, { 'id': 6, 'name': 'JUNE' },
        { 'id': 7, 'name': 'JULY' }, { 'id': 8, 'name': 'AUGUST' },
        { 'id': 9, 'name': 'SEPTEMBER' }, { 'id': 10, 'name': 'OCTOBER' },
        { 'id': 11, 'name': 'NOVEMBER' }, { 'id': 12, 'name': 'DECEMBER' }];

        $scope.Departments = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.Departments = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });

        $scope.fromyear = new Date().getFullYear();
        $scope.toyear = new Date().getFullYear();
        $scope.year = new Date().getFullYear();

        $scope.ConfigChanged = function () {
            if ($scope.Payroll.payrolltypeid > 2) {
                var config = $scope.SpecialPayment.find(function (item) {
                    return item.id == $scope.configid;
                });
                if (config != null)
                    $scope.attributes = config.additionalinfo;
            }
            else {
                $scope.attributes = null;
            }
        };

        $scope.PayrollProce = function () {
            $scope.ShowPayroll = $scope.Payroll.payrolltypeid == 0;
            $scope.ShowLeave = $scope.Payroll.payrolltypeid == 1;
            $scope.ShowIncentive = $scope.Payroll.payrolltypeid == 2;
            $scope.ShowBonus = $scope.Payroll.payrolltypeid == 3;
            $scope.ShowBeta = $scope.Payroll.payrolltypeid == 4;
            $scope.ShowOvertime = $scope.Payroll.payrolltypeid == 5;
            $scope.ShowSummary = $scope.Payroll.payrolltypeid == 6;
            $scope.DailyWages = $scope.Payroll.payrolltypeid == 7;
            $scope.ShowPayrollUploader = $scope.Payroll.payrolltypeid == 8;
            $scope.BankReport = $scope.Payroll.payrolltypeid == 9;
        }

        $scope.employeeid = 0;
        $scope.payslipmonth = new Date();
        $scope.SearchResult = [];
        $scope.LeaveResult = [];
        $scope.ShowCheckList = false;
        $scope.SearchCheckList = function () {
            $scope.ShowCheckList = true;
            request = {
                "Item": {
                    employeeid: $scope.employeeid, institutionid: UserDetails.institutionid,
                    month: parseInt($scope.month), year: $scope.year, floorid: $scope.floorid,
                    filtermode: $scope.filtermode, companyid: $scope.companyid,
                    paydate: $scope.paydate, salarymode: $scope.salarymode, workingmode: $scope.workingmode
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/Search",
                data: srequest
            }).success(function (result) {

                $scope.totalearning = 0;
                $scope.totaldedution = 0;
                $scope.totalnetsalary = 0;

                if (result != null && result.length > 0) {
                    $scope.ShowCheckList = true;
                    $scope.SearchCheckResult = result;
                    $.each(result, function (k, v) {
                        $scope.totalearning += v.earnings;
                        $scope.totaldedution += v.deductions;
                        $scope.totalnetsalary += v.netsalary;
                    })
                }
                else {
                    $scope.ShowCheckList = false;
                    $scope.SearchCheckResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }

        $scope.BankResult = [];
        $scope.SearchBankReport = function () {           
            request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    month: parseInt($scope.month),
                    year: $scope.year, 
                    companyid: $scope.companyid
                    //paydate: $scope.paydate,
                    //salarymode: $scope.salarymode,
                    //workingmode: $scope.workingmode
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/Searchbank",
                data: srequest
            }).success(function (result) {

             
                $scope.totalnetsalary = 0;

                if (result != null && result.length > 0) {

                    $scope.BankResult = result;
                    $.each(result, function (k, v) {
                      
                        $scope.totalnetsalary += v.netsalary;
                    })
                }
                else {
                    $scope.BankResult = [];
                    $rootScope.Toaster.Info("select the month");
                }
            }).error(function (result) {
            });
        }


        $scope.Search = function () {
            $scope.ShowCheckList = false;
            request = {
                "Item": {
                    employeeid: $scope.employeeid,institutionid: UserDetails.institutionid,month: parseInt($scope.month),
                    year: $scope.year,floorid: $scope.floorid,filtermode: $scope.filtermode,companyid: $scope.companyid,
                    paydate: $scope.paydate,salarymode: $scope.salarymode,workingmode: $scope.workingmode,
                    departmentid: $scope.departmentid,locationid: $scope.locationid
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/Search",
                data: srequest
            }).success(function (result) {

                $scope.totalearning = 0;
                $scope.totaldedution = 0;
                $scope.totalnetsalary = 0;

                if (result != null && result.length > 0) {
                    $scope.SearchResult = result;

                    $.each(result, function (k, v) {
                        $scope.totalearning += v.earnings;
                        $scope.totaldedution += v.deductions;
                        $scope.totalnetsalary += v.netsalary;
                    })

                }
                else {
                    $scope.SearchResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }

        $scope.GenerateLeave = function () {
            request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    month: parseInt($scope.month),
                    year: $scope.year,
                    departmentid: $scope.departmentid,
                    locationid: $scope.locationid,
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentivesService/GenerateLeave",
                data: srequest
            }).success(function (result) {
                $scope.totalsundaysalary = 0;
                $scope.totalholidaysalary = 0;
                $scope.totalleavesalary = 0;

                if (result != null && result.length > 0) {
                    $scope.LeaveResult = result;
                    $.each(result, function (k, v) {
                        $scope.totalsundaysalary += v.sundaysalary;
                        $scope.totalholidaysalary += v.holidaysalary;
                        $scope.totalleavesalary += v.sundaysalary + v.holidaysalary;
                    })
                }
                $scope.LeaveResult = result;

            }).error(function (result) {
            });
        }

        $scope.GenerateIncentive = function () {
            request = {
                "Item": {
                    departmentid: $scope.departmentid,
                    locationid: $scope.locationid,
                    month: $scope.month,
                    year: $scope.year,
                    companyid: $scope.companyid,
                    includesales: $scope.includesales
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentivesService/GenerateIncentive",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0)
                    $scope.IncentiveResult = result;
                else {
                    $scope.IncentiveResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }

        $scope.GenerateBonus = function () {
            request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    month: $scope.month, year: $scope.year,
                    departmentid: $scope.departmentid,
                    locationid: $scope.locationid,
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentivesService/GenerateBonus",
                data: srequest
            }).success(function (result) {
                $scope.BonusResult = result;

            }).error(function (result) {
            });
        }

        $scope.GenerateBeta = function () {
            request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    month: $scope.month,
                    year: $scope.year,
                    departmentid: $scope.departmentid,
                    locationid: $scope.locationid,
                    attributes: $scope.attributes
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentivesService/GenerateBeta",
                data: srequest
            }).success(function (result) {
                $scope.totalfullbeta = 0;
                $scope.totallatebeta = 0;
                $scope.totalhalfbeta = 0;
                $scope.totalbeta = 0;
                $scope.totalminbeta = 0;
                $scope.ZeroBata = "Zero-Bata";
                if (typeof ($scope.attributes) != 'undefined' && $scope.attributes != null && $scope.attributes.reportconfig != null) {
                    if ($scope.attributes.reportconfig.Beta_Female_Minimum > 0 || $scope.attributes.reportconfig.Beta_Male_Minimum)
                        $scope.ZeroBata = 'Min-Bata';
                }

                if (result != null && result.length > 0) {
                    $scope.BetaResult = result;
                    $.each(result, function (k, v) {
                        $scope.totalfullbeta += v.FullBata;
                        $scope.totallatebeta += v.LateBata;
                        $scope.totalhalfbeta += v.HalfBata;
                        $scope.totalminbeta += v.MinBata;
                        $scope.totalbeta += v.Beta;
                    })
                }
                $scope.BetaResult = result;

            }).error(function (result) {
            });
        }

        $scope.GenerateOT = function () {
            request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    month: $scope.month,
                    year: $scope.year,
                    departmentid: $scope.departmentid,
                    locationid: $scope.locationid,
                    attributes: $scope.attributes
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentivesService/GenerateOT",
                data: srequest
            }).success(function (result) {
                $scope.OTResult = result;

            }).error(function (result) {
            });
        }


        $scope.SearchDailyWages = function () {
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
                $scope.DailyWagesResult = result;

            }).error(function (result) {
            });
        };

        $scope.SearchIncentives = function (type) {
            request = {
                "Item": {
                    employeeid: $scope.employeeid,
                    month: $scope.month,
                    year: $scope.year,
                    departmentid: $scope.departmentid,
                    locationid: $scope.locationid,
                    datatype: type
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentivesService/Search",
                data: srequest
            }).success(function (result) {
                switch (type) {
                    case 0:
                        $scope.SummaryResult = result;
                        break;
                    case 1:
                        $scope.IncentiveResult = result;
                        break;
                    case 2:
                        $scope.LeaveResult = result;
                        break;
                    case 3:
                        $scope.totalfullbeta = 0;
                        $scope.totallatebeta = 0;
                        $scope.totalhalfbeta = 0;
                        $scope.totalbeta = 0;
                        $scope.totalminbeta = 0;


                        $.each(result, function (k, v) {
                            $scope.totalfullbeta += v.FullBata;
                            $scope.totallatebeta += v.LateBata;
                            $scope.totalhalfbeta += v.HalfBata;
                            $scope.totalminbeta += v.MinBata;
                            $scope.totalbeta += v.Beta;
                        })

                        $scope.BetaResult = result;
                        break;
                    case 4:
                        $scope.OTResult = result;
                        break;
                    case 5:
                        $scope.BetaResult = result;
                        break;
                }

            }).error(function (result) {
            });
        }



        $scope.exporttoExcel = function (tableid, title, removeactioncolumn) {
            

            if (tableid == 'payroll' || tableid == 'payrollchecklist')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALARYLIST FOR THE MONTH - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            else if (tableid == 'leave')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'LEAVE SALARY FOR THE MONTH - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            else if (tableid == 'incentive')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'INCENTIVE REPORT FOR THE MONTH - ' + $scope.SearchRef($scope.frommonth, $scope.MonthList) + " - " + $scope.SearchRef($scope.tomonth, $scope.MonthList));
            else if (tableid == 'bonus')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'BONUS REPORT FOR THE MONTH - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            else if (tableid == 'beta')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'BETA REPORT FOR THE MONTH - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            else if (tableid == 'ot')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'OVERTIME REPORT FOR THE MONTH - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            else if (tableid == 'uploadsalary')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALARY REPORT FOR THE MONTH - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            else if (tableid == 'BankReport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'BANK REPORT - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            else
                return;
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }


        $scope.printDiv = function () {

            var title = 'Salary - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year;
            var otitle = angular.element('#apptitle').context.title;
            var titlele = angular.element('#apptitle').context.title = title;

            var docHead = document.head.outerHTML;

            var table = $('#payroll');
            var ctable = table.clone();

            ctable.find('INPUT').remove();

            ctable.find('THEAD TR.hidden-header').remove();

            ctable.find('THEAD TR TH').css('text-align', 'center');

            ctable.find('.th-inner').removeClass();
            ctable.removeClass();
            ctable.addClass('fixed_table');
            ctable.addClass('table-bordered');

            ctable.find('THEAD').prepend("<tr><th style = 'text-align:center' colspan='" + ctable.find('THEAD TR TH').length + "'>" + title + "</th></tr>");
            ctable.find('THEAD').prepend("<tr><th style = 'text-align:center' colspan='" + ctable.find('THEAD TR TH').length + "'>" + $rootScope.UserInfo.CompanyName + "</th></tr>");

            var width = 800, height = 900;
            var printContents = ctable[0].outerHTML;
            var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=" + width + ", height=" + height + ", resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            var newWin = window.open("", "_blank", winAttr);
            var writeDoc = newWin.document;
            writeDoc.open();
            writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            writeDoc.close();
            newWin.focus();

            angular.element('#apptitle').context.title = otitle;

            //var otitle = angular.element('#apptitle').context.title;
            //var titlele = angular.element('#apptitle').context.title = " Advance Report for ";

            //var docHead = document.head.outerHTML;
            //var printContents = document.getElementById(div).outerHTML;
            //var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            //var newWin = window.open("", "_blank", winAttr);
            //var writeDoc = newWin.document;
            //writeDoc.open();
            //writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            //writeDoc.close();
            //newWin.focus();

            //angular.element('#apptitle').context.title = otitle;
        }

        $scope.printDivs = function () {

            var title = 'Bank Report - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year;
            var otitle = angular.element('#apptitle').context.title;
            var titlele = angular.element('#apptitle').context.title = title;

            var docHead = document.head.outerHTML;

            var table = $('#BankReport');
            var ctable = table.clone();

            ctable.find('INPUT').remove();

            ctable.find('THEAD TR.hidden-header').remove();

            ctable.find('THEAD TR TH').css('text-align', 'center');

            ctable.find('.th-inner').removeClass();
            ctable.removeClass();
            ctable.addClass('fixed_table');
            ctable.addClass('table-bordered');

            ctable.find('THEAD').prepend("<tr><th style = 'text-align:center' colspan='" + ctable.find('THEAD TR TH').length + "'>" + title + "</th></tr>");
            ctable.find('THEAD').prepend("<tr><th style = 'text-align:center' colspan='" + ctable.find('THEAD TR TH').length + "'>" + $rootScope.UserInfo.CompanyName + "</th></tr>");

            var width = 800, height = 900;
            var printContents = ctable[0].outerHTML;
            var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=" + width + ", height=" + height + ", resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            var newWin = window.open("", "_blank", winAttr);
            var writeDoc = newWin.document;
            writeDoc.open();
            writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            writeDoc.close();
            newWin.focus();

            angular.element('#apptitle').context.title = otitle;

            //var otitle = angular.element('#apptitle').context.title;
            //var titlele = angular.element('#apptitle').context.title = " Advance Report for ";

            //var docHead = document.head.outerHTML;
            //var printContents = document.getElementById(div).outerHTML;
            //var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

            //var newWin = window.open("", "_blank", winAttr);
            //var writeDoc = newWin.document;
            //writeDoc.open();
            //writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
            //writeDoc.close();
            //newWin.focus();

            //angular.element('#apptitle').context.title = otitle;
        }
        $scope.ShowDayInfo = function (additionalinfo, name) {
            additionalinfo.name = name;
            var dialog = ngDialog.open({
                template: 'ShowAdditionalInfo',
                data: {
                    'additionalinfo': additionalinfo
                },
                width: '300px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.additionalinfo = $scope.ngDialogData.additionalinfo;
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                }
            });
        }

        $scope.UploadResultHeader = { 'id': 0, 'name': '', 'isdeduction': false };
        $scope.UploadResultHeaderChanged = function (list, id) {
            $scope.UploadResultHeader = list.find(function (v) {
                return v.id == id;
            });
            if (typeof ($scope.UploadResultHeader) == 'undefined') {
                $scope.UploadResultHeader = {
                    'id': 0, 'name': '', 'isdeduction': false
                };
            }
            else if ($scope.SearchUploadResult && $scope.SearchUploadResult.length > 0) {
                $scope.UploadResultHeader.isdeduction = $scope.UploadResultHeader.additionalinfo.deductsalary;
                $scope.totalchanges = 0;

                $scope.totalearning = 0;
                $scope.totaldedution = 0;
                $scope.totalnetsalary = 0;

                $.each($scope.SearchUploadResult, function (k, empitem) {
                    var result = empitem.detail.find(function (item) {
                        return item.id == $scope.UploadResultHeader.id;
                    })
                    if (typeof (result) == 'undefined' || result == null)
                        empitem.updatevalue = 0;
                    else
                        empitem.updatevalue = result.amount;
                    empitem.updateprevalue = empitem.updatevalue;
                    $scope.totalchanges += empitem.updatevalue;
                    empitem.tearnings = empitem.earnings;
                    empitem.tdeductions = empitem.deductions;
                    empitem.tnetsalary = empitem.netsalary;
                    $scope.totalearning += empitem.earnings;
                    $scope.totaldedution += empitem.deductions;
                    $scope.totalnetsalary += empitem.netsalary;
                });
            }
        }

        $scope.SaveSalaryChanges = function () {

            var request = {
                "Items": [],
                "SecurityToken": "String content"
            }

            $.each($scope.SearchUploadResult, function (k, empitem) {
                request.Items.push({ 'id': empitem.id, 'headerid': $scope.UploadResultHeader.id, 'amount': empitem.updatevalue });
            });

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/UpdateDetail",
                data: srequest
            }).success(function (result) {

            });
        }

        $scope.itemvalueupdated = function (empitem, lastvalue) {
            $scope.totalchanges -= lastvalue;
            $scope.totalearning -= empitem.tearnings;
            $scope.totaldedution -= empitem.tdeductions;
            $scope.totalnetsalary -= empitem.tnetsalary;
            if ($scope.UploadResultHeader.isdeduction) {
                empitem.tdeductions = empitem.deductions + empitem.updatevalue;
                empitem.tnetsalary = empitem.netsalary - empitem.updatevalue;
            }
            else {
                empitem.tearnings = empitem.earnings + empitem.updatevalue;
                empitem.tnetsalary = empitem.netsalary + empitem.updatevalue;
            }
            $scope.totalchanges += empitem.updatevalue;
            $scope.totalearning += empitem.tearnings;
            $scope.totaldedution += empitem.tdeductions;
            $scope.totalnetsalary += empitem.tnetsalary;
        }

        $scope.uploaderfilecompleted = function (value) {
            $scope.uploaderfile = null;

            var employecodeindex = -1;
            var headervalue = -1;
            $.each(value.columns, function (k, v) {
                if (v == $scope.UploadResultHeader.name) {
                    headervalue = k;
                }
                if (v == 'Code') {
                    employecodeindex = k;
                }
            });

            if (employecodeindex >= 0 && employecodeindex >= 0) {
                $.each(value.rows, function (k, v) {

                    var empitem = $scope.SearchUploadResult.find(function (item) {
                        return item.employeeno == v.values[employecodeindex];
                    });

                    if (typeof (empitem) != 'undefined' && empitem != null) {
                        var val = parseInt(v.values[headervalue]);
                        if (val != null && !isNaN(val))
                            empitem.updatevalue = val;
                    }

                });
            }
            else {
                $rootScope.Toaster.Info("Excel doesn't have Code or " + $scope.UploadResultHeader.name + ' columns');
            }
        }

        $scope.Searchforploader = function () {
            var request = {
                "Item": {
                    employeeid: $scope.employeeid, locationid: $scope.locationid,
                    month: parseInt($scope.month), year: $scope.year, workingmode: $scope.workingmode,
                    departmentid: $scope.departmentid, floorid: $scope.floorid, companyid: $scope.companyid
                },
                "SecurityToken": "String content"
            };

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/Search",
                data: srequest
            }).success(function (result) {

                $scope.totalearning = 0;
                $scope.totaldedution = 0;
                $scope.totalnetsalary = 0;

                if (result != null && result.length > 0) {
                    $scope.SearchUploadResult = result;

                    $.each(result, function (k, v) {
                        $scope.totalearning += v.earnings;
                        $scope.totaldedution += v.deductions;
                        $scope.totalnetsalary += v.netsalary;
                        v.tearnings = v.earnings;
                        v.tdeductions = v.deductions;
                        v.tnetsalary = v.netsalary;
                    });
                }
                else {
                    $scope.SearchUploadResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }

        $scope.Generate = function () {
            $scope.ShowCheckList = false;
            request = {
                "Item": {
                    employeeid: $scope.employeeid, institutionid: UserDetails.institutionid, save: true,
                    month: $scope.month, year: $scope.year, paydate: $scope.paydate, salarymode: $scope.salarymode, workingmode: $scope.workingmode,
                    fromdate : $scope.fromdate, todate : $scope.todate
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/Generate",
                data: srequest
            }).success(function (result) {
                $scope.totalearning = 0;
                $scope.totaldedution = 0;
                $scope.totalnetsalary = 0;

                if (result != null && result.length > 0) {
                    $scope.SearchResult = result;
                    $.each(result, function (k, v) {
                        $scope.totalearning += v.earnings;
                        $scope.totaldedution += v.deductions;
                        $scope.totalnetsalary += v.netsalary;
                    })
                }
                else {
                    $scope.SearchResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }

        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);