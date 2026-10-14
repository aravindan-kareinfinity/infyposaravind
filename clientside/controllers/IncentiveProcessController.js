
var app = angular.module('InfinityLab');

app.controller('IncentiveProcessController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.EnableNotes = false;
        $scope.totalearning = 0;
        $scope.totaldedution = 0;
        $scope.totalnetsalary = 0;
        $scope.searchmode = 0;

        $scope.IncentiveModes = [
            { 'id': 0, 'name': 'ALL', 'show': false },
            { 'id': 44, 'name': 'Fixed %', 'show': false },
            { 'id': 12, 'name': 'Star(*) Barcode', 'show': false },
            { 'id': 13, 'name': 'Stock Marker', 'show': false },
            { 'id': 10, 'name': 'Section Price Range', 'show': false },
            { 'id': 1, 'name': 'Product Price Range', 'show': false },
            { 'id': 3, 'name': 'Section By Age', 'show': false },
            { 'id': 4, 'name': 'Product By Age', 'show': false },
            { 'id': 15, 'name': 'Product By Age and Price', 'show': false },
            { 'id': 11, 'name': 'Product By Qty', 'show': false },
            { 'id': 5, 'name': 'Employee By Product Age', 'show': false },
            { 'id': 6, 'name': 'Barcode Prefix', 'show': false },
            { 'id': 7, 'name': 'Sales Value By Slab', 'show': false },
            { 'id': 77, 'name': 'Target Value By Slab', 'show': false },
            { 'id': 22, 'name': 'Tailoring By Employee', 'show': false },
            { 'id': 8, 'name': 'Target / Gross / Days', 'show': false },
            { 'id': 9, 'name': 'Company Level', 'show': false },
            { 'id': 16, 'name': 'Employee Target Range', 'show': false },
            { 'id': 88, 'name': 'Employee Role', 'show': false },
            { 'id': 17, 'name': 'Employee By Section', 'show': false }];



        $scope.Floors = restService.Get('ReferencelistService/FLOOR', true).then(function (results) {
            $scope.Floors = results;
        });

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

        $scope.Locations = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.Divisions = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Divisions = results;
        });

        $scope.fromyear = new Date().getFullYear();
        $scope.toyear = new Date().getFullYear();
        $scope.year = new Date().getFullYear();

        $scope.employeeid = 0;
        $scope.payslipmonth = new Date();
        $scope.SearchResult = [];
        $scope.LeaveResult = [];


        $scope.GenerateIncentive = function () {
            var searchmodes = [];
            $.each($scope.IncentiveModes, function (k, v) {
                if (v.show == true)
                    searchmodes.push(v.id);
            });

            var request = {
                "Item": {
                    departmentid: $scope.departmentid,
                    sectionid: $scope.sectionid,
                    employeesectionid: $scope.employeesectionid,
                    employeefloorid: $scope.employeefloorid,
                    locationid: $scope.locationid,
                    fromdate: $scope.fromdate,
                    todate: $scope.todate,
                    companyid: $scope.companyid,
                    employeeid: $scope.employeeid,
                    includesales: $scope.includesales,
                    bydate: true,
                    searchmodes: searchmodes
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $scope.EnableNotes = false;
            $http({
                method: 'POST',
                url: "IncentivesService/GenerateIncentive",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    if ($scope.excludezero) {
                        $scope.IncentiveResult = [];
                        $.each(result, function (k, v) {
                            if (v.amount > 0)
                                $scope.IncentiveResult.push(v);
                        });
                    }
                    else {
                        $scope.IncentiveResult = result;
                    }
                    $scope.IncentiveConfig();
                }
                else {
                    $scope.IncentiveResult = [];
                    $rootScope.Toaster.Info("Payslip not generated.");
                }
            }).error(function (result) {
            });
        }

        $scope.IncentiveConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);

            igrid.AddSimpleColumn('employeecode', 'Employee ID', 100);
            igrid.AddSimpleColumn('employeename', 'Employee Name', 200);
            igrid.AddReferenceColumn('departmentid', 'Department', 120, $scope.Departments);
            igrid.AddReferenceColumn('sectionid', 'Section', 100, $scope.Divisions);





            var hasbarcodeprefix = false;
            var hassm = false;
            var hasproductbyname = false;
            var hassectionprice = false;
            var hasproductprice = false;
            var hassectionage = false;
            var hasproductage = false;
            var hascompanylevel = false;
            var hassalesslabincentive = false;
            var hasemployeeagerangeincentive = false;
            var hasproductagepricerangeincentive = false;
            var hasemployeetailoringincentive = false;
            var hastargetincentive = false;
            var hasnotes = false;
            var hassection = false;
            var hastargetamount = false;
            var hasrole = false;

            $.each($scope.IncentiveResult, function (k, v) {
                if (v.notes != null)
                    hasnotes = true;
                if (v.rolesalesmenincentive > 0 || v.rolesupervisorincentive > 0)
                    hasrole = true;
                if (v.targetamount > 0)
                    hastargetamount = true;
                if (v.targetincentive != 0)
                    hastargetincentive = true;
                if (v.prefixincentive != 0)
                    hasbarcodeprefix = true;
                if (v.smincentive != 0)
                    hassm = true;
                if (v.employeeagerangeincentive != 0)
                    hasemployeeagerangeincentive = true;
                if (v.productlevelincentive != 0)
                    hasproductbyname = true;
                if (v.sectionpricerange != 0)
                    hassectionprice = true;
                if (v.sectionincentive != 0)
                    hassection = true;
                if (v.productpricerangeincentive != 0)
                    hasproductprice = true;
                if (v.sectionagerangeincentive != 0)
                    hassectionage = true;
                if (v.productagerangeincentive != 0)
                    hasproductage = true;
                if (v.companyincentive != 0)
                    hascompanylevel = true;
                if (v.salesslabincentive != 0)
                    hassalesslabincentive = true;
                if (v.productagepricerangeincentive != 0)
                    hasproductagepricerangeincentive = true;
                if (v.employeetailoringincentive != 0)
                    hasemployeetailoringincentive = true;
            });


            if (hastargetamount) {
                igrid.AddReferenceColumn('inchargeid', 'Incarge', 120, $scope.Employee);
            }
            igrid.AddFormatedColumn('days', 'Working Days', 80, 'N2', 2);
            if (hastargetamount) {
                igrid.AddFormatedColumn('workingdays', 'Present Days', 80, 'N2', 2);
            }

            if ($rootScope.UserInfo.appsettings.incentivemode < 4) {
                igrid.AddFormatedColumn('incentivepercentage', 'Incentive %', 100, 'N4', 4);
                igrid.AddFormatedColumn('earnings', 'Earnings', 100, 'N2', 2);
                igrid.AddFormatedColumn('totalpay', 'T.Earning', 100, 'N2', 2);
                igrid.AddFormatedColumn('distpercentage', 'D.Incentive %', 100, 'N2', 2);
                igrid.AddFormatedColumn('allocation', 'T.Allocation', 100, 'N2', 2);
            }

            igrid.AddFormatedColumn('salesvalue', 'Total Sales', 100, 'N2', 2).total = true;
            $scope.EnableNotes = hasnotes;

            if (hastargetamount) {
                igrid.AddFormatedColumn('targetamount', 'Target', 100, 'N2', 2).total = true;
            }

            if ($rootScope.UserInfo.appsettings.incentivemode >= 4) {
                igrid.AddFormatedColumn('incentivepercentage', '%', 50, 'N2', 2);
                if ($rootScope.UserInfo.appsettings.incentivemode != 6)
                    igrid.AddFormatedColumn('incentive', 'Configured', 100, 'N2', 2);

                //if ($rootScope.UserInfo.appsettings.incentivemode == 6)
                //    igrid.AddFormatedColumn('incentive', 'Configured', 100, 'N3', 3);
                //else
                //    igrid.AddFormatedColumn('incentive', 'Configured', 100, 'N2', 2);
            }
            if (hastargetincentive) {
                igrid.AddFormatedColumn('targetincentive', 'Target Incentive', 100, 'N2', 2).total = true;
            }

            if (hasproductbyname) {
                igrid.AddFormatedColumn('productlevelvalue', 'Product Level (S)', 100, 'N3', 3).total = true;
                igrid.AddFormatedColumn('productlevelincentive', 'Product Level (I)', 100, 'N2', 2).total = true;
            }

            
            if (hasrole) {
                igrid.AddFormatedColumn('rolesupervisorincentive', 'Role (S)', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rolesalesmenincentive', 'Role (G)', 100, 'N2', 2).total = true;
            }

            if (hasbarcodeprefix) {
                igrid.AddFormatedColumn('prefixvalue', 'Barcode Prefix (S)', 100, 'N3', 3).total = true;
                igrid.AddFormatedColumn('prefixincentive', 'Barcode Prefix (I)', 100, 'N2', 2).total = true;
            }

            if (hassection) {
                igrid.AddFormatedColumn('sectionincentive', 'Section (I)', 100, 'N2', 2).total = true;
            }

            if (hassectionprice) {
                igrid.AddFormatedColumn('sectionpricerange', 'Section Price (I)', 100, 'N3', 3).total = true;
            }
            if (hasproductprice) {
                igrid.AddFormatedColumn('productpricerangeincentive', 'Product Price (I)', 100, 'N3', 3).total = true;
            }
            if (hasemployeeagerangeincentive) {
                igrid.AddFormatedColumn('employeeagerangeincentive', 'Empoyee Age (I)', 100, 'N3', 3).total = true;
            }
            if (hasemployeetailoringincentive) {
                igrid.AddFormatedColumn('employeetailoringqty', 'Tailoring(Q)', 100, 'N3', 3).total = true;
                igrid.AddFormatedColumn('employeetailoringvalue', 'Tailoring(V)', 100, 'N3', 3).total = true;
                igrid.AddFormatedColumn('employeetailoringincentive', 'Tailoring(I)', 100, 'N3', 3).total = true;
            }
            if (hassectionage) {
                igrid.AddFormatedColumn('sectionagerangeincentive', 'Section Age (I)', 100, 'N3', 3).total = true;
            }
            if (hasproductage) {
                igrid.AddFormatedColumn('productagerangeincentive', 'Product Age (I)', 100, 'N3', 3).total = true;
            }
            if (hasproductagepricerangeincentive) {
                igrid.AddFormatedColumn('productagepricerangeincentive', 'Product Age/Price (I)', 100, 'N3', 3).total = true;
            }
            if (hascompanylevel) {
                igrid.AddFormatedColumn('companyincentive', 'Company Level (I)', 100, 'N3', 3).total = true;
            }
            if (hassalesslabincentive) {
                igrid.AddFormatedColumn('salesslabincentive', 'Sales Slab (I)', 100, 'N3', 3).total = true;
            }

            var addColumns = [];
            $.each($scope.IncentiveResult, function (k, v) {

                if (v.Salebyages != null) {
                    $.each(v.Salebyages, function (kk, vv) {

                        var found = addColumns.find(function (item) {
                            return item.name == vv.name;
                        });

                        if (typeof (found) == 'undefined' && found == null) {
                            addColumns.push(vv);
                        }
                    });
                }
            });

            if (addColumns && addColumns.length > 0) {

                igrid.AddFormatedColumn('amount', 'Total Incentive', 100, 'N2', 2).total = true;
                var groupColumns = [];
                groupColumns.push(
                    {
                        id: 'name',
                        name: 'Details',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });

                if (hassm) {
                    igrid.columns = [];
                    igrid.AddFormatedColumn('smcount', 'Qty', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('smvalue', 'Value', 100, 'N3', 3).total = true;
                    igrid.AddFormatedColumn('smincentive', 'Incentive', 100, 'N2', 2).total = true;
                    groupColumns.push(
                        {
                            id: 'stockmarker',
                            name: 'Stock Marker',
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                }

                if ($scope.searchmode == 'undefined' || $scope.searchmode == null || $scope.searchmode == 0 || $scope.searchmode == '12') {
                    igrid.columns = [];
                    igrid.AddFormatedColumn('incentivecount', 'Qty', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('itemsalevalue', 'Value', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('itemincentive', 'Incentive', 100, 'N2', 2).total = true;
                    groupColumns.push(
                        {
                            id: 'barcode',
                            name: '* Barcode',
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                }

                addColumns.sort(function (a, b) {
                    return (a.from) > (b.from) ? -1 : (a.from) < (b.from) ? 1 : 0;
                });

                $.each(addColumns, function (k, v) {

                    igrid.columns = [];
                    groupColumns.push(
                        {
                            id: 'sale_age_title' + k,
                            name: v.name,
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });

                    var column = igrid.AddCustomColumn('sale_age_value' + k, 'Value', 100, function (item, col) {
                        var taxitem = col.internal;
                        var val = col.getAgeSummary(item, taxitem);
                        return val > 0 ? val : "";
                    });

                    column.internal = v;
                    column.getAgeSummary = function (item, taxitem) {
                        if (item != null && item.Salebyages != null) {
                            var value = item.Salebyages.find(function (fitem) {
                                return fitem.name == taxitem.name;
                            });

                            if (typeof (value) != 'undefined' && value != null) {
                                return value.salevalue;
                            }
                        }
                        return 0;
                    };

                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var taxitem = col.internal;
                        return col.getAgeSummary(item, taxitem)
                    }

                    var column = igrid.AddCustomColumn('sale_age_qty' + k, 'Qty', 100, function (item, col) {
                        var taxitem = col.internal;
                        var val = col.getAgeSummary(item, taxitem);
                        return val > 0 ? val : "";
                    });

                    column.internal = v;
                    column.getAgeSummary = function (item, taxitem) {
                        if (item != null && item.Salebyages != null) {
                            var value = item.Salebyages.find(function (fitem) {
                                return fitem.name == taxitem.name;
                            });

                            if (typeof (value) != 'undefined' && value != null) {
                                return value.incentivecount;
                            }
                        }
                        return 0;
                    };

                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var taxitem = col.internal;
                        return col.getAgeSummary(item, taxitem)
                    }

                    column = igrid.AddCustomColumn('sale_age_incentive' + k, 'Incentive', 100, function (item, col) {
                        var taxitem = col.internal;
                        var val = col.getAgeSummary(item, taxitem);
                        return val > 0 ? val : "";
                    });

                    column.internal = v;
                    column.getAgeSummary = function (item, taxitem) {
                        if (item != null && item.Salebyages != null) {
                            var value = item.Salebyages.find(function (fitem) {
                                return fitem.name == taxitem.name;
                            });

                            if (typeof (value) != 'undefined' && value != null) {
                                return value.incentive;
                            }
                        }
                        return 0;
                    };

                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        var taxitem = col.internal;
                        return col.getAgeSummary(item, taxitem)
                    }
                });

                igrid.columns = groupColumns;
            }
            else {
                if (hassm) {
                    igrid.AddFormatedColumn('smvalue', 'S.Marker (V)', 100, 'N3', 3).total = true;
                    igrid.AddFormatedColumn('smincentive', 'S.Marker (I)', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('smcount', 'S.Marker (Q)', 100, 'N2', 2).total = true;
                }
                if ($scope.searchmode == 'undefined' || $scope.searchmode == null || $scope.searchmode == 0 || $scope.searchmode == '12') {
                    igrid.AddFormatedColumn('incentivecount', '* Barcode (Q)', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('itemsalevalue', '* Barcode (V)', 100, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('itemincentive', '* Barcode (I)', 100, 'N2', 2).total = true;
                }
                igrid.AddFormatedColumn('amount', 'Total Incentive', 100, 'N2', 2).total = true;
            }
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.IncentiveResult);
        }

        $scope.ShowNotes = function (igrid) {
            var seletedRows = igrid.GetSelectedRows();
            if (seletedRows.length == 1) {
                var notes = [];
                $.each(seletedRows[0].notes, function (k, v) {
                    notes.push(v.Key + ' = ' + v.Value);
                });
                $rootScope.ShowAlert('Notes', notes.join('<br>'));
            }
        }

        $scope.printInfyGrid = function (grid) {
            var strWindowFeatures = "menubar=yes,location=yes,resizable=yes,scrollbars=yes,status=yes";
            grid.grid.printPlugin.printToWindow(window.open('/print-grid.html', 'print_window', strWindowFeatures));
        }

        $scope.printDiv = function (div, title) {

            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
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

                angular.element('#apptitle').context.title = otitle;
            }, 100);

        }

        $scope.Export2CSV = function () {

            var hasbarcodeprefix = false;
            var hassm = false;
            var hasproductbyname = false;
            var hassectionprice = false;
            var hasproductprice = false;
            var hassectionage = false;
            var hasproductage = false;
            var hascompanylevel = false;
            var hassalesslabincentive = false;
            var hasemployeeagerangeincentive = false;
            var hasproductagepricerangeincentive = false;
            var hastargetincentive = false;
            var hasnotes = false;
            var hastargetamount = false;
            var hasrole = false;
            $.each($scope.IncentiveResult, function (k, v) {
                if (v.notes != null)
                    hasnotes = true;
                if (v.rolesalesmenincentive > 0 || v.rolesupervisorincentive > 0)
                    hasrole = true;
                if (v.targetamount > 0)
                    hastargetamount = true;
                if (v.targetincentive != 0)
                    hastargetincentive = true;
                if (v.prefixincentive != 0)
                    hasbarcodeprefix = true;
                if (v.smincentive != 0)
                    hassm = true;
                if (v.employeeagerangeincentive != 0)
                    hasemployeeagerangeincentive = true;
                if (v.productlevelincentive != 0)
                    hasproductbyname = true;
                if (v.sectionpricerange != 0)
                    hassectionprice = true;
                if (v.productpricerangeincentive != 0)
                    hasproductprice = true;
                if (v.sectionagerangeincentive != 0)
                    hassectionage = true;
                if (v.productagerangeincentive != 0)
                    hasproductage = true;
                if (v.companyincentive != 0)
                    hascompanylevel = true;
                if (v.salesslabincentive != 0)
                    hassalesslabincentive = true;
                if (v.productagepricerangeincentive != 0)
                    hasproductagepricerangeincentive = true;
            });

            var rows = [];
            var header = [];
            $.each($scope.IncentiveResult, function (k, v) {
                var cells = [];

                cells.push(v.employeecode);
                cells.push(v.employeename);
                cells.push(v.departmentid);
                cells.push(v.sectionid);
                cells.push(v.inchargeid);
                cells.push(v.days);
                cells.push(v.workingdays);
                if (k == 0)
                    header.push('employeecode,employeename,departmentid,sectionid,inchargeid,days,workingdays');

                if ($rootScope.UserInfo.appsettings.incentivemode < 4) {
                    cells.push(v.incentivepercentage);
                    cells.push(v.earnings);
                    cells.push(v.totalpay);
                    cells.push(v.distpercentage);
                    cells.push(v.allocation);
                    if (k == 0)
                        header.push('incentivepercentage,earnings,totalpay,distpercentage,allocation');
                }

                if (k == 0)
                    header.push('salesvalue');
                cells.push(v.salesvalue);

                if (hastargetamount) {
                    if (k == 0)
                        header.push('targetamount');

                    cells.push(v.targetamount);
                }

                if (hasproductbyname) {
                    cells.push(v.productlevelvalue);
                    cells.push(v.productlevelincentive);
                    if (k == 0)
                        header.push('productlevelvalue,productlevelincentive');
                }

                if (hasbarcodeprefix) {
                    cells.push(v.prefixvalue);
                    cells.push(v.prefixincentive);
                    if (k == 0)
                        header.push('prefixvalue,prefixincentive');

                }

                if (hassectionprice) {
                    cells.push(v.sectionpricerange);
                    if (k == 0)
                        header.push('sectionpricerange');
                }
                if (hasproductprice) {
                    cells.push(v.productpricerangeincentive);
                    if (k == 0)
                        header.push('productpricerangeincentive');
                }
                if (hasemployeeagerangeincentive) {
                    cells.push(v.employeeagerangeincentive);
                    if (k == 0)
                        header.push('employeeagerangeincentive');
                }

                if (hassectionage) {
                    cells.push(v.sectionagerangeincentive);
                    if (k == 0)
                        header.push('sectionagerangeincentive');
                }
                if (hasproductage) {
                    cells.push(v.productagerangeincentive);
                    if (k == 0)
                        header.push('productagerangeincentive');
                }
                if (hasproductagepricerangeincentive) {
                    cells.push(v.productagepricerangeincentive);
                    if (k == 0)
                        header.push('productagepricerangeincentive');
                }
                if (hascompanylevel) {
                    cells.push(v.companyincentive);
                    if (k == 0)
                        header.push('companyincentive');
                }
                if (hassalesslabincentive) {
                    cells.push(v.salesslabincentive);
                    if (k == 0)
                        header.push('salesslabincentive');
                }

                if (v.Salebyages && v.Salebyages.length > 0) {
                    var salebyageamount = 0;
                    var salebyageincentive = 0;
                    var salebyageqty = 0;
                    $.each(v.Salebyages, function (ks, kv) {
                        salebyageamount += kv.salevalue;
                        salebyageqty += kv.incentivecount;
                        salebyageincentive += kv.incentive;
                    });
                    cells.push(salebyageamount);
                    cells.push(salebyageincentive);
                    cells.push(salebyageqty);
                    if (k == 0)
                        header.push('salebyageamount,salebyageincentive,salebyageqty');
                }


                if (hassm) {
                    cells.push(v.smcount);
                    cells.push(v.smvalue);
                    cells.push(v.smincentive);
                    if (k == 0)
                        header.push('smcount,smvalue,smincentive');
                }

                if ($scope.searchmode == 'undefined' || $scope.searchmode == null || $scope.searchmode == 0 || $scope.searchmode == '12') {
              
                    //igrid.AddFormatedColumn('incentivecount', 'Qty', 100, 'N2', 2).total = true;
                    //igrid.AddFormatedColumn('itemsalevalue', 'Value', 100, 'N2', 2).total = true;
                    //igrid.AddFormatedColumn('itemincentive', 'Incentive', 100, 'N2', 2).total = true;

                    cells.push(v.incentivecount);
                    cells.push(v.itemsalevalue);
                    cells.push(v.itemincentive);
                    if (k == 0)
                        header.push('bcodecount,bcodevalue,bcodeincentive');
                }
                cells.push(v.amount);
                if (k == 0) {
                    header.push('amount');
                    rows.push(header.join(','));
                }
                rows.push(cells.join(','));
            });

            var blob = new Blob([rows.join('\r\n')],
                { type: "text/plain;charset=utf-8" });
            saveAs(blob, "Schedule.csv");
        }

        $scope.exporttoExcel = function (infygrid, title) {

            var title = $scope.SearchRef($scope.companyid, $scope.Companies);
            var dept = $scope.SearchRef($scope.departmentid, $scope.Departments, '');
            var loc = $scope.SearchRef($scope.locationid, $scope.Locations);

            var fromdate = $filter('date')($scope.fromdate, 'dd-MMM-yyyy');
            var todate = $filter('date')($scope.todate, 'dd-MMM-yyyy');
            var subtitle = loc + " Incentive Report for " + (fromdate == todate ? fromdate : (fromdate + " to " + todate));
            if (dept != '')
                subtitle += ' (' + dept + ' )';

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

        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);