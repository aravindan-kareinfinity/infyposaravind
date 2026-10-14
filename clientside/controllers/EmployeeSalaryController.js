
var app = angular.module('InfinityLab');

app.controller('EmployeeSalaryController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'cntxService', 'ngDialog', 'Excel', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, cntxService, ngDialog, Excel, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.panelheight = { 'height': '500px' };
        $scope.Mode = 'Fine';
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.LocalContext = cntxService.CreatePageContext('employeesalary');
        $scope.detective =
            $scope.refapplist = [{ 'code': 'APPLICATIONSETTINGS', 'value': 'Application Settings' }];
        $scope.key = "APPLICATIONSETTINGS"
        $scope.Entity = {
            "checked": 'payroll_leaveencashmentadjust.value',
            "id": 0,
            "userid": 0,
            "key": "APPLICATIONSETTINGS",
            "reportformat": { format: "" },
            "organizationid": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": new Date(),
            "name": "",
            "nooflines": 0,
            "isdefault": false,
            "locationid": 0,
            "appsettings": {
                "offersale": true, "season": null, "area": null, "salereason": null, "lockdate": "", intray_supplier: "", "key": "APPLICATIONSETTINGS",
                intray_product: "", intray_agent: "", intray_inward: "", intray_task: "", salesman_insentive: "",
                "sale_negativestock": false, 'settlement_reward_slabs': null
            },
            StoreOpeningAttribute: {
                'opening': '', 'closing': ''
            },

        };

        $scope.refpaycategorylistearnings = [];
        $scope.refpaycategorylistdeductions = [];


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

        $scope.GetDate2String = function (date) {
            return $filter('date')(date, 'dd-MM-yyyy');
        }

        $scope.exportigrid = function (infygrid, filename, subtitle) {
            var title = $rootScope.UserInfo.CompanyName;
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

        $scope.exporttoExcel = function (tableid, title, removeactioncolumn) {
            var title = $scope.config.bydate ? ($scope.GetDate2String($scope.config.generatefrom) + ' - ' +
                $scope.GetDate2String($scope.config.generateto)) : ($scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'SALARY LIST FOR  - ' + title);

            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }


        $scope.printDiv = function (tablename) {
            var title = $scope.config.bydate ? ($scope.GetDate2String($scope.config.generatefrom) + ' - ' +
                $scope.GetDate2String($scope.config.generateto)) : ($scope.SearchRef($scope.month, $scope.MonthList) + " " + $scope.year);
            title = 'Salary for ' + title;
            var otitle = angular.element('#apptitle').context.title;
            var titlele = angular.element('#apptitle').context.title = title;

            var docHead = document.head.outerHTML;

            var table = $('#' + tablename);
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

        $scope.getsalaryitem = function (refitem, empitem) {
            var result = empitem.detail.find(function (item) {
                return item.id == refitem.id;
            })
            if (typeof (result) == 'undefined' || result == null)
                return 0;
            else
                return result.amount;
        };

        $scope.setsalaryitem = function (refitem, empitem,value) {
            var result = empitem.detail.find(function (item) {
                return item.id == refitem.id;
            })
            if (typeof (result) != 'undefined' && result != null)                
                result.amount = value;
        };

        $scope.Companies = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Companies = results;
        });

        $scope.Departments = restService.Get('ReferencelistService/DEPARTMENT', true).then(function (results) {
            $scope.Departments = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
            $scope.EmployeeBasedIncentive = $rootScope.UserInfo.appsettings.incentivemode == 6;
            $timeout(function () {
                $scope.config = $scope.LocalContext.Get('Config', false);
            }, 100);
        });

        $scope.MonthList = [{ 'id': 1, 'name': 'JANUARY' }, { 'id': 2, 'name': 'FEBRUARY' },
        { 'id': 3, 'name': 'MARCH' }, { 'id': 4, 'name': 'APRIL' },
        { 'id': 5, 'name': 'MAY' }, { 'id': 6, 'name': 'JUNE' },
        { 'id': 7, 'name': 'JULY' }, { 'id': 8, 'name': 'AUGUST' },
        { 'id': 9, 'name': 'SEPTEMBER' }, { 'id': 10, 'name': 'OCTOBER' },
        { 'id': 11, 'name': 'NOVEMBER' }, { 'id': 12, 'name': 'DECEMBER' }];

        $scope.OvertimeSourceList = [{ 'id': 1, 'name': 'GR' }, { 'id': 2, 'name': 'BP' }];
        
        $scope.SalaryModes = [{ 'id': 0, 'name': 'Monthly' }, { 'id': 1, 'name': 'Dailywages' }];
        $scope.WorkingModes = [{ 'id': 0, 'name': 'ALL' }, { 'id': 1, 'name': 'Temporary' }, { 'id': 2, 'name': 'Probation' }, { 'id': 3, 'name': 'Permanant' }];
        $scope.FilterModes = [{ 'id': 0, 'name': 'All Employees' }, { 'id': 1, 'name': 'Payable Employees' }, { 'id': 2, 'name': 'Left Employees' }, { 'id': 3, 'name': 'With held Employees' }];
        $scope.filtermode = 1;

        $scope.ParollProcess = [{ 'id': 0, 'name': 'Payroll Process' },
        { 'id': 8, 'name': 'Payroll Uploader' },
        //{ 'id': 1, 'name': 'Leave Process' },
        //{ 'id': 2, 'name': 'Incentive Process' },
        //{ 'id': 3, 'name': 'Bonus Process' },
        //{ 'id': 4, 'name': 'Beta Process' },
        //{ 'id': 5, 'name': 'OverTime Process' },
        //{ 'id': 6, 'name': 'OT-Beta-Leave' },
        { 'id': 7, 'name': 'Daily Wages' }];


        $scope.FineEntry = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.WorkdaysMode = [{ 'id': 1, 'name': 'Fixed Days' }, { 'id': 2, 'name': 'All Days' }];
        $scope.LeaveDaysType = [{ 'id': 1, 'name': 'Fixed Days' }, { 'id': 2, 'name': 'Config' }, { 'id': 3, 'name': '7 Working Days' }];

        $scope.LeaveEncashType = [{ 'id': 1, 'name': 'Fixed Days' }, { 'id': 2, 'name': 'Config' }];
        $scope.HolidayDayType = [{ 'id': 1, 'name': 'Fixed Days' }, { 'id': 2, 'name': 'All Days' }];

        $scope.LeaveEncashMode = [{ 'id': 1, 'name': 'Gross Salary' }, { 'id': 2, 'name': 'Basic Pay' }];
        $scope.EncashmentModes = [{ 'id': 1, 'name': 'Each Days Of ' }, { 'id': 2, 'name': 'Minimum Workings' }, { 'id': 3, 'name': 'Permanent' }];

        $scope.key = "APPLICATIONSETTINGS"
        $scope.ErrorMessage = "";
        $scope.ApplicationSettings = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ApplicationSettings.key = 'APPLICATIONSETTINGS';

        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }

        $scope.Search = function () {
            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ConfigurationService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $scope.SearchResult = result;
                }
                else {
                    $scope.ApplicationSettings = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.ApplicationSettings.key = $scope.SearchCondition.key;
                }
            }).error(function (result) {
            });

        }




        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.New = function () {
            $scope.FineEntry = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.Clear = function () {
            $scope.FineEntry = (JSON.parse(JSON.stringify($scope.Entity)));
            if ($scope.igrid != null)
                $scope.igrid.destroy();
        }

        $scope.SearchStoreTransaction = function () {
            request = {
                "Item": $scope.FineEntry,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "StoretransactionService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ShowOpeningResult(result);

            }).error(function (result) {
            });
        }

        $scope.SaveStoreTransaction = function () {

            request = {
                "Item": $scope.FineEntry,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StoretransactionService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("Data Saved Successfully.!");
                    $scope.FineEntry = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }

        $scope.Save = function () {
            debugger;
            request = {
                "Item":
                    //$scope.ApplicationSettings.id = "1",
                    $scope.ApplicationSettings,
                key: $scope.key,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalaryConfig/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.SearchHistory(result);
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
                $rootScope.Toaster.Error(result.ErrorMessage);
            });
        }
        $scope.getsalarydefault = function (refitem, empitem) {
            var result = empitem.detail.find(function (item) {
                return item.id == refitem.id;
            })
            if (typeof (result) == 'undefined' || result == null)
                return 0;
            else
                return result.value;
        };
        $scope.Generate = function () {
            $scope.LocalContext.Post('Config', $scope.config, false);
            $scope.ShowCheckList = false;
            request = {
                "Item": $scope.config,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/GenerateCustom",
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
                    });
                    $timeout(function () {
                        $scope.SalaryConfig();
                    }, 50);
                }
                else {
                    $scope.SearchResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }

        $scope.SearchCheckList = function () {
            $scope.LocalContext.Post('Config', $scope.config, false);
            $scope.ShowCheckList = true;
            request = {
                "Item": $scope.config,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeesalaryService/GenerateCustom",
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
                    $timeout(function () {
                        $scope.CheckListConfig();
                    }, 50);
                }
                else {
                    $scope.SearchResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }
        $scope.GenerateFine = function () {
            request = {
                "Item": $scope.FineEntry,
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentivesService/GenerateFine",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.FineConfig();
            });
        }

        $scope.LoadConfig = function () {
            $http({
                method: 'GET',
                url: "ConfigurationService/ConfigByKey/CONFIG_CUSTOMSALARY"
            }).success(function (result) {
                if (result && result != null) {
                    $scope.config = JSON.parse(result.value);
                    $rootScope.Toaster.Info("Configuration Loaded Successfully.!");
                }
            });
        }

        $scope.SaveConfig = function () {
            request = {
                "Item": { 'key': 'CONFIG_CUSTOMSALARY', 'value': JSON.stringify($scope.config)},
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ConfigurationService/ConfigByKey",
                data: srequest
            }).success(function (result) {
                if (result)
                    $rootScope.Toaster.Info("Configuration Saved Successfully.!");
            });
        }

        $scope.FineConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 100);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('EmployeeCode', 'Code', 100);
            igrid.AddSimpleColumn('Name', 'Name', 150);
            igrid.AddSimpleColumn('GrossPay', 'Gross Pay', 75);
            igrid.AddSimpleColumn('workingdays', 'Days', 75);
            igrid.AddSimpleColumn('actualworking', 'Working', 75);
            igrid.AddSimpleColumn('nonworking', 'Non Working', 75);
            igrid.AddSimpleColumn('latecheckin', 'Late', 75);
            igrid.AddSimpleColumn('earlycheckout', 'Early', 75);
            igrid.AddSimpleColumn('earlyandlate', 'Minutes', 75);
            igrid.AddSimpleColumn('Fine', 'Fine', 75);
            igrid.AddButtonColumn('action', 'Action', 100, 'Change', function (e, args) {
                $scope.$apply(function () {
                    $scope.ManageLRInboxList(args.item);
                });
                return true;
            });
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.OpenSalaryConfig = function (item) {
            var request = {
                "Item": {'employeeid':item.employeeid},
                "securitytoken": "String content"
            }
            request.Item.showconfig = true;
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "EmployeesalaryheaderService/SearchAll",
                data: srequest
            }).success(function (result) {
                $scope.ShowSalaryConfig(item.employeeid,result[0]);
            }).error(function (result) {
            });
        }

        $scope.ShowSalaryConfig = function (employeeid,item) {

            var dialog = ngDialog.open({
                template: 'SalaryManagement',
                data: [$scope, { 'employeeid':employeeid }, item],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {

                    $scope.IncentiveSources = [
                        { 'id': 0, 'name': 'None' },
                        { 'id': 1, 'name': 'Total Sales' },
                        { 'id': 2, 'name': 'Location Sales' },
                        { 'id': 3, 'name': 'Section Sales' },
                        { 'id': 4, 'name': 'Floor Sales' },
                        { 'id': 5, 'name': 'Team Sales' },
                        { 'id': 6, 'name': 'Salesmen' },
                        { 'id': 7, 'name': 'Company' }];

                    $scope.TargetMinumumMethods = [
                        { 'id': 0, 'name': 'No Target' },
                        { 'id': 1, 'name': 'Fixed Value' },
                        { 'id': 2, 'name': 'Percentage Of Target' }];

                    $scope.CalculationSources = [
                        { 'id': 0, 'name': 'None' },
                        { 'id': 1, 'name': 'Fixed' },
                        { 'id': 2, 'name': 'Product Age Slab' },
                        { 'id': 3, 'name': 'Section Age Slab' },
                        { 'id': 4, 'name': 'Product Price Slab' },
                        { 'id': 5, 'name': 'Section Price Slab' },
                        { 'id': 6, 'name': 'Target Working Days' },
                        { 'id': 7, 'name': 'Value Slab' },
                        { 'id': 8, 'name': 'Company' },
                        { 'id': 9, 'name': 'Product Age And Price' },
                        { 'id': 10, 'name': 'Employe & Product Age' },
                        { 'id': 11, 'name': 'Employee By Section' }
                    ];
                    $scope.ValueTypes =
                        [{ 'id': 0, 'name': 'None' },
                        { 'id': 1, 'name': 'Value' },
                        { 'id': 2, 'name': 'Quantity' }];

                    $scope.Entity = {
                        "id": 0,
                        "employeeid": "",
                        "fromdate": "",
                        "iscurrent": 0,
                        "basicpay": 0.0,
                        "eligibleesi": false,
                        "eligiblepf": false,
                        "configuration": [],
                        "pfselfpaid": 0,
                        "esiselfpaid": 0.0,
                        "grosspay": 0,
                        "isactive": true,
                        "createdby": 0,
                        "modifiedby": 0,
                        "version": 1,
                        "salarystructureid": 0,
                        "createdon": new Date(),
                        "modifiedon": new Date()
                    }
                    $scope.Loaded = false;
                    $scope.EmployeeBasedIncentive = $scope.ngDialogData[0].EmployeeBasedIncentive;
                    $scope.refemployeelist = $scope.ngDialogData[0].Employee;
                    $scope.Delete = function (item) {
                        request = {
                            "Item": $scope.Item,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "EmployeesalaryheaderService/Delete",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(null);
                        }).error(function (result) {
                        });
                    };


                    $scope.SetPayCategory = function (type, value, structure) {
                        var exist = $scope.refpaycategorylist.find(function (item) {
                            return item.additionalinfo.paytype == type;
                        });

                        if (exist != null) {
                            var itm = $scope.Item.configuration.find(function (vv) {
                                return (exist.code == vv.code);
                            });

                            itm.formula = value.toString();
                        }
                    }

                    $scope.StructureChanged = function (list, id) {
                        var itm = list.find(function (vv) {
                            return (vv.id == id);
                        });

                        if (itm != null) {
                            $scope.newgross = 'BP:' + itm.fixedbasic + ',' + 'DA:' + itm.fixedda, '=' + (itm.fixedbasic + itm.fixedda);
                        }
                    }

                    $scope.ChangeStructure = function (item) {
                        var structure = null;
                        $.each($scope.refstructurelist, function (k, v) {
                            if (structure == null && v.id == item.salarystructureid) {
                                structure = v;
                            }
                        });

                        if (structure != null) {
                            $scope.Item.basicpay = structure.fixedbasic;
                            $scope.Item.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));

                            $.each($scope.Item.configuration, function (k, v) {
                                var itm = structure.configuration.find(function (vv) {
                                    return (v.code == vv.code);
                                });
                                if (itm != null) {
                                    v.formula = itm.formula;
                                    v.code = itm.code;
                                }
                            });

                            var gross = item.grosspay;
                            if (typeof (item.ctc) == 'undefined' || item.ctc == '')
                                item.ctc = item.grosspay;

                            var ctc = item.ctc;
                            var specialdeduction = 0;;
                            var conveyance = 0;
                            var hra = 0;
                            var specialallow = 0;
                            var otherallowance = 0;
                            var available = 0;

                            if (structure.fixedbasic + structure.fixedda > ctc) {
                                specialdeduction = (structure.fixedbasic + structure.fixedda) - ctc;
                            }
                            else {
                                available = gross - (structure.fixedbasic + structure.fixedda);
                            }

                            if (available > 0) {
                                if (available > 1600)
                                    conveyance = 1600;
                                else
                                    conveyance = available;
                                available -= conveyance;
                            }

                            if (available >= Math.ceil((structure.fixedbasic + structure.fixedda) * 15 / 100)) {
                                hra = Math.ceil((structure.fixedbasic + structure.fixedda) * 30 / 100);
                                if (hra > available)
                                    hra = available;
                                available -= hra;
                            }

                            if (available > 0) {
                                specialallow = available;
                            }

                            $scope.SetPayCategory('BasicPay', structure.fixedbasic);
                            $scope.SetPayCategory('DearnessAllowance', structure.fixedda);
                            $scope.SetPayCategory('Conveyance', conveyance);
                            $scope.SetPayCategory('HouseRentAllowance', hra);
                            $scope.SetPayCategory('SpecialAllowance', specialallow);
                            $scope.SetPayCategory('SpecialDeduction', specialdeduction);

                            if (!item.eligibleesi) {
                                $scope.SetPayCategory('ESI', 0);
                            }
                            if (!item.eligiblepf) {
                                $scope.SetPayCategory('ProvidentFund', 0);
                            }

                            if (!item.esiselfpaid) {
                                $scope.SetPayCategory('ESI_Employee', 0);
                            }
                            if (!item.pfselfpaid) {
                                $scope.SetPayCategory('PF_Employee', 0);
                            }
                        }
                    }

                    $scope.refstructurelist = restService.Get('ReferencelistService/FULL-STRUCTURE', true).then(function (results) {
                        $scope.refstructurelist = results;
                    });
                    $scope.refpaycategorylist = restService.Get('ReferencelistService/FULL-PAYCATEGORY', true).then(function (results) {
                        $scope.refpaycategorylist = results;

                        $.each(results, function (k, v) {
                            $scope.Entity.configuration.push({
                                'id': v.id, 'code': v.code, 'formula':
                                    v.additionalinfo == null ? '' : v.additionalinfo.formula
                            });
                        });
                        $scope.SalaryStructure = (JSON.parse(JSON.stringify($scope.Entity)));
                    });


                    $scope.refstructurelist = null;
                    restService.Get('ReferencelistService/FULL-STRUCTURE', true).then(function (results) {
                        $scope.refstructurelist = results;
                    });
                    $scope.refpaycategorylist = null;
                    restService.Get('ReferencelistService/FULL-PAYCATEGORY', true).then(function (results) {
                        $scope.refpaycategorylist = results;
                        $.each(results, function (k, v) {
                            $scope.Entity.configuration.push({
                                'id': v.id, 'code': v.code, 'formula':
                                    v.additionalinfo == null ? '' : v.additionalinfo.formula
                            });
                        });
                        if (typeof ($scope.ngDialogData[2]) != 'undefined' && $scope.ngDialogData[2] != null) {
                            $scope.Item = $scope.ngDialogData[2];
                            if ($scope.Item.configuration == null) {
                                $scope.Item.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));
                            }
                            else {
                                var oldconfig = $scope.Item.configuration;
                                $scope.Item.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));
                                $.each($scope.Item.configuration, function (k, v) {
                                    var itm = oldconfig.find(function (vv) {
                                        return (v.code == vv.code);
                                    });
                                    if (itm != null) {
                                        v.formula = itm.formula;
                                        v.code = itm.code;
                                    }
                                });
                            }
                            $scope.Loaded = true;
                            $scope.salarystructureid = $scope.Item.salarystructureid;
                            $scope.Item.salarystructureid = 0;
                            $scope.LoadDefault();
                        }
                        else {
                            $scope.Item = $scope.ngDialogData[1];
                            $scope.Item.configuration = (JSON.parse(JSON.stringify($scope.Entity.configuration)));
                            $scope.Loaded = true;
                        }
                    });

                    $scope.LoadDefault = function () {
                        if ($scope.refstructurelist == null || $scope.refpaycategorylist == null) {
                            $timeout(function () { $scope.LoadDefault(); }, 50);
                            return;
                        }
                        $scope.Item.salarystructureid = $scope.salarystructureid;
                        $scope.StructureChanged($scope.refstructurelist, $scope.Item.salarystructureid);
                    }
                    $scope.newsalary = function () {
                        $scope.Item.salaryheaderid = 0;
                    };

                    $scope.Change = function () {
                        $scope.closeThisDialog($scope.Item);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": data.value,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "EmployeesalaryheaderService/Save",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Salary Managed Successfully.!");
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.cgrid = null;
        $scope.CheckListConfig = function () {
            if ($scope.cgrid != null)
                $scope.cgrid.destroy();

            var igrid = new InfyGrid('gridChecklist', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('employeeno', 'Code', 100);
            igrid.AddSimpleColumn('name', 'Name', 150);
            igrid.AddSimpleColumn('presentdays', 'P.D', 40);


            var column = igrid.AddCustomColumn('PR', 'PR', 40, function (item, col) {
                return item.additionalinfo.totaldays - item.additionalinfo.leavetaken;
            });
            igrid.AddSimpleColumn('leavedays', 'L.Days', 40);
            var column = igrid.AddCustomColumn('absentdays', 'A.Days', 40, function (item, col) {
                return item.additionalinfo.absentdays;
            });
            var column = igrid.AddCustomColumn('leaveencashmentdays', 'LE.Days', 40, function (item, col) {
                return item.additionalinfo.leaveencashmentdays;
            });
            var column = igrid.AddCustomColumn('lop', 'LOP', 40, function (item, col) {
                return item.additionalinfo.gross - item.earnings;
            });

            $.each($scope.refpaycategorylistearnings, function (k, v) {
                var column = igrid.AddCustomColumn('default-' + k, v.code, 40, function (item, col) {
                    return $scope.getsalarydefault(col.internal, item);
                });
                column.internal = v;
                column.total = true;
                column.getValue = function (item, col) {
                    return $scope.getsalarydefault(col.internal, item);
                }
            })

            var column = igrid.AddCustomColumn('gross', 'Gross', 70, function (item, col) {
                return item.additionalinfo.gross;
            });
            column.getValue = function (item, col) {
                return item.additionalinfo.gross;
            }

            igrid.AddFormatedColumn('earnings', 'Earning', 70, 'N2', 2).total = true;

            $.each($scope.refpaycategorylistearnings, function (k, v) {
                var column = igrid.AddCustomColumn('earnings-' + k, v.code, 40, function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                });
                column.internal = v;
                column.total = true;
                column.getValue = function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                }
            })

            $.each($scope.refpaycategorylistdeductions, function (k, v) {
                var column = igrid.AddCustomColumn('decution-' + k, v.code, 40, function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                });
                column.internal = v;
                column.total = true;
                column.getValue = function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                }
            })

            igrid.AddFormatedColumn('deductions', 'Deductions', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('netsalary', 'Net', 70, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.cgrid = igrid;
            $scope.cgrid.UpdateData($scope.SearchResult);
        }

        $scope.sgrid = null;
        $scope.SalaryConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('SalaryList', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('employeeno', 'Code', 100);
            igrid.AddSimpleColumn('name', 'Name', 150);

            //var additionalabsentdays = $scope.SearchResult.find(function (item) {
            //    return item.additionalinfo.additionalabsentdays > 0;
            //});

            //if (additionalabsentdays && additionalabsentdays != null) {

                var column = igrid.AddCustomColumn('WD', 'W.D', 40, function (item, col) {
                    return item.additionalinfo.workingdays;
                });

                var column = igrid.AddCustomColumn('presentdays', 'P.D', 40, function (item, col) {
                    return item.additionalinfo.paydays;
                });
            //}
            //else {
            //    igrid.AddSimpleColumn('presentdays', 'P.D', 40);
            //}

            if ($scope.config.showalldays) {
                var column = igrid.AddCustomColumn('PR', 'PR', 40, function (item, col) {
                    return item.additionalinfo.totaldays - item.additionalinfo.leavetaken;
                });
                igrid.AddSimpleColumn('leavedays', 'L.Days', 40);
                var column = igrid.AddCustomColumn('absentdays', 'A.Days', 40, function (item, col) {
                    return item.additionalinfo.absentdays;
                });
                var column = igrid.AddCustomColumn('leaveencashmentdays', 'LE.Days', 40, function (item, col) {
                    return item.additionalinfo.leaveencashmentdays;
                });
            }

            $.each($scope.refpaycategorylistearnings, function (k, v) {
                if ($scope.config.editpayheader && v.id == $scope.config.editpayheaderid) {
                    return;
                }
                var column = igrid.AddCustomColumn('earning-' + k, v.code, 40, function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                });
                column.internal = v;
                column.total = true;
                column.getValue = function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                }
            })

            if ($scope.config.editpayheader) {
                var v = $scope.refpaycategorylistearnings.find(function (item) { return item.id == $scope.config.editpayheaderid; });
                var k = $scope.refpaycategorylistearnings.indexOf(v);
                var column = igrid.AddCustomColumn('earning-' + k, v.code, 40, function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                });
                column.internal = v;
                column.total = true;
                column.getValue = function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                }
                column.setValue = function (item, col, newvalue, callback) {
                    var lastValue = col.getValue(item, col);
                    var newvalue = parseFloat(newvalue);
                    if (isNaN(newvalue))
                        newvalue = 0;
                    if (lastValue > newvalue) {
                        item.earnings -= (lastValue - newvalue);
                    } else {
                        item.earnings += (newvalue-lastValue);
                    }
                    $scope.setsalaryitem(col.internal, item,newvalue);
                }
                column.editor = Slick.Editors.Text;
            }

            igrid.AddFormatedColumn('earnings', 'Earning', 70, 'N2', 2).total = true;
            $.each($scope.refpaycategorylistdeductions, function (k, v) {
                var column = igrid.AddCustomColumn('decution-' + k, v.code, 40, function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                });
                column.internal = v;
                column.total = true;
                column.getValue = function (item, col) {
                    return $scope.getsalaryitem(col.internal, item);
                }
            })

            igrid.AddFormatedColumn('deductions', 'Deductions', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('netsalary', 'Net', 70, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onContextMenu.subscribe(function (e) {
                e.preventDefault();
                var cell = igrid.grid.getCellFromEvent(e);
                var row = cell.row;
                var column = cell.cell;
                if (column == 2 || ($scope.config.showalldays && (column == 3 || column == 4 || column == 5))) {
                    var itemdata = igrid.grid.getDataItem(row);
                    $scope.ShowDayInfo(itemdata.additionalinfo, itemdata.name);
                }
                if (column == 1 && row >= 0) {
                    var itemdata = igrid.grid.getDataItem(row);
                    $scope.OpenSalaryConfig(itemdata);
                }
            });
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.SearchResult);
        }

        $scope.LoadSchedule = function (headerid) {
            var saveschedule = { 'month': $scope.config.month, 'year': $scope.config.year, 'headerid': headerid, 'schedules': [] };
            $.each($scope.SearchResult, function (k, v) {
                var result = v.detail.find(function (item) {
                    return item.id == headerid;
                });
                var amount = 0;
                if (result && result != null)
                    amount = result.amount;
                saveschedule.schedules.push({ 'employeeid': v.employeeid, 'amount': amount, 'salaryid': v.id, 'earnings': v.earnings });
            });

            if (saveschedule.schedules.length == 0) {
                return;
            }

            request = {
                "Item": saveschedule,
                "SecurityToken": "String content"
            }
            $http({
                method: 'POST',
                url: "EmployeesalaryService/LoadSchedule",
                data: request
            }).success(function (result) {


            }).error(function (result) {
            });
        }

        $scope.SaveSchedule = function (headerid) {
            var saveschedule = { 'month': $scope.config.month, 'year': $scope.config.year, 'headerid': headerid, 'schedules': [] };
            $.each($scope.SearchResult, function (k, v) {
                var result = v.detail.find(function (item) {
                    return item.id == headerid;
                });
                var amount = 0;
                if (result && result != null)
                    amount = result.amount;
                saveschedule.schedules.push({ 'employeeid': v.employeeid, 'amount': amount, 'salaryid': v.id, 'earnings': v.earnings });
            });
            if (saveschedule.schedules.length == 0) {
                return;
            }

            request = {
                "Item": saveschedule,
                "SecurityToken": "String content"
            }
             $http({
                method: 'POST',
                 url: "EmployeesalaryService/SaveSchedule",
                 data: request
            }).success(function (result) {


            }).error(function (result) {
            });
        }

        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 50) + 'px';
            }
        }, 100);

    }]);