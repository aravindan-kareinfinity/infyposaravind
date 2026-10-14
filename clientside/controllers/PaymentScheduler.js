
var app = angular.module('InfinityLab');

app.controller('PaymentSchedulerController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Modes = [{ 'id': '0', 'name': 'All' }, { 'id': '1', 'name': 'Loan Paid' },
        { 'id': '2', 'name': 'EMI Receivable' }, { 'id': '3', 'name': 'Advance Paid' },
        { 'id': '4', 'name': 'Advance Receivble' },
            { 'id': '5', 'name': 'Pending Bills' }];
        $scope.SearchModes = [{ 'id': 1, 'name': 'Issue Single' }, { 'id': 2, 'name': 'Issue Multiple' }, { 'id': 3, 'name': 'Search' }]
        $scope.SearchModes.id = 3;

        $scope.MonthList = [{ 'id': 1, 'name': 'JANUARY' }, { 'id': 2, 'name': 'FEBRUARY' },
        { 'id': 3, 'name': 'MARCH' }, { 'id': 4, 'name': 'APRIL' },
        { 'id': 5, 'name': 'MAY' }, { 'id': 6, 'name': 'JUNE' },
        { 'id': 7, 'name': 'JULY' }, { 'id': 8, 'name': 'AUGUST' },
        { 'id': 9, 'name': 'SEPTEMBER' }, { 'id': 10, 'name': 'OCTOBER' },
        { 'id': 11, 'name': 'NOVEMBER' }, { 'id': 12, 'name': 'DECEMBER' }];
        $scope.searchmode = 1;
        $scope.mode == 0;
        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });

        $scope.Departments = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.Departments = results;
        });

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

        $scope.refpaytypelist = restService.Get('ReferencelistService/PAYCATEGORY', true).then(function (results) {
            $scope.refpaytypelist = results;

            $timeout(function () {
                $scope.refpaytypelist.find(function (item) {
                    if (item.name == 'ADVANCE' || item.code == 'ADVANCE' || item.name == 'ADV') {
                        $scope.Advance.salaryheaderid = item.id;
                        $scope.$digest();
                    }
                });
            }, 20);

        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.WorkingModes = [{ 'id': 0, 'name': 'ALL' }, { 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.SalaryModes = [{ 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];

        $scope.payslipmonth = new Date();

        $scope.Active = true;

        $scope.MonthList = [{ 'id': '1', 'name': 'JANUARY' }, { 'id': '2', 'name': 'FEBRUARY' },
        { 'id': '3', 'name': 'MARCH' }, { 'id': '4', 'name': 'APRIL' },
        { 'id': '5', 'name': 'MAY' }, { 'id': '6', 'name': 'JUNE' },
        { 'id': '7', 'name': 'JULY' }, { 'id': '8', 'name': 'AUGUST' },
        { 'id': '9', 'name': 'SEPTEMBER' }, { 'id': '10', 'name': 'OCTOBER' },
        { 'id': '11', 'name': 'NOVEMBER' }, { 'id': '12', 'name': 'DECEMBER' }];



        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";
        $scope.SelectedCount = 0;
        $scope.SelecteAll = function (list, selected) {
            $.each(list, function (k, v) {
                v.selected = selected;
            });
            $scope.SelectedCount = selected ? list.length : 0;
        };

        $scope.CalculateSelected = function (list) {
            $scope.SelectedCount = 0;
            $.each(list, function (k, v) {
                if (v.selected)
                    $scope.SelectedCount += 1;
            });
            $scope.selecteall = (list.length == $scope.SelectedCount);
        }

        $scope.Search = function () {
            $scope.ErrorMessage = '';
            if ($scope.Advance.salaryheaderid == null || $scope.Advance.salaryheaderid == "") {
                $rootScope.Toaster.Error("Please Select header name.!");
                return;
            }


            if ($scope.Advance.monthid == null || $scope.Advance.monthid == "") {
                $rootScope.Toaster.Error("Please Select the month.!");
                return;
            }
            if ($scope.Advance.year == null || $scope.Advance.year == "") {
                $rootScope.Toaster.Error("Please Select the year.!");
                return;
            }


            request = {
                "Item": {
                    'showallemployee': $scope.searchmode == 1 ? false : true,
                    salaryheaderid: $scope.Advance.salaryheaderid,
                    searchmonth: $scope.Advance.monthid,
                    searchyear: $scope.Advance.year,
                    departmentid: $scope.departmentid,
                    employeeid: $scope.employeeid,
                    floorid: $scope.floorid,
                    divisionid: $scope.divisionid,
                    salarymode: $scope.salarymode,
                    showhasleft: $scope.showhasleft,
                    workingmode: $scope.workingmode,
                    schedulemonth: $scope.salarymonth,
                    searchmode: $scope.mode == 0 ? 1 : 0,
                    mode: $scope.mode
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryscheduleService/Search",
                data: srequest
            }).success(function (result) {

                if ($scope.searchmode == 1) {
                    $scope.SearchResult = result;
                    $scope.Total = 0;
                    $.each(result, function (k, v) {
                        $scope.Total += v.amount;
                        if (v.amount == 0)
                            v.amount = '';
                    });
                }
                else {
                    $scope.EntryList = result;
                    $scope.CurrentTotal = 0;
                    $scope.Total = 0;
                    $.each(result, function (k, v) {
                        $scope.CurrentTotal += v.currentamount;
                        if (v.amount == 0)
                            v.amount = '';
                    });
                }
            }).error(function (result) {
                $rootScope.Toaster.Error("Please Select Properly");
            });
        }

        $scope.advanceChanged = function (item, newamt, oldamt) {
            $scope.Total -= $rootScope.GetNumber(oldamt);
            $scope.Total += $rootScope.GetNumber(newamt);
        }

        $scope.Delete = function () {
            var items = [];
            $.each($scope.SearchResult, function (k, v) {
                if (v.selected)
                    items.push(v);
            });
            var request = {
                "Items": items,
                "SecurityToken": "String content"
            };

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EmployeesalaryscheduleService/DeleteAll",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $scope.ErrorMessage = "Successfully " + $scope.SelectedCount + " deleted";
                    for (var index = $scope.SearchResult.length - 1; index >= 0; index--) {
                        if ($scope.SearchResult[index].selected)
                            $scope.SearchResult.splice(index);
                    }
                    $scope.SelectedCount = 0;
                }

            }).error(function (result) {

            });
        }

        $scope.ManageSearchItem = function (action) {
            if (action == "Delete") {

                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
                    confirm: function () {

                        request = {
                            "Item": item,
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "EmployeesalaryscheduleService/Delete",
                            data: srequest
                        }).success(function (result) {

                            if (result) {
                                $scope.ErrorMessage = "Successfully deleted";
                                $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                if (item.isreceivable) {
                                    var deletelist = [];
                                    $.each($scope.SearchResult, function (k, v) {
                                        if (v.parentid == item.id)
                                            deletelist.push(v);
                                    });

                                    $.each(deletelist, function (k, v) {
                                        $scope.SearchResult.splice(v, 1);
                                    });
                                }
                            }

                        }).error(function (result) {

                        });
                    }
                });
            }
            else if (action == "Edit") {
                $scope.Active = !$scope.Active;
            }
            else if (action == "RemoveZero") {
                for (var index = $scope.SearchResult.length - 1; index >= 0; index--) {
                    if (parseInt($scope.SearchResult[index].amount) == 0)
                        $scope.SearchResult.splice(index);
                }
            }
        }

        $scope.Apply = function () {
            if (parseFloat($scope.Advance.amount) > 0) {
                $scope.Advance.amount = parseFloat($scope.Advance.amount);
                $.each($scope.SearchResult, function (key, item) {
                    item.amount = $scope.Advance.amount;
                });
            }
        }

        $scope.SaveIndivitual = function () {
            $scope.ErrorMessage = '';
            if (typeof ($scope.Advance.salaryheaderid) == 'undefined' || $scope.Advance.salaryheaderid < 0) {
                $rootScope.Toaster.Error("Please Select Advance Head.");
                return;
            }

            if (typeof ($scope.employeeid) == 'undefined' || $scope.employeeid < 0) {
                $rootScope.Toaster.Error("Please Select Employee.");
                return;
            }

            if (typeof ($scope.salarymonth) == 'undefined' || $scope.salarymonth == '' || $scope.salarymonth == null) {
                $rootScope.Toaster.Error("Please Enter issue date.");
                return;
            }

            var request = {
                "Item": {
                    salaryheaderid: $scope.Advance.salaryheaderid,
                    salarymonth: $scope.salarymonth,
                    payateom: $scope.payateom,
                    remarks: $scope.remarks,
                    schedules: []
                },
                "securitytoken": "String content"
            };

            request.Item.schedules.push({
                'employeeid': $scope.employeeid,
                'amount': parseFloat($scope.Advance.advanceamount),
                'advancedate': $scope.salarymonth,
                'remarks': $scope.remarks

            })

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryscheduleService/SaveAdvance",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $rootScope.Toaster.Info("Advance Successfully saved");
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });
        }

        $scope.Save = function () {

            var request = {
                "Item": {
                    salaryheaderid: $scope.Advance.salaryheaderid,
                    salarymonth: $scope.salarymonth,
                    payateom: $scope.payateom,
                    remarks: $scope.remarks,
                    schedules: []
                },
                "securitytoken": "String content"
            }


            $.each($scope.SearchResult, function (key, item) {
                if (parseFloat(item.amount) > 0) {
                    request.Item.schedules.push({
                        'employeeid': item.employeeid,
                        'amount': parseFloat(item.amount),
                        'advancedate': $scope.salarymonth,
                        'remarks': $scope.remarks
                    })
                }
            });


            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryscheduleService/SaveAdvance",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Advance Successfully saved");
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        }

        $scope.Total = 0;

        $scope.exporttoExcel = function (tableid, title) {
            if ($scope.searchmode == 1)
                tableid = 'searchresult';
            else
                tableid = 'entryresult';

            $scope.exportHref = Excel.tableToExcel(tableid, title, 0, 'ADVANCE LIST');
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = "advance.xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
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


        $scope.printSlip = function (div, title, actioncolumncount) {

            var otitle = angular.element('#apptitle').context.title;
            var titlele = angular.element('#apptitle').context.title = title;

            var docHead = document.head.outerHTML;

            var table = $($scope.searchmode == 1 ? '#searchresult' : '#entryresult');
            var ctable = table.clone();

            ctable.find('INPUT').remove();

            ctable.find('THEAD TR.hidden-header').remove();

            for (var index = 0; index < actioncolumncount; index++) {
                ctable.find("th:last-child").remove();
                ctable.find("td:last-child").remove();
            }

            ctable.find('.th-inner').removeClass();
            ctable.removeClass();
            ctable.addClass('fixed_table');
            ctable.addClass('table-bordered');



            ctable.find('THEAD').prepend("<tr><th style = 'text-align:center' colspan='" + ctable.find('THEAD TR TH').length + "'>" + title + ' - ' + $filter('date')($scope.salarymonth, 'dd-MMM-yyyy') + "</th></tr>");
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
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);