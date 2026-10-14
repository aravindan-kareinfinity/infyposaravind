
var app = angular.module('InfinityLab');

app.controller('PayrollReportController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.ParollProcess = [{ 'id': 0, 'name': 'PF Report' },
            { 'id': 1, 'name': 'ESI Report' },
            { 'id': 2, 'name': 'Summary Report' }
        ];
        $scope.SummaryMode = [{ 'id': 0, 'name': 'Company Summary' },
           { 'id': 1, 'name': 'Employee Summary' }
        ];
        
        $scope.isESI = true;
        $scope.year = new Date().getFullYear();
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });
        $scope.refpaycategorylistearnings = [];
        $scope.refpaycategorylistdeductions = [];

        $scope.totalpfearning = 0;
        $scope.totalepfcontribution = 0;
        $scope.totalepfdifference = 0;
        $scope.totalpensionfund = 0;
        $scope.totalesiearning = 0;
        $scope.totalesi = 0;


        $scope.Generate = function () {
            request = {
                "Item": {
                    companyid: $scope.companyid, isESI: $scope.isESI,
                    month: $scope.month, year: $scope.year
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/ESIPFReport",
                data: srequest
            }).success(function (result) {

                $scope.totalpfearning = 0;
                $scope.totalepfcontribution = 0;
                $scope.totalepfdifference = 0;
                $scope.totalpensionfund = 0;
                $scope.totalesiearning = 0;
                $scope.totalesi = 0;



                if (result != null && result.items.length > 0) {
                    $scope.SearchResult = result.items;
                    $.each(result.items, function (k, v) {
                        $scope.totalpfearning += v.pfearning;
                        $scope.totalepfcontribution += v.pf;
                        $scope.totalepfdifference += v.pfcontribution;
                        $scope.totalpensionfund += v.pfpension;
                        $scope.totalesiearning += v.esiearning;
                        $scope.totalesi += v.esi;
                    })
                }
                else {
                    $scope.SearchResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }

        $scope.refpaycategorylist = restService.Get('ReferencelistService/FULL-PAYCATEGORY', true).then(function (results) {
            $scope.refpaycategorylist = results;
            $.each(results, function (k, v) {
                if (v.additionalinfo.deductsalary)
                    $scope.refpaycategorylistdeductions.push(v);
                else
                    $scope.refpaycategorylistearnings.push(v);
            })
        });

        $scope.SearchRequest = {};

        $scope.getsalaryitem = function (refitem, empitem) {
            var result = empitem.detail.find(function (item) {
                return item.id == refitem.id;
            })
            if (typeof (result) == 'undefined' || result == null)
                return 0;
            else
                return result.amount;
        };

        $scope.MonthList = [{ 'id': '1', 'name': 'JANUARY' }, { 'id': '2', 'name': 'FEBRUARY' },
                           { 'id': '3', 'name': 'MARCH' }, { 'id': '4', 'name': 'APRIL' },
                           { 'id': '5', 'name': 'MAY' }, { 'id': '6', 'name': 'JUNE' },
                           { 'id': '7', 'name': 'JULY' }, { 'id': '8', 'name': 'AUGUST' },
                           { 'id': '9', 'name': 'SEPTEMBER' }, { 'id': '10', 'name': 'OCTOBER' },
                           { 'id': '11', 'name': 'NOVEMBER' }, { 'id': '12', 'name': 'DECEMBER' }];

      
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

        $scope.ReportProcess = function () {
            if ($scope.Payroll.payrolltypeid == 0) {
                $scope.ShowPayroll = true;
                $scope.ShowLeave = false;
                $scope.isESI = false;
            }
            else if ($scope.Payroll.payrolltypeid == 1) {
                $scope.ShowPayroll = false;
                $scope.ShowLeave = true;
                $scope.isESI = true;
            }
        }

        $scope.employeeid = 0;
        $scope.SearchResult = [];


        $scope.exporttoExcel = function (tableid, title) {
            if (tableid == 'pftable') {
                $scope.exportHref = Excel.tableToExcel(tableid, title, 0, 'PF REPORT FOR THE MONTH OF - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            }
            if (tableid == 'esitable') {
                $scope.exportHref = Excel.tableToExcel(tableid, title, 0, 'ESI REPORT FOR THE MONTH OF - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            }
            else {
                $scope.exportHref = Excel.tableToExcel(tableid, title, 0, 'PAYROLL REPORT FOR THE MONTH OF - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            }
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }


        $scope.printDiv = function (id) {

            var title = 'PF REPORT FOR THE MONTH OF - ' + $scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year;
            var otitle = angular.element('#apptitle').context.title;
            var titlele = angular.element('#apptitle').context.title = title;

            var docHead = document.head.outerHTML;

            var table = $('#'+id);
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
        }



        $scope.Export2PF = function () {
            var list = [];
            $.each($scope.SearchResult, function (k, v) {
                var row = [];
                row.push(v.pfuan);
                row.push(v.name);
                row.push($filter('number')(v.earnings, 0).replace(/,/g, ''));
                row.push($filter('number')(v.pfearning, 0).replace(/,/g, ''));
                if (v.pfpension>0)
                    row.push($filter('number')(v.pfearning, 0).replace(/,/g, ''));
                else
                    row.push($filter('number')(0, 0));
                row.push($filter('number')(v.pfearning, 0).replace(/,/g, ''));

                row.push($filter('number')(v.pfcontribution, 0).replace(/,/g, ''));
                row.push($filter('number')(v.pfpension, 0).replace(/,/g, ''));
                row.push($filter('number')(v.pfcontribution - v.pfpension, 0).replace(/,/g, ''));
                row.push('0');
                row.push('0');
                list.push(row.join('#~#'));
            });
            var output = list.join('\r\n');
            var blob = new Blob([output], { type: "text/plain;charset=utf-8" });
            saveAs(blob, "PFReport-" + $scope.SearchRef($scope.month, $scope.MonthList) + "-" + $scope.year+".txt");
        };

        $scope.Summary = function () {
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/SalarySummary",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0)
                    $scope.SummaryResult = result;
                else {
                    $scope.SummaryResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);