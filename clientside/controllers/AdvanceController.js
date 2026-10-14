
var app = angular.module('InfinityLab');

app.controller('AdvanceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Modes = [{ 'id': '0', 'name': 'All' }, { 'id': '1', 'name': 'Loan Paid' },
        { 'id': '2', 'name': 'EMI Receivable' }, { 'id': '3', 'name': 'Advance Paid' },
        { 'id': '4', 'name': 'Advance Receivble' },
            { 'id': '5', 'name': 'Pending Bills' },
            { 'id': '6', 'name': 'Add Paid' }];
        $scope.SearchModes = [{ 'id': 1, 'name': 'Search' }, { 'id': 2, 'name': 'Issue Multiple' }, { 'id': 3, 'name': 'Issue Single' }, { 'id': 4, 'name': 'Summary' }]
        $scope.MonthList = [{ 'id': 1, 'name': 'JANUARY' }, { 'id': 2, 'name': 'FEBRUARY' },
        { 'id': 3, 'name': 'MARCH' }, { 'id': 4, 'name': 'APRIL' },
        { 'id': 5, 'name': 'MAY' }, { 'id': 6, 'name': 'JUNE' },
        { 'id': 7, 'name': 'JULY' }, { 'id': 8, 'name': 'AUGUST' },
        { 'id': 9, 'name': 'SEPTEMBER' }, { 'id': 10, 'name': 'OCTOBER' },
        { 'id': 11, 'name': 'NOVEMBER' }, { 'id': 12, 'name': 'DECEMBER' }];
        $scope.searchmode = 1;
        $scope.mode == 0;
        $scope.showall = false;
        $scope.EntryList = [];
        $scope.SearchResult = [];
        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });
        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Departments = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.Departments = results;
        });
        $scope.Location = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Location = results;
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
                    companyid: $scope.companyid,
                    floorid: $scope.floorid,
                    sectionid: $scope.sectionid,
                    salarymode: $scope.salarymode,
                    showhasleft: $scope.showhasleft,
                    workingmode: $scope.workingmode,
                    schedulemonth: $scope.salarymonth,
                    searchmode: $scope.searchmode,
                    mode: $scope.searchmode == 1 ? $scope.mode : 0
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
                    if ($scope.showall) {
                        $scope.SearchResultAll = $scope.SearchResult;
                    }
                }
                else {
                    $scope.EntryList = result;
                    $scope.CurrentTotal = 0;
                    $scope.Total = 0;
                    $.each(result, function (k, v) {
                        $scope.Total += v.amount;
                        $scope.CurrentTotal += v.amount;
                        if (v.amount == 0)
                            v.amount = '';
                        else
                            v.currentamount = v.amount;
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
                $scope.Total = 0;
                $.each($scope.EntryList, function (key, item) {
                    item.amount = $scope.Advance.amount;
                    $scope.Total += item.amount;
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
                    searchmode: $scope.searchmode,
                    remarks: $scope.remarks,
                    schedules: []
                },
                "securitytoken": "String content"
            };

            request.Item.schedules.push({
                'employeeid': $scope.employeeid,
                'amount': parseFloat($scope.Advance.advanceamount),
                'advancedate': $scope.salarymonth
            });

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryscheduleService/SaveAdvance",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $rootScope.Toaster.Info("Advance Successfully saved");
                    $scope.mode = '3';
                    $.each(result, function (k, v) {
                        v.selected = true;
                        $scope.SearchResult.push(v);
                    });
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }
            }).error(function (result) {
            });
        }

        $scope.searchmodechanged = function () {
            if ($scope.Advance.searchmode == 2) {
                $scope.EntryList = [];
                $.each($scope.Employee, function (k, v) {
                    $scope.EntryList.push(v);
                });
            } else if ($scope.Advance.searchmode == 3) {
                $scope.EntryList = [];
            }
        }

        $scope.Save = function () {

            var request = {
                "Item": {
                    salaryheaderid: $scope.Advance.salaryheaderid,
                    salarymonth: new Date($scope.Advance.year, $scope.Advance.monthid - 1, 1),
                    payateom: $scope.payateom,
                    schedules: []
                },
                "securitytoken": "String content"
            };

            $.each($scope.EntryList, function (key, item) {
                if (parseFloat(item.amount) > 0) {
                    request.Item.schedules.push({
                        'employeeid': item.employeeid,
                        'amount': parseFloat(item.amount),
                        'advancedate': request.Item.salarymonth
                    });
                }
            });


            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryscheduleService/SaveAdvance",
                data: srequest
            }).success(function (result) {
                if (result.ErrorMessage == null || result.ErrorMessage == '') {
                    $rootScope.Toaster.Info("Advance Successfully saved");
                    $scope.CurrentTotal = 0;
                    $scope.Total = 0;
                    $.each($scope.EntryList, function (key, item) {
                        if (parseFloat(item.amount) > 0) {
                            item.currentamount = parseFloat(item.amount);
                            $scope.CurrentTotal += item.currentamount;
                            $scope.Total += item.currentamount;
                        }
                        else {
                            item.currentamount = '';
                            item.amount = '';
                        }
                    });
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });
        }

        $scope.Paycategory = '';
        $scope.PaycategoryChanged = function () {
            var pay = $scope.refpaytypelist.find(function (item) {
                return item.id == $scope.Advance.salaryheaderid;
            });
            if (pay && pay != null) {
                $scope.Paycategory = pay.name;
            }
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

        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            var empcolumn = -1;
            var amtcolumn = -1;
            var salaryheadercode = null;
            var salaryheader = $scope.refpaytypelist.find(function (item) {
                return $scope.Advance.salaryheaderid == item.id;
            });
            if (salaryheader && salaryheader != null) {
                salaryheadercode = salaryheader.code.toLowerCase();
            }
            $.each(value.columns, function (k, v) {
                if (v.toLowerCase().indexOf('code') >= 0)
                    empcolumn = k;
                if (salaryheadercode != null &&
                    v.toLowerCase().indexOf(salaryheadercode) >= 0)
                    amtcolumn = k;
                if (v.toLowerCase().indexOf('amt') >= 0)
                    amtcolumn = k;
                if (v.toLowerCase().indexOf('amount') >= 0)
                    amtcolumn = k;
            });

            if (empcolumn == -1) {
                $rootScope.Toaster.Error("Employee Code Column Missing");
                return;
            }

            if (amtcolumn == -1) {
                $rootScope.Toaster.Error("Amount Column Missing");
                return;
            }

            var missingcodes = [];
            $.each(value.rows, function (k, v) {
                if (k == 0) return;

                var emprow = $scope.EntryList.find(function (item) {
                    return item.employeecode == v.values[empcolumn];
                });

                if (emprow && emprow != null) {
                    emprow.amount = parseFloat(v.values[amtcolumn]);
                }
                else {
                    missingcodes.push(v.values[empcolumn]);
                }
            });

            $scope.Total = 0;
            $.each($scope.EntryList, function (key, item) {
                $scope.Total += item.amount;
            });

            if (missingcodes.length > 0) {
                $rootScope.Toaster.Error("Missing Employees " + missingcodes.join('<br>'));
            }
        }


        $scope.printSlip = function (div) {


            $scope.SearchResultAll = [];
            $.each($scope.searchmode != '2' ? $scope.SearchResult : $scope.EntryList, function (k, v) {
                if (v.selected)
                    $scope.SearchResultAll.push(v)
            });

            if ($scope.SearchResultAll.length > 0) {
                $scope.showall = true;

                $timeout(function () {

                    var docHead = document.head.outerHTML;
                    var printContents = document.getElementById(div).outerHTML;
                    var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

                    var newWin = window.open("", "_blank", winAttr);
                    var writeDoc = newWin.document;
                    writeDoc.open();
                    writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
                    writeDoc.close();
                    newWin.focus();

                    $scope.showall = false;
                }, 30);
            }
            else {
                $rootScope.Toaster.Error("Please select the advance items to be print.");
            }
        }

        $scope.printDiv = function (div, title, actioncolumncount) {

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