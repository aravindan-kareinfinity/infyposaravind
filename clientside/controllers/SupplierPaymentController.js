var app = angular.module('InfinityLab');
app.controller('SupplierPaymentController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter, ngDialog) {

        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SupplierCompanyList = []
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.HideSupplier = true;
        $scope.ShowEntryLocation = false;
        $scope.ShowDays = false;
        $scope.ShowCreteria = true;
        $scope.PaymentApprovalByDues = false;
        $scope.summarygrid = null;
        $scope.PaymentSelectedCount = 0;
        $scope.SummaryList = [{ 'id': 'SUPPLIER', 'name': 'Supplier' },
        { 'id': 'JOBWORKER', 'name': 'Job Worker' },
        { 'id': 'AGE', 'name': 'Age' },
        { 'id': 'CITY', 'name': 'City' },
        { 'id': 'SECTION', 'name': 'Section' },
        { 'id': 'AGENT', 'name': 'Agent' },
        { 'id': 'MSME', 'name': 'MSME' }
        ];

        $scope.SearchModes = [{ 'id': 0, 'name': 'Supplier Payment' },
        { 'id': 1, 'name': 'Supplier Payment Summary' },
        { 'id': 2, 'name': 'Agent Commission' },
        { 'id': 3, 'name': 'Agent Commission Summary' }
        ];

        $scope.DisplayMode = [{ 'id': 'NORMAL', 'name': 'Normal' },
        { 'id': 'DETAIL', 'name': 'Detail' }];

        $scope.DayModes = [
            { 'id': 'DUE', 'name': 'Due Date' },
            { 'id': 'FULLDUE', 'name': 'Inv/DN Due' },
        { 'id': 'INVOICE', 'name': 'Invoice Date' },
        { 'id': 'ENTRY', 'name': 'Entry Date' },
        { 'id': 'AGE', 'name': 'Agewise' }];

        $scope.Agents = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agents = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
            $scope.ShowEntryLocation = $rootScope.UserInfo.appsettings.system_showroompurchase || false;
            $scope.PaymentApprovalByDues = $rootScope.UserInfo.appsettings.intray_supplierpayment || false;
            //$rootScope.BindLocation($scope, 'SupplierPayment.locationid');
        });

        $scope.City = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            results.push({ 'id': -1, 'name': '--LOCAL STATE--' });
            results.push({ 'id': -2, 'name': '--INTER STATE--' });
            $scope.City = results;
        });
        $scope.Section = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Section = results;
        });
        $scope.Banks = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.Banks = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
            //$rootScope.BindCompany($scope, 'SupplierPayment.companyid');
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.SupplierCompanyList = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompanyList = results;
        });

        $scope.SupplierCompanyChanged = function () {
            var cmpid = $scope.SupplierPayment.suppliercompanyid
            var cmp = $scope.SupplierCompanyList.find(function (item) {
                return item.id == cmpid;
            });
            if (cmp && cmp != null)
                $scope.SupplierPayment.supplierid = cmp.supplierid;
        }

        $scope.FilterSupplierCompany = function () {
            $scope.SupplierCompany = [];
            $.each($scope.SupplierCompanyList, function (k, v) {
                if (v.supplierid == $scope.SupplierPayment.supplierid)
                    $scope.SupplierCompany.push(v);
            });
            if ($scope.SupplierCompany.length == 1) {
                $timeout(function () {
                    $scope.SupplierPayment.suppliercompanyid = $scope.SupplierCompany[0].id;
                }, 10);
            }
        }


        $scope.AddedAmount = 0;
        $scope.AddedTaxAmount = 0;
        $scope.AdjustedAmount = 0;
        $scope.TotalInvoiceAmount = 0;
        $scope.TotalTax = 0;
        $scope.TDSAmount = 0;
        $scope.TotalChequeAmount = 0;
        $scope.TotalDiscountAmount = 0;
        $scope.TotalAmount = 0;
        $scope.PaymentModes =
            [{ 'id': 0, 'name': 'NEFT/RTGS' },
        { 'id': 1, 'name': 'Cheque/DD' },
        { 'id': 2, 'name': 'Cash' },
        { 'id': 3, 'name': 'Multiple Cheque/DD' },
        { 'id': 4, 'name': 'PDC' },
        { 'id': 5, 'name': 'Pending' },
        { 'id': 9, 'name': 'Bill Return' }
        ];

        $scope.Entity = {
            "id": 0,
            "mode": "NORMAL",
            "supplierid": 0,
            "suppliercompanyid": 0,
            "companyid": 0,
            "receiptno": null,
            "referencedetail": null,
            "paymentdate": null,
            "datatransmitted": false,
            "paidamount": 0.0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "Supplierpaymentinvoice": [],
            "bankcheques": []
        };
        $scope.SupplierPayment = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.AddedAmount = 0;
        $scope.AddedTaxAmount = 0;
        $scope.BalanceAmount = 0;
        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.ShowPendingBills = false;
        $scope.showSearchResult = false;
        $scope.SearchResult = [];
        $scope.ErrorMessage = "";
        $scope.LoadPendinginvoice = [];

        $scope.SupplierPayment.paymentdate = new Date();

        $scope.debitnote = {
            "background-color": "lightgray"
        };

        $scope.lrinvoice = {
            "background-color": "white"
        };

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.SupplierPayment = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.LoadPendinginvoice = [];
        }
        $scope.AddedAmountChanged = function () {
            $scope.AddedAmount = 0;
            $scope.AddedTaxAmount = 0;
            $scope.AdjustedAmount = 0;
            $scope.SelectedCount = 0;
            $.each($scope.LoadPendinginvoice, function (k, v) {

                if (!v.settlementamount.toString().endsWith('%')) {
                    v.settlementamount = parseFloat(v.settlementamount);
                    if (isNaN(v.settlementamount) || v.settlementamount < 0)
                        v.settlementamount = 0;

                    //if (!v.isinvoice && v.settlementamount > 0)
                    //    v.settlementamount = 0;

                    if (v.settlementamount > Math.abs(v.totalamount - v.paidamount))
                        v.settlementamount = Math.abs(v.totalamount - v.paidamount);

                    //if (!v.isinvoice && (v.settlementamount*-1) > (v.totalamount - v.paidamount))
                    //    v.settlementamount = (v.totalamount - v.paidamount)*-1;
                    if (v.isinvoice || v.isjobwork || v.iscreditnote) {
                        $scope.AddedAmount += v.settlementamount;
                        $scope.AddedTaxAmount += v.taxamount;
                    }
                    else {
                        $scope.AdjustedAmount += v.settlementamount;
                        $scope.AddedTaxAmount -= v.taxamount;
                    }
                    $scope.SelectedCount += v.settlementamount != 0 ? 1 : 0;
                }
            });
            $scope.SupplierPayment.adjustableamount = Math.round($scope.AddedAmount - $scope.AdjustedAmount);
        }


        $scope.tdspercentagechanged = function () {
            var tdspercentage = parseFloat($scope.SupplierPayment.tdspercentage);
            if (isNaN(tdspercentage)) tdspercentage = 0;
            if (tdspercentage != 0) {
                var amount = $scope.TotalInvoiceAmount - ($scope.TotalDiscountAmount + $scope.TotalTax);
                $scope.TDSAmount = (amount * tdspercentage / 100).round(0);
            }
            else {
                $scope.TDSAmount = 0;
            }
            $scope.SupplierPayment.tdsamount = $scope.TDSAmount;
        }

        $scope.tdsamountchanged = function () {
            var tdsamount = parseFloat($scope.SupplierPayment.tdsamount);
            if (isNaN(tdsamount)) tdsamount = 0;
            if (tdsamount != 0) {
                $scope.TDSAmount = tdsamount;
                $scope.SupplierPayment.tdspercentage = ((tdsamount / ($scope.TotalInvoiceAmount - $scope.TotalDiscountAmount)) * 100).round(2);
            }
            else {
                $scope.TDSAmount = 0;
                $scope.SupplierPayment.tdsamount = 0;
                $scope.SupplierPayment.tdspercentage = 0;
            }
        }

        $scope.Add2Plan = function () {
            var planlist = [];
            $.each($scope.LoadPendinginvoice, function (k, v) {
                if (v.settlementamount != 0) {
                    v.plannedpayment = v.settlementamount;
                    planlist.push(v);
                }
            });
            if (planlist.length > 0) {

                //var locationid = 0;
                //if ($scope.ShowEntryLocation) {
                //    if (typeof ($scope.SearchCondition.locationid) != 'undefined' &&
                //        $scope.SearchCondition.locationid)
                //        locationid = $scope.SearchCondition.locationid;
                //    else
                //        locationid = $scope.SupplierPayment.locationid;
                //}
                var request = {
                    "Item": {
                        'pendinginvoices': planlist,
                        'locationid': $scope.SupplierPayment.locationid
                    },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SupplierpaymentService/SetPlannedPayment",
                    data: srequest
                }).success(function (result) {
                    if (result != null) {
                        $.each(result, function (k, v) {
                            var exist = $scope.PlannedList.find(function (item) {
                                return item.suppliercompanyid = v.suppliercompanyid;
                            });
                            if (exist && exist != null) {
                                $scope.PlannedList.splice($scope.PlannedList.indexOf(exist), 1);
                            }
                            $scope.PlannedList.push(v);
                        });

                        if ($scope.PlanningLoaded && $scope.plannedgrid != null) {
                            $scope.plannedgrid.EnableSerial($scope.PlannedList, false);
                            $scope.plannedgrid.UpdateData($scope.PlannedList);
                        }
                    }
                });
            }
        }

        $scope.SearchItems = function () {

            $scope.SearchCondition.showSearchResult = true;
            $scope.ShowPendingBills = true;
            var rowtype = { 'rowtype': 0 };
            if ($scope.SupplierPayment.supplierid && $scope.SupplierPayment.supplierid > 0) {
                rowtype = $scope.Suppliers.find(function (item) {
                    return $scope.SupplierPayment.supplierid == item.id;
                });
            }
            else if ($scope.SupplierPayment.summarytype == 'JOBWORKER') {
                rowtype = { 'rowtype': 1 };
            }

            var request = {
                "Item": {
                    companyid: $scope.SupplierPayment.companyid,
                    suppliercompanyid: $scope.SupplierPayment.suppliercompanyid,
                    ageranges: $scope.SupplierPayment.ageranges,
                    cityid: $scope.SupplierPayment.cityid,
                    agentid: $scope.SupplierPayment.agentid,
                    sectionid: $scope.SupplierPayment.sectionid,
                    duedebitnotes: $scope.SupplierPayment.daymode == 'FULLDUE',
                    duefromdate: ($scope.SupplierPayment.daymode != 'undefined' && ($scope.SupplierPayment.daymode == 'DUE' || $scope.SupplierPayment.daymode == 'FULLDUE')) ? $scope.SupplierPayment.duefromdate : null,
                    duetodate: ($scope.SupplierPayment.daymode != 'undefined' && $scope.SupplierPayment.daymode == 'DUE' || $scope.SupplierPayment.daymode == 'FULLDUE') ? $scope.SupplierPayment.duetodate : null,
                    entryfromdate: ($scope.SupplierPayment.daymode != 'undefined' && $scope.SupplierPayment.daymode == 'ENTRY') ? $scope.SupplierPayment.duefromdate : null,
                    entrytodate: ($scope.SupplierPayment.daymode != 'undefined' && $scope.SupplierPayment.daymode == 'ENTRY') ? $scope.SupplierPayment.duetodate : null,
                    invfromdate: ($scope.SupplierPayment.daymode != 'undefined' && $scope.SupplierPayment.daymode == 'INVOICE') ? $scope.SupplierPayment.duefromdate : null,
                    invtodate: ($scope.SupplierPayment.daymode != 'undefined' && $scope.SupplierPayment.daymode == 'INVOICE') ? $scope.SupplierPayment.duetodate : null,
                    showmsme: ($scope.SupplierPayment.summarytype != 'undefined' && $scope.SupplierPayment.summarytype == 'MSME'),
                    locationid: $scope.SupplierPayment.locationid,
                    rowtype: rowtype ? rowtype.rowtype : 0
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SupplierpaymentService/SearchPendingPayment",
                data: srequest
            }).success(function (result) {
                //$scope.LoadPendinginvoice = result;
                $scope.LoadPendinginvoice = [];
                if (result == null || result.length == 0) {
                    $scope.ShowSummary(result);
                    $rootScope.Toaster.Info("No Results found.");
                    return;
                }
                $scope.SearchCondition.showSearchResult = result.length > 0;
                $scope.ShowPendingBills = result.length > 0;

                $scope.AdjustedAmount = 0;
                $scope.BalanceAmount = 0;
                $scope.AddedAmount = 0;
                $scope.AddedTaxAmount = 0;
                $scope.TotalAmount = 0;
                $scope.HideSupplier = typeof ($scope.SupplierPayment.suppliercompanyid) != 'undefined' &&
                    $scope.SupplierPayment.suppliercompanyid > 0;
                $scope.ShowDays = typeof ($scope.SupplierPayment.ageranges) != 'undefined' &&
                    $scope.SupplierPayment.ageranges != null && $scope.SupplierPayment.ageranges.length > 3;
                var suppliers = $scope.Suppliers;
                var agentcommission = false;
                var haspo = false;
                $.each(result, function (k, v) {
                    if (v.lrentryno == 'undefined' || v.lrentryno == null)
                        v.lrentryno = '';
                    if (v.slipno == 'undefined' || v.slipno == null)
                        v.slipno = '';

                    if (!$scope.HideSupplier) {
                        v.supplier = $scope.SearchRef(v.supplierid, $scope.Suppliers);
                    }
                    if (!haspo && v.pono && v.pono != '')
                        haspo = true;
                    v.settlementamount = v.plannedpayment;
                    v.fullvalue = v.settlementamount != 0;
                    v.balanceamount = (v.totalamount - (v.paidamount + v.tdsamount));
                    v.agentcommissionvalue = 0;
                    if (v.isinvoice || v.isjobwork || v.iscreditnote) {
                        $scope.BalanceAmount += v.balanceamount;
                        $scope.TotalAmount += v.balanceamount;
                        if (v.agentcommission > 0) {
                            agentcommission = true;
                            if ($rootScope.UserInfo.appsettings.system_comisnbeforediscount) {
                                v.agentcommissionvalue = (v.grossamount * v.agentcommission / 100).round(0);
                            }
                            else {
                                v.agentcommissionvalue = ((v.totalamount - v.taxamount) * v.agentcommission / 100).round(0);
                            }
                        }
                    }
                    else {
                        $scope.TotalAmount -= v.balanceamount;
                        v.style = $scope.debitnote;
                    }

                    if ((v.totalamount - v.paidamount - v.agentcommissionvalue) > 0)
                        $scope.LoadPendinginvoice.push(v);
                });

                $scope.ConfigInvoiceGrid(agentcommission, haspo);
                $scope.AddedAmountChanged();
                $scope.ShowSummary(result);
            }).error(function (result) {
            });
        };

        $scope.ShowSummary = function (sourcelist) {
            var groupbyfield = '';
            var groupbytitle = '';
            var groupbylist = [];
            $scope.SummaryList = [{ 'id': 'SUPPLIER', 'name': 'Supplier' },
            { 'id': 'JOBWORKER', 'name': 'Job Worker' },
            { 'id': 'AGE', 'name': 'Age' },
            { 'id': 'CITY', 'name': 'City' },
            { 'id': 'SECTION', 'name': 'Section' },
            { 'id': 'AGENT', 'name': 'Agent' }];

            if ($scope.SupplierPayment.summarytype == null
                || $scope.SupplierPayment.summarytype == 'undefined' || $scope.SupplierPayment.summarytype == '')
                return;

            switch ($scope.SupplierPayment.summarytype) {
                case 'SUPPLIER':
                case 'JOBWORKER':
                    groupbylist = $scope.SupplierCompanyList;
                    groupbyfield = 'suppliercompanyid';
                    groupbytitle = 'Supplier';
                    break;
                case 'AGE':
                    //groupbylist = $scope.SupplierCompanyList;
                    groupbyfield = 'days';
                    groupbytitle = 'Days';
                    break;
                case 'CITY':
                    groupbylist = $scope.City;
                    groupbyfield = 'cityid';
                    groupbytitle = 'City';
                    break;
                case 'SECTION':
                    groupbylist = $scope.Section;
                    groupbyfield = 'sectionid';
                    groupbytitle = 'Section';
                    break;
                case 'AGENT':
                    groupbylist = $scope.Agents;
                    groupbyfield = 'agentid';
                    groupbytitle = 'Agent';
                    break;
            }

            var groupby = [];

            if ($scope.SupplierPayment.summarytype == 'SUPPLIER' ||
                $scope.SupplierPayment.summarytype == 'JOBWORKER') {
                if ($scope.SupplierPayment.suppliercompanyid &&
                    $scope.SupplierPayment.suppliercompanyid > 0) {
                    var existing = sourcelist.find(function (item) {
                        return item.suppliercompanyid == $scope.SupplierPayment.suppliercompanyid;
                    });
                    if (typeof (existing) == 'undefined' || existing == null) {
                        var suppliercompany = $scope.SupplierCompanyList.find(function (item) {
                            return item.id == $scope.SupplierPayment.suppliercompanyid;
                        });
                        sourcelist.push({
                            'suppliercompanyid': suppliercompany.id,
                            'supplier': suppliercompany.name,
                            'isdebitnote': 0,
                            'iscreditnote': 0,
                            'balanceamount': 0,
                            'plannedpayment': 0,
                            'lrinvoiceid': 0
                        });
                    }
                }
            }

            $.each(sourcelist, function (k, v) {
                var existing = groupby.find(function (item) { return item.groupid == v[groupbyfield]; });
                if (typeof (existing) == 'undefined' || existing == null) {
                    existing = {
                        'groupid': v[groupbyfield], 'groupdisplay': v[groupbyfield],
                        'balanceamount': 0,
                        'plannedpayment': 0,
                        'childrens': []
                    };
                    groupby.push(existing);
                }
                existing.debitnote = v.isdebitnote ? v.balanceamount : 0;
                existing.balanceamount += v.isdebitnote ? v.balanceamount * -1 : v.balanceamount;
                existing.plannedpayment += v.isdebitnote ? v.plannedpayment * -1 : v.plannedpayment;
                existing.childrens.push(v);
            });


            if ($scope.summarygrid != null)
                $scope.summarygrid.destroy();

            var igrid = new InfyGrid('gridSummaryContainer', '400px', '100%', true, true, true, 280);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(groupby);
            if (groupbylist != null && groupbylist.length > 0)
                igrid.AddReferenceColumn('groupid', groupbytitle, 150, groupbylist);
            else
                igrid.AddSimpleColumn('groupid', groupbytitle, 150);
            igrid.AddFormatedColumn('balanceamount', 'Balance', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('plannedpayment', 'Planned', 90, 'N2', 2).total = true;

            if ($scope.SupplierPayment.companyid &&
                $scope.SupplierPayment.companyid != null &&
                $scope.SupplierPayment.companyid > 0) {
                igrid.AddButtonColumn('Print', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' class='btn btn-success btn-xs'><i class='fa fa-print' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if ($scope.SupplierPayment.locationid == undefined)
                    $scope.SupplierPayment.locationid = 0;

                if ($scope.SupplierPayment.companyid == undefined)
                    $scope.SupplierPayment.companyid = 0;

                $scope.$apply(function () {
                    if (command == 'Print') {
                        $rootScope.Download('SupplierpaymentService/PrintSummaryPDF/' + $scope.UserInfo.OrganizationID + '/'
                            + $scope.SupplierPayment.companyid + '/' + $scope.SupplierPayment.locationid + '/' + item.groupid + '/'
                            + $filter('date')(new Date($scope.SupplierPayment.duefromdate), 'dd-MM-yyyy') + '/'
                            + $filter('date')(new Date($scope.SupplierPayment.duetodate), 'dd-MM-yyyy'), 'paymentadvice.pdf', 'application/pdf');
                    }
                });
            });


            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                var companyids = [];
                $.each(range, function (k, v) {
                    companyids.push(rowmodel.infyGrid.grid.getDataItem(v.fromRow).groupid);
                });
                var agentid = 0;
                $.each($scope.LoadPendinginvoice, function (k, v) {
                    if (companyids.indexOf(v[groupbyfield]) >= 0) {
                        v.fullvalue = true;
                        v.settlementamount = v.fullvalue ? (v.totalamount - v.paidamount) : 0;
                        if (v.agentid && v.agentid > 0)
                            agentid = v.agentid;
                    }
                    else {
                        v.fullvalue = false;
                        v.settlementamount = v.fullvalue ? (v.totalamount - v.paidamount) : 0;
                    }
                });
                $scope.AddedAmountChanged();
                $timeout(function () {
                    $scope.SupplierPayment.agentid = agentid;
                    $scope.invgrid.UpdateAllTotals($scope.invgrid.grid);
                    $scope.invgrid.grid.invalidate();
                    $scope.invgrid.grid.render();
                }, 10);
            };

            $scope.summarygrid = igrid;
            $scope.summarygrid.UpdateData(groupby);
        }

        $scope.PlanningLoaded = false;
        $scope.PlannedList = [];
        $scope.LoadPlanned = function () {
            if (!$scope.PlanningLoaded) {
                $scope.PlanningLoaded = true;
                $scope.ShowCreteria = false;
                var request = {
                    "Item": { 'locationid': $scope.SupplierPayment.locationid },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "SupplierpaymentService/SearchPlannedPayment",
                    data: srequest
                }).success(function (results) {
                    $scope.ConfigPlannedGrid();
                    $scope.PlannedList = results;
                    $scope.plannedgrid.EnableSerial($scope.PlannedList, false);
                    $scope.plannedgrid.UpdateData($scope.PlannedList);
                });
            }
        }

        $scope.AddMultipleCheque = function () {
            $scope.SupplierPayment.bankcheques.push({
                bankid: $scope.SupplierPayment.bankid,
                chequedate: $scope.SupplierPayment.chequedate,
                chequeno: $scope.SupplierPayment.referencedetail,
                amount: parseFloat($scope.SupplierPayment.chequeamount),
                sourcetype: 'SPMT',
                companyid: $scope.SupplierPayment.companyid,
                accountname: $scope.SupplierCompanyList.find(function (item) {
                    return item.id == $scope.SupplierPayment.suppliercompanyid;
                }).name
            });
            $scope.TotalChequeAmount += parseFloat($scope.SupplierPayment.chequeamount);
        }

        $scope.ManagePaymentItem = function (action, item) {
            if (action == "DeleteInvoice") {
                $scope.TotalInvoiceAmount -= $rootScope.GetNumber(item.paidamount);
                $scope.TotalTax -= $rootScope.GetNumber(item.taxamount);
                $scope.TotalDiscountAmount -= $rootScope.GetNumber(item.discountamount);
                $scope.SupplierPayment.Supplierpaymentinvoice.splice($scope.SupplierPayment.Supplierpaymentinvoice.indexOf(item), 1);
            } else
                if (action == "Delete") {
                    $scope.TotalChequeAmount -= item.amount;
                    $scope.SupplierPayment.bankcheques.splice($scope.SupplierPayment.bankcheques.indexOf(item), 1);
                } else if (action == "Edit") {
                    $scope.SupplierPayment = {
                        'supplierid': item.supplierid
                    };
                    $scope.FilterSupplierCompany();
                    var request = {
                        "Item": { 'id': item.id },
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "SupplierpaymentService/Search",
                        data: srequest
                    }).success(function (result) {
                        if (result.length == 1) {

                            $scope.AddedAmount = 0;
                            $scope.AddedTaxAmount = 0;
                            $scope.AdjustedAmount = 0;
                            $scope.TotalInvoiceAmount = 0;
                            $scope.TDSAmount = 0;
                            $scope.TotalChequeAmount = 0;
                            $scope.TotalDiscountAmount = 0;
                            $scope.TotalAmount = 0;
                            $scope.TotalTax = 0;

                            $scope.SupplierPayment = result[0];
                            if ($scope.SupplierPayment.paymentmode == 3) {
                                $scope.TotalChequeAmount = $scope.SupplierPayment.paidamount;
                            }
                            else if ($scope.SupplierPayment.bankcheques != null &&
                                $scope.SupplierPayment.bankcheques.length == 1) {
                                $scope.SupplierPayment.bankid = $scope.SupplierPayment.bankcheques[0].bankid;
                                $scope.SupplierPayment.chequedate = $scope.SupplierPayment.bankcheques[0].chequedate;
                                $scope.SupplierPayment.referencedetail = $scope.SupplierPayment.bankcheques[0].chequeno;
                                $scope.SupplierPayment.paidamount = $scope.SupplierPayment.bankcheques[0].amount;
                            }
                            var agentcommission = false;
                            var intagrmt = false;
                            $.each($scope.SupplierPayment.Supplierpaymentinvoice, function (k, v) {
                                if (v.islrinvoicereturn)
                                    v.style = $scope.debitnote;
                                $scope.TotalInvoiceAmount += $rootScope.GetNumber(v.paidamount);
                                $scope.TotalTax += $rootScope.GetNumber(v.taxamount);
                                $scope.TotalDiscountAmount += $rootScope.GetNumber(v.discountamount);
                                if (v.agentcommission > 0) {
                                    agentcommission = true;
                                }
                                if (v.intpay > 0 || v.intreceive > 0) {
                                    agentcommission = true;
                                }
                            });
                            $scope.SearchCondition.showSearchResult = false;
                            $scope.ShowSearch = false;
                            $scope.Navigation.SetMode('EDIT');

                            $timeout(function () {
                                $scope.ShowSupplierPaymentInvoices(agentcommission, intagrmt);
                            }, 10);
                        }
                    }).error(function (result) {
                    });
                }
        }

        $scope.DiscountPercentChanged = function (item) {
            if (!isNaN(item.discountamountpercent)) {
                item.discountamount = Math.round(item.paidamount * item.discountamountpercent / 100);
                $scope.DiscountAmountChanged();
            }
            else if (item.discountamount != 0) {
                item.discountamount = 0;
                $scope.DiscountAmountChanged();
            }
        }

        $scope.DiscountAmountChanged = function () {
            $scope.TotalInvoiceAmount = 0;
            $scope.TotalDiscountAmount = 0;
            $scope.TotalTax = 0;
            $.each($scope.SupplierPayment.Supplierpaymentinvoice, function (k, v) {
                $scope.TotalInvoiceAmount += $rootScope.GetNumber(v.paidamount);
                $scope.TotalDiscountAmount += $rootScope.GetNumber(v.discountamount);
                $scope.TotalTax += $rootScope.GetNumber(v.taxamount);
            });
            $scope.TotalInvoiceAmount = $scope.TotalInvoiceAmount.round(2);
        }

        $scope.Apply = function () {
            if ($scope.SupplierPayment.adjustableamount != 0) {
                var amount = $scope.SupplierPayment.adjustableamount;

                $.each($scope.LoadPendinginvoice, function (k, v) {
                    if (v.isdebitnote) {
                        v.settlementamount = (v.totalamount - v.paidamount);
                        amount -= v.settlementamount;
                    }
                    else if (amount >= 0) {
                        var balanceamt = v.totalamount - v.paidamount;
                        v.settlementamount = parseFloat(v.settlementamount);
                        if (isNaN(v.settlementamount))
                            v.settlementamount = 0;

                        if (balanceamt > amount) {
                            v.settlementamount += amount;
                            amount = 0;
                        }
                        else {
                            v.settlementamount += balanceamt;
                            amount -= balanceamt;
                        }
                    }
                });

                $scope.AddedAmountChanged();
                $scope.invgrid.UpdateData($scope.LoadPendinginvoice);
            }
        }
        $scope.LoadPayament = function () {

            if ($scope.SupplierPayment.suppliercompanyid == null || $scope.SupplierPayment.suppliercompanyid == 0) {
                $rootScope.Toaster.Info("Please select suppliercompany");
                $scope['setFocussuppliercompanyid']();
                return;
            }

            if ($scope.SupplierPayment.companyid == null || $scope.SupplierPayment.companyid == 0) {
                $rootScope.Toaster.Info("Please select company");
                $scope['setFocusfirst']();
                return;
            }

            if ($scope.PaymentApprovalByDues) {
                var suppliercompany = $scope.SupplierCompanyList.find(function (item) {
                    return item.id == $scope.SupplierPayment.suppliercompanyid;
                });
                if (suppliercompany && suppliercompany.supplierid && suppliercompany.supplierid > 0) {
                    var supplier = $scope.Suppliers.find(function (item) {
                        return item.id == suppliercompany.supplierid;
                    });
                    if (supplier && supplier != null) {
                        var soldpercentage = supplier.soldpercentage;
                        var creditdays = supplier.creditdays;
                        var exceptionList = [];
                        $.each($scope.LoadPendinginvoice, function (k, v) {
                            if (v.settlementamount != 0) {
                                if (soldpercentage > 0 && v.soldpercent < soldpercentage) {
                                    exceptionList.push(v.invoiceno + ' sold(%) ' + v.soldpercent + 'only, expected %(' + soldpercentage + ')');
                                } else if (creditdays > 0 && v.ndays < creditdays) {
                                    exceptionList.push(v.invoiceno + ' due days ' + v.ndays + ' only, expected ' + creditdays + ' days');
                                }
                            }
                        });
                        $rootScope.ShowAlert('Warning', exceptionList.join('<br>'));
                        var specialpermission = $rootScope.UserInfo && $rootScope.UserInfo.p_SPECIALSUPPLIERPAYMENT && $rootScope.UserInfo.p_SPECIALSUPPLIERPAYMENT.view;
                        if (!specialpermission) {
                            $rootScope.Toaster.Error("Please contact your manager for special approval");
                            return;
                        }
                    }
                }
            }

            var amount = 0;
            var invamt = 0;
            var debitamt = 0;
            var tdsamount = 0;
            $.each($scope.LoadPendinginvoice, function (k, v) {
                if (v.settlementamount != 0 && $scope.SupplierPayment.suppliercompanyid == v.suppliercompanyid) {
                    amount += v.isinvoice || v.isjobwork || v.iscreditnote ? v.settlementamount : (v.settlementamount * -1);
                    invamt += v.isinvoice || v.isjobwork || v.iscreditnote ? v.settlementamount : 0;
                    debitamt += v.isinvoice || v.isjobwork || v.iscreditnote ? 0 : (v.settlementamount * -1);
                    tdsamount += v.tdsamount;
                }
            });

            amount = amount.round(2);
            invamt = invamt.round(2);
            debitamt = debitamt.round(2);
            tdsamount = tdsamount.round(2);

            if (amount == 0 && invamt == 0 && debitamt == 0) {
                $rootScope.Toaster.Error("Please select the correct supplier and relevents invoices");
                return;
            }
            var agentcommission = false;
            var intagrmt = 0;
            if (tdsamount != 0) {
                $scope.SupplierPayment.tdsoninv = true;
                $scope.SupplierPayment.hastds = true;
                $scope.SupplierPayment.tdsamount = tdsamount;

                var tdsinvoice = $scope.LoadPendinginvoice.find(function (item) {
                    return item.tdsamount > 0 && item.tdsledgerid > 0;
                });
                if (tdsinvoice && tdsinvoice != null)
                    $scope.SupplierPayment.tdsledgerid = tdsinvoice.tdsledgerid;
            }
            else {
                $scope.SupplierPayment.tdsoninv = false;
                $scope.SupplierPayment.hastds = false;
                $scope.SupplierPayment.tdsamount = 0;
            }
            $.each($scope.LoadPendinginvoice, function (k, v) {
                if (v.settlementamount != 0) {

                    var existing = $scope.SupplierPayment.Supplierpaymentinvoice.find(function (item) {
                        return item.invoiceid == v.lrinvoiceid && item.invoicetype == v.invoicetype;
                    });

                    if (existing != null)
                        $scope.SupplierPayment.Supplierpaymentinvoice.splice($scope.SupplierPayment.Supplierpaymentinvoice.indexOf(existing), 1);

                    $scope.SupplierPayment.Supplierpaymentinvoice.push({
                        'supplierid': $scope.SupplierPayment.suppliercompanyid,
                        'invoiceid': v.lrinvoiceid,
                        'invoicetype': v.invoicetype,
                        'companyid': v.companyid,
                        'invoiceno': v.invoiceno,
                        'invoicedate': v.invoicedate,
                        'tdsamount': v.tdsamount,
                        'tdsledgerid': v.tdsledgerid,
                        'paidamount': v.isinvoice || v.isjobwork || v.iscreditnote ? v.settlementamount : v.settlementamount * -1,
                        'taxamount': v.taxamount,
                        'discountamount': 0,
                        'isinvoice': v.isinvoice,
                        'isjobwork': v.isjobwork,
                        'isdebitnote': v.isdebitnote,
                        'iscreditnote': v.iscreditnote,
                        'islrinvoicereturn': v.isdebitnote,
                        'supplier': v.supplier,
                        'supplierid': v.supplierid,
                        'style': v.isinvoice || v.isjobwork ? null : $scope.debitnote,
                        'agentcommission': v.agentcommission,
                        'agentid': v.agentid,
                        'agentcommissionvalue': v.agentcommissionvalue,
                        'intpay': v.intpay,
                        'intreceive': v.intreceive
                    });
                    if (v.agentcommission > 0 && v.agentcommissionvalue > 0) {
                        agentcommission = true;
                    }
                    if (v.intpay > 0 || v.intreceive > 0) {
                        intagrmt = true;
                    }
                }
            });

            $scope.DiscountAmountChanged();
            $scope.SearchCondition.showSearchResult = false;
            $timeout(function () {
                $scope.ShowSupplierPaymentInvoices(agentcommission, intagrmt);
            }, 10)
        }

        $scope.paymentgrid = null;
        $scope.ShowSupplierPaymentInvoices = function (agentcommission, hasintagrmnt) {


            if ($scope.paymentgrid != null)
                $scope.paymentgrid.destroy();

            var igrid = new InfyGrid('gridsupplierpaymentinvoices', '370px', '100%', true, true, true, 80);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SupplierPayment.Supplierpaymentinvoice);
            if (!$scope.HideSupplier) {
                igrid.AddSimpleColumn('supplier', 'Supplier Name', 160);
            }

            var column = igrid.AddCustomColumn('invoiceno', 'Inv. No', 100, function (item, col) {
                if ((item.isdebitnote || item.iscreditnote) && item.coreinvoiceno != null) {
                    return item.coreinvoiceno + '(' + item.invoiceno + ')';
                }
                return item.invoiceno;
            });
            column.enablefilter = true;

            igrid.AddSimpleColumn('invoicetype', 'Invoice Type', 80);
            igrid.AddFormatedColumn('invoicedate', 'Inv. Date', 100, 'dd/MM/yyyy');

            if ($scope.SupplierPayment.mode == 'DETAIL') {
                igrid.AddSimpleColumn('lrentryno', 'Entry No', 100);
                igrid.AddSimpleColumn('slipno', 'Slip No', 100);
                igrid.AddFormatedColumn('entrydate', 'Ent. Date', 100, 'dd/MM/yyyy');
            }

            igrid.SubscribeClickEvent();
            var dcolumn = igrid.AddColumn(
                {
                    id: "discountamountpercent", name: "Discount %", field: "discountamountpercent",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            dcolumn.cssClass = 'rightalign';
            dcolumn = igrid.AddColumn(
                {
                    id: "discountamount", name: "Discount", field: "discountamount",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            dcolumn.cssClass = 'rightalign';
            dcolumn.total = true;
            igrid.AddFormatedColumn('paidamount', 'Amount', 80, 'N2', 2).total = true;
            if (agentcommission) {
                igrid.AddFormatedColumn('agentcommission', 'A.Comm %', 60, 'N2', 2);
                igrid.AddFormatedColumn('agentcommissionvalue', 'A.Comm', 60, 'N2', 2).total = true;
            }

            if (hasintagrmnt) {
                igrid.AddFormatedColumn('intreceive', 'Interest(+)', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('intpay', 'Interest(-)', 60, 'N2', 2).total = true;
            }

            igrid.EnableTotal();
            igrid.options.autoEdit = true;

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.PaymentSelectedCount = range.length;
                }, 10);
            };

            igrid.Initialize();

            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                if (item.iscreditnote)
                    return { cssClasses: 'igridcreditnoterow' };
                if (!item.isinvoice && !item.isjobwork) {
                    return { cssClasses: 'igriddebitnoterow' };
                }
            };

            igrid.grid.onCellChange.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'discountamountpercent') {
                    item = args.grid.getDataItem(args.row);
                    $timeout(function () {
                        $scope.DiscountPercentChanged(item);
                        igrid.UpdateAllTotals(args.grid);
                        args.grid.invalidate();
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'discountamount') {
                    item = args.grid.getDataItem(args.row);
                    $timeout(function () {
                        var damt = parseFloat(item.discountamount);
                        if (!isNaN(damt)) {
                            item.discountamountpercent = (item.discountamount / item.paidamount * 100).round(2);
                        }
                        $scope.DiscountAmountChanged(item);
                        igrid.UpdateAllTotals(args.grid);
                        args.grid.invalidate();
                        args.grid.render();
                    }, 10);
                }
            });

            $scope.paymentgrid = igrid;
            $scope.paymentgrid.UpdateData($scope.SupplierPayment.Supplierpaymentinvoice, '_id');
        }

        $scope.ShowDiscount = function () {
            $rootScope.Prompt('Discount %', 'Discount', function (discount) {
                discountpercentage = parseFloat(discount);
                if (isNaN(discountpercentage)) {
                    discountpercentage = 0;
                }

                $.each($scope.SupplierPayment.Supplierpaymentinvoice, function (k, v) {
                    if (discountpercentage == 0) {
                        v.discountamount = 0;
                        v.discountamountpercent = 0;
                    }
                    else {
                        v.discountamount = (v.paidamount * discountpercentage / 100).round(0);
                        v.discountamountpercent = discountpercentage;
                    }
                });

                $timeout(function () {
                    $scope.DiscountAmountChanged();
                    $scope.paymentgrid.UpdateData($scope.SupplierPayment.Supplierpaymentinvoice, '_id');
                }, 10);
            })
        }

        $scope.AsssignPaidAmount = function () {
            $scope.SupplierPayment.paidamount = Math.round($scope.TotalInvoiceAmount - $scope.TotalDiscountAmount - $scope.TDSAmount);
        }
        $scope.DeleteItems = function () {
            var items = [];
            $.each(this.paymentgrid.GetSelectedRows(), function (k, v) {
                items.push(v);
            });
            $.each(items, function (k, v) {
                $scope.SupplierPayment.Supplierpaymentinvoice.splice($scope.SupplierPayment.Supplierpaymentinvoice.indexOf(v), 1);
            })
            var agentcommission = false;
            $scope.TotalInvoiceAmount = 0;
            $scope.TotalDiscountAmount = 0;
            $scope.TotalTax = 0;
            $scope.tdspercentagechanged();
            $.each($scope.SupplierPayment.Supplierpaymentinvoice, function (k, v) {
                if (v.islrinvoicereturn)
                    v.style = $scope.debitnote;
                $scope.TotalInvoiceAmount += $rootScope.GetNumber(v.paidamount);
                $scope.TotalDiscountAmount += $rootScope.GetNumber(v.discountamount);
                $scope.TotalTax += $rootScope.GetNumber(v.taxamount);
                if (v.agentcommission > 0) {
                    agentcommission = true;
                }
            });
            $scope.TotalInvoiceAmount = $scope.TotalInvoiceAmount.round(2);
            $scope.paymentgrid.UpdateData($scope.SupplierPayment.Supplierpaymentinvoice, '_id');
        }

        $scope.Save = function () {
            $scope.SupplierPayment.instrumentdate = $scope.SupplierPayment.chequedate;
            if ($scope.SupplierPayment.paymentmode == 3) {
                $scope.SupplierPayment.paidamount = $scope.TotalChequeAmount;
            }
            else {
                $scope.SupplierPayment.bankcheques = [];
                $scope.SupplierPayment.bankcheques.push({
                    bankid: $scope.SupplierPayment.bankid,
                    chequedate: $scope.SupplierPayment.chequedate,
                    chequeno: $scope.SupplierPayment.referencedetail,
                    amount: parseFloat($scope.SupplierPayment.paidamount),
                    sourcetype: 'SPMT',
                    companyid: $scope.SupplierPayment.companyid,
                    accountname: $scope.SupplierCompanyList.find(function (item) {
                        return item.id == $scope.SupplierPayment.suppliercompanyid;
                    }).name
                });
            }

            if ($scope.SupplierPayment.paidamount == null || $rootScope.GetNumber($scope.SupplierPayment.paidamount) <= 0) {
                if ($scope.SupplierPayment.Supplierpaymentinvoice == null || $scope.SupplierPayment.Supplierpaymentinvoice.length == 0) {
                    $scope['setFocuspaidamount']();
                    $rootScope.Toaster.Info("Please enter paidamount");
                    return;
                }
            }

            if ($scope.SupplierPayment.paymentdate == null || $scope.SupplierPayment.paymentdate == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info("Please select paymentdate");
                return;
            }

            if ($scope.SupplierPayment.supplierid == null || $scope.SupplierPayment.supplierid == 0) {
                $scope['setFocussupplierid']();
                $rootScope.Toaster.Info("Please select supplier");
                return;
            }
            if ($scope.SupplierPayment.suppliercompanyid == null || $scope.SupplierPayment.suppliercompanyid == 0) {
                $scope['setFocussuppliercompanyid']();
                $rootScope.Toaster.Info("Please select suppliercompany");
                return;
            }

            if ($scope.SupplierPayment.companyid == null || $scope.SupplierPayment.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select company");
                return;
            }

            var invoiceValue = ($scope.TotalInvoiceAmount - $scope.TotalDiscountAmount - $rootScope.GetNumber($scope.SupplierPayment.tdsamount)).round(2);
            if ($scope.SupplierPayment.tdsoninv)
                invoiceValue = ($scope.TotalInvoiceAmount - $scope.TotalDiscountAmount).round(2);

            $scope.SupplierPayment.rounding = $scope.SupplierPayment.paidamount - invoiceValue;
            if (Math.abs($scope.SupplierPayment.rounding) > 1) {
                $rootScope.Toaster.Info("Paid Amount mismatched, paid amount altered, validate before save.");
                return;
            }

            var bank = $scope.Banks.find(function (item) {
                return item.id == $scope.SupplierPayment.bankid;
            });
            $scope.SupplierPayment.bankname = bank && bank != null ? bank.name : "";
            request = {
                "Item": $scope.SupplierPayment,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SupplierpaymentService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with Reference No : " + result.receiptno);
                    $scope.New();
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }
        $scope.SearchCondition = {
            supplierid: 0,
            paymentdate: new Date(),
            lrinvoiceno: "",
            showSearchResult: true
        }

        $scope.New = function () {
            $scope.SupplierPayment = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchCondition.showSearchResult = true;
            $scope.ShowSearch = false;
            $scope.LoadPendinginvoice = [];
            $scope.TotalAmount = 0;
            $scope.AddedAmount = 0;
            $scope.AddedTaxAmount = 0;
            $scope.AdjustedAmount = 0;
            $scope.TotalInvoiceAmount = 0;
            $scope.TotalTax = 0;
            $scope.TotalChequeAmount = 0;
            $scope.TotalDiscountAmount = 0;
            $scope.TDSAmount = 0;
            if ($scope.paymentgrid != null)
                $scope.paymentgrid.destroy();
            if ($scope.summarygrid != null)
                $scope.summarygrid.destroy();

            $scope.LoadPendinginvoice = [];
            $timeout(function () {
                $scope.ConfigInvoiceGrid(false);
            }, 10);
        }

        $scope.Search = function () {

            var searchmode = parseInt($scope.SearchCondition.searchmode);
            if (isNaN(searchmode)) searchmode = 0;
            $scope.SearchCondition.searchmode = searchmode;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SupplierpaymentService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                switch (searchmode) {
                    case 0:
                        $scope.SupplierPaymentConfig();
                        break;
                    case 1:
                        $scope.SupplierPaymentSummaryConfig();
                        break;
                    case 2:
                        $scope.AgentPaymentConfig();
                        break;
                    case 3:
                        $scope.AgentPaymentSummaryConfig();
                        break;
                }
            }).error(function (result) {
            });
        }


        $scope.AmountInPercentage = function (item) {
            if (item.settlementamount.toString().endsWith('%')) {
                var percent = parseFloat(item.settlementamount.toString().replace('%', ''));
                if (isNaN(percent)) percent = 0;
                item.settlementamount = Math.round(item.totalamount * percent / 100);
                if (item.settlementamount > (item.totalamount - item.paidamount)) {
                    item.settlementamount = item.totalamount - item.paidamount;
                }
                $scope.AddedAmountChanged();
            }
        }

        $scope.ConfigPlannedGrid = function () {
            if ($scope.plannedgrid != null)
                $scope.plannedgrid.destroy();

            var igrid = new InfyGrid('gridPlanContainer', '370px', '100%', true, true, true, 0);
            igrid.EnableSerial($scope.LoadPendinginvoice);
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 150, $scope.Suppliers);
            igrid.AddFormatedColumn('balance', 'Total', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'Ready', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('dn', 'DN', 70, 'N2', 2).total = true;

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"

            igrid.AddButtonColumn('action', 'ACTION', 130, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit') {

                        $scope.SupplierPayment.companyid = 0;
                        $scope.SupplierPayment.suppliercompanyid = args.item.suppliercompanyid;
                        $scope.SupplierCompanyChanged();
                        $scope.SupplierPayment.companyid = args.item.companyid;
                        $scope.SupplierPayment.locationid = args.item.locationid;
                        $scope.SupplierPayment.ageranges = '';
                        $scope.SupplierPayment.cityid = 0;
                        $scope.SupplierPayment.sectionid = 0;
                        $scope.ShowCreteria = true;
                        $scope.SearchItems();
                    }
                    else if ($(e.target).val() == 'Delete') {

                        $.each(args.item.invoicelist, function (k, v) {
                            v.plannedpayment = 0;
                        });
                        //var locationid = 0;
                        //if ($scope.ShowEntryLocation) {
                        //    if (typeof ($scope.SearchCondition.locationid) != 'undefined' &&
                        //        $scope.SearchCondition.locationid)
                        //        locationid = $scope.SearchCondition.locationid;
                        //    else
                        //        locationid = $scope.SupplierPayment.locationid;
                        //}
                        var request = {
                            "Item": {
                                'pendinginvoices': args.item.invoicelist,
                                'locationid': $scope.SupplierPayment.locationid
                            },
                            "securitytoken": "String content"
                        }

                        $scope.PlannedList.splice($scope.PlannedList.indexOf(args.item));
                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "SupplierpaymentService/SetPlannedPayment",
                            data: srequest
                        }).success(function (result) {
                            $scope.plannedgrid.EnableSerial($scope.PlannedList, false);
                            $scope.plannedgrid.UpdateData($scope.PlannedList);
                        });
                    }
                });
            });

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.plannedgrid = igrid;
        }


        $scope.ConfigInvoiceGrid = function (showagentcommission, haspo) {
            if ($scope.invgrid != null)
                $scope.invgrid.destroy();

            var igrid = new InfyGrid('gridinvoiceContainer', '370px', '100%', true, false, true, 80);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.LoadPendinginvoice);
            //igrid.AddSimpleColumn('invoicetype', 'Type', 40);

            if (!$scope.HideSupplier) {
                igrid.AddReferenceColumn('supplierid', 'Supplier Name', 160, $scope.Suppliers);
            }

            var column = igrid.AddCustomColumn('invoiceno', 'Inv. No', 80, function (item, col) {
                if ((item.isdebitnote || item.iscreditnote) && item.coreinvoiceno != null) {
                    return item.invoiceno + '(' + item.coreinvoiceno + ')';
                }
                return item.invoiceno;
            });

            igrid.AddSimpleColumn('invoicetype', 'Invoice Type', 80);
            igrid.AddFormatedColumn('invoicedate', 'Inv. Date', 75, 'dd/MM/yyyy');
            if ($scope.SupplierPayment.mode == 'DETAIL') {
                igrid.AddSimpleColumn('lrentryno', 'Entry No', 100);
                igrid.AddSimpleColumn('slipno', 'Slip No', 100);
                igrid.AddFormatedColumn('entrydate', 'Ent. Date', 80, 'dd/MM/yyyy');
                igrid.AddFormatedColumn('duedate', 'Due Date', 80, 'dd/MM/yyyy');
            }

            //if (haspo) {
            //    igrid.AddSimpleColumn('pono', 'PO No', 100);
            //    igrid.AddFormatedColumn('podate', 'PO Date', 100, 'dd/MM/yyyy');
            //    igrid.AddButtonColumn('3waymatch', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='3waymatch' class='btn btn-success btn-xs'><i class='fa fa-shield' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            //}

            if ($scope.SupplierPayment.ageranges == 'undefined' || $scope.SupplierPayment.ageranges == null)
                igrid.AddFormatedColumn('ndays', 'Days', 40, 'N0', 0);
            else
                igrid.AddFormatedColumn('days', 'Days', 40, 'N0', 0);
            igrid.AddFormatedColumn('invoicediscount', 'Discount', 60, 'N2', 2).total = true;
            var column = igrid.AddCustomColumn('invdiscountpercentage', '%  ', 40, function (item, col) {
                if (item.invoicediscount > 0) {
                    if (item.totalamount > 0) {
                        return (item.invoicediscount / item.totalamount * 100).toFixed(2);
                    }
                }
                return '';
            });

            var column = igrid.AddCustomColumn('totalamount', 'Total', 90, function (item, col) {
                return (item.isdebitnote) ? item.totalamount * -1 : item.totalamount;
            });
            column.sortable = true;
            column.total = true;
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                return (item.isdebitnote) ? item.totalamount * -1 : item.totalamount;
            }


            var tdsinvoice = $scope.LoadPendinginvoice.find(function (item) {
                return item.tdsamount && item.tdsamount > 0
            });

            if (tdsinvoice && tdsinvoice != null) {
                igrid.AddFormatedColumn('tdsamount', 'TDS', 80, 'N2', 2).total = true;
            }

            igrid.AddFormatedColumn('paidamount', 'Paid', 80, 'N2', 2).total = true;
            var column = igrid.AddCustomColumn('balanceamount', 'Balance', 90, function (item, col) {
                return (item.isdebitnote) ? item.balanceamount * -1 : item.balanceamount;
            });
            column.sortable = true;
            column.total = true;
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                return (item.isdebitnote) ? item.balanceamount * -1 : item.balanceamount;
            }

            //igrid.AddFormatedColumn('pendingpercent', 'Pending %', 50, 'N2', 2);
            if ($scope.HideSupplier)
                igrid.AddFormatedColumn('soldpercent', 'Sold %', 50, 'N2', 2);
            if (showagentcommission) {
                igrid.AddFormatedColumn('agentcommission', 'A.Comm %', 80, 'N2', 2);
                igrid.AddFormatedColumn('agentcommissionvalue', 'A.Comm', 80, 'N2', 2).total = true;
            }

            igrid.SubscribeClickEvent();
            var dcolumn = igrid.AddCustomColumn('fullvalue',
                '<input type="checkbox" style="z-index: auto;margin-right:10px">', 50, function (item, col) {
                    if (item.fullvalue)
                        return '<input type="checkbox" style="z-index: auto;margin-left:15px" checked>';
                    else
                        return '<input type="checkbox" style="z-index: auto;margin-left:15px">';
                });
            dcolumn.sortable = true;

            if ($scope.HideSupplier) {
                var dcolumn = igrid.AddCustomColumn('warning',
                    "<i class='fa-warning' style='font: normal normal normal 12px / 1 FontAwesome'></i>", 40, function (item, col) {
                        if (item.warn && item.warn != '')
                            return '<button type="button" style="width:22px;height:20px;margin-right:5px;line-height: 0;margin-left: 7px;margin-top: 3px;" value="Warn" class="btn btn-danger btn-xs"><i class="fa-warning " style="font:normal normal normal 10px/1 FontAwesome;"></i></button>';
                        else
                            return '';
                    });
                dcolumn.sortable = true;
            }

            var dcolumn = igrid.AddColumn(
                {
                    id: "settlementamount", name: "Amount", field: "settlementamount",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            dcolumn.cssClass = 'rightalign';
            if ($scope.SupplierPayment.ageranges && $scope.SupplierPayment.ageranges != null &&
                $scope.SupplierPayment.ageranges.length > 3) {
                var days = [];
                $.each($scope.LoadPendinginvoice, function (k, v) {
                    if (days.indexOf(v.days) < 0)
                        days.push(v.days);
                });

                $.each(days, function (k, v) {

                    var column = igrid.AddCustomColumn('Grp-Qty-' + k, v, 60, function (item, col) {
                        if ((item.isinvoice || item.isjobwork || item.iscreditnote) && item.days == col.internal)
                            return $filter('IndianNumber')(item.balanceamount, 2);
                        else if (item.isdebitnote && item.days == col.internal)
                            return $filter('IndianNumber')(item.balanceamount * -1, 2);
                        return '';
                    });
                    column.internal = v;
                    column.total = true;
                    column.cssClass = 'rightalign';
                    column.getValue = function (item, col) {
                        if ((item.isinvoice || item.isjobwork || item.iscreditnote) && item.days == col.internal)
                            return item.balanceamount;
                        if ((item.iscreditnote) && item.days == col.internal)
                            return item.balanceamount;
                        else if (item.isdebitnote && item.days == col.internal)
                            return item.balanceamount * -1;
                        return 0;
                    }

                })
            }

            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();

            if ($scope.SupplierPayment.mode != 'DETAIL') {
                if ($scope.HideSupplier)
                    igrid.grid.setOptions({ 'frozenColumn': 4 });
                else
                    igrid.grid.setOptions({ 'frozenColumn': 5 });
            }

            igrid.grid.onHeaderClick.subscribe(function (e, args) {

                if (args.column.id == 'fullvalue' && $(e.target).is(":checkbox")) {
                    if (igrid.grid.getEditorLock().isActive() && !igrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }

                    if ($(e.target).is(":checked")) {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.fullvalue = true;
                            item.settlementamount = item.fullvalue ? (item.totalamount - item.tdsamount - item.paidamount - item.agentcommissionvalue) : 0;
                        }
                    } else {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.fullvalue = false;
                            item.settlementamount = 0;
                        }
                    }
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                    $timeout(function () {
                        $scope.AddedAmountChanged();
                        args.grid.invalidate();
                        args.grid.render();
                    }, 10);
                }

            });

            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                if (item.iscreditnote)
                    return { cssClasses: 'igridcreditnoterow' };
                if (!item.isinvoice && !item.isjobwork) {
                    return { cssClasses: 'igriddebitnoterow' };
                }
            };

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'fullvalue') {
                    item = args.grid.getDataItem(args.row);
                    item.fullvalue = !item.fullvalue && $(e.target).val() == 'on';
                    item.settlementamount = item.fullvalue ? (item.totalamount - item.tdsamount - item.paidamount - item.agentcommissionvalue) : 0;
                    var agentid = 0;
                    if (item.fullvalue && item.agentid && item.agentid > 0) {
                        agentid = item.agentid;
                    }
                    $timeout(function () {
                        if ($scope.SupplierPayment.id == undefined ||
                            $scope.SupplierPayment.id == null ||
                            $scope.SupplierPayment.id == 0) {
                            $scope.SupplierPayment.agentid = agentid;
                        }
                        $scope.AddedAmountChanged();
                        args.grid.invalidate();
                        args.grid.render();
                    }, 10);
                }
                if (args.grid.getColumns()[args.cell].id == 'warning') {
                    item = args.grid.getDataItem(args.row);
                    if (item.warn && item.warn != null && item != '') {
                        $rootScope.Toaster.Error(item.warn);
                    }
                }
                if (args.grid.getColumns()[args.cell].id == '3waymatch') {
                    item = args.grid.getDataItem(args.row);
                    $scope.Show3WayMatch(item);
                }
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'settlementamount') {
                    if (item.settlementamount > (item.totalamount - item.tdsamount - item.paidamount - item.agentcommissionvalue)) {
                        item.settlementamount = (item.totalamount - item.tdsamount - item.paidamount - item.agentcommissionvalue);
                        $timeout(function () {
                            $scope.AddedAmountChanged();
                            args.grid.invalidate();
                            args.grid.render();
                        }, 10);
                    }
                    else {
                        $timeout(function () {
                            $scope.AddedAmountChanged();
                        }, 10);
                    }
                }
            });

            $scope.invgrid = igrid;
            $scope.invgrid.UpdateData($scope.LoadPendinginvoice);
        }

        $scope.Show3WayMatch = function (item) {
            var srequest = JSON.stringify({ 'Item': item });
            $http({
                method: 'POST',
                url: "SupplierpaymentService/SearchPendingPaymentDetail",
                data: srequest
            }).success(function (result) {

            });
        }

        $scope.AgentPaymentSummaryConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '370px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);
            igrid.AddSimpleColumn('agentname', 'Agent', 200);
            igrid.AddSimpleColumn('suppliername', 'Supplier', 200);
            igrid.AddFormatedColumn('agentcommissionvalue', 'Commission', 80, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }


        $scope.AgentPaymentConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '370px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);
            igrid.AddSimpleColumn('agentname', 'Agent', 200);
            igrid.AddSimpleColumn('suppliername', 'Supplier', 200);
            igrid.AddSimpleColumn('receiptno', 'Receipt No', 200);
            igrid.AddSimpleColumn('invoiceno', 'Invoice Nos', 200);
            igrid.AddFormatedColumn('paymentdate', 'Payment Date', 100, 'dd/MM/yyyy');
            igrid.AddFormatedColumn('paidamount', 'Invoice Paid', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discountamount', 'Invoice Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('agentcommissionvalue', 'Commission', 80, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.SupplierPaymentConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '370px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 220, $scope.Suppliers);

            if ($scope.SearchResult && $scope.SearchResult != null) {
                var hasagent = $scope.SearchResult.find(function (item) {
                    return item.agentname && item.agentname != null && item.agentname != '';
                });
                if (hasagent && hasagent != null) {
                    igrid.AddSimpleColumn('agentname', 'Agent', 100);
                }
            }
            igrid.AddFormatedColumn('paymentdate', 'Payment Date', 100, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('receiptno', 'Receipt No', 100);
            igrid.AddReferenceColumn('paymentmode', 'Payment Mode', 140, $scope.PaymentModes);
            igrid.AddCustomColumn('isinterstatesale', 'IGST', 40, function (item, col) {
                return item.isinterstatesale ? 'Yes' : 'No';
            });

            if ($scope.SearchCondition.includecity) {
                igrid.AddReferenceColumn('cityid', 'City', 140, $scope.City);
            }

            igrid.AddCustomColumn('interbank', 'B.T', 40, function (item, col) {
                return item.interbank ? 'I' : (item.paidamount>=200000 ? 'R':'N');
            });
            if ($scope.SearchCondition.includebankinfo) {
                igrid.AddSimpleColumn('suppliermailid', 'Supplier MailID', 80);

                igrid.AddCustomColumn('bankname', 'Bank Name', 100, function (item, col) {
                    return item.paymentinfo != null ? item.paymentinfo.bankname : '';
                });
                igrid.AddCustomColumn('bankbranch', 'Branch', 100, function (item, col) {
                    return item.paymentinfo != null ? item.paymentinfo.bankbranch : '';
                });
                igrid.AddCustomColumn('bankaccountname', 'Account Name', 100, function (item, col) {
                    return item.paymentinfo != null ? item.paymentinfo.bankacname : '';
                });
                igrid.AddCustomColumn('bankaccountno', 'Account No', 100, function (item, col) {
                    return item.paymentinfo != null ? item.paymentinfo.bankacno : '';
                });
                igrid.AddCustomColumn('bankifci', 'IFSC', 100, function (item, col) {
                    return item.paymentinfo != null ? item.paymentinfo.bankifsccode : '';
                });
            }

            igrid.AddSimpleColumn('referencedetail', 'Cheque/Ref Nos', 120);
            igrid.AddSimpleColumn('invoicenos', 'Invoice Nos', 120);
            igrid.AddFormatedColumn('invoicetotal', 'Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('debitnotetotal', 'CN/DN Amount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totaldiscount', 'Discount', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('tdsamount', 'TDS', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paidamount', 'Paid Amount', 100, 'N2', 2).total = true;
            igrid.AddSimpleColumn('createduser', 'Created By', 80);
            igrid.AddFormatedColumn('createdon', 'Created On', 100, 'dd-MM-yyyy hh:mm');
            igrid.AddSimpleColumn('modifieduser', 'Modified By', 80);
            igrid.AddFormatedColumn('modifiedon', 'Modified On', 100, 'dd-MM-yyyy hh:mm');

            var commands = "";
            if ($rootScope.UserInfo.p_SUPPLIERPAYMENT.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_SUPPLIERPAYMENT.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            if ($rootScope.UserInfo.p_SUPPLIERPAYMENT.print)
                commands += "<input type='button' class='btn btn-success btn-xs' value='Print'/> &nbsp;"
            if ($rootScope.UserInfo.p_SUPPLIERPAYMENT.print)
                commands += "<input type='button' class='btn btn-secondary btn-xs' value='PDF'/> &nbsp;"

            igrid.AddButtonColumn('action', 'Action', 200, commands, function (e, args) {

                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManagePaymentItem('Edit', args.item);
                    if ($(e.target).val() == 'Delete')
                        $scope.DeletePayment(args.item);
                    else if ($(e.target).val() == 'Print')
                        $scope.PrintCheuqeAdvice(args.item.id);
                    else if ($(e.target).val() == 'PDF')
                        $scope.DownloadCheuqeAdvice(args.item.id);
                });
                return true;

            });
            $scope.selectedcount = 0;
            igrid.EnableTotal();
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.SupplierPaymentSummaryConfig = function () {

            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('gridContainer', '370px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 220, $scope.Suppliers);
            igrid.AddCustomColumn('bankbranch', 'Branch', 100, function (item, col) {
                return item.paymentinfo != null ? item.paymentinfo.bankbranch : '';
            });
            igrid.AddCustomColumn('bankaccountno', 'Account No', 100, function (item, col) {
                return item.paymentinfo != null ? item.paymentinfo.bankacno : '';
            });
            igrid.AddCustomColumn('bankifci', 'IFSC', 100, function (item, col) {
                return item.paymentinfo != null ? item.paymentinfo.bankifsccode : '';
            });

            igrid.AddFormatedColumn('paidamount', 'Paid Amount', 100, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.DeletePayment = function (item) {
            $.confirm({
                title: 'Delete?',
                content: 'Do you want to delete the supplier payment?',
                confirmButton: 'Yes',
                confirmButtonClass: 'btn-info',
                icon: 'fa fa-question-circle',
                animation: 'scale',
                animationClose: 'top',
                opacity: 0.5,
                buttons: {
                    formSubmit: {
                        text: 'Submit',
                        btnClass: 'btn-blue',
                        action: function () {


                            var request = {
                                "Item": item,
                                "securitytoken": "String content"
                            }

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "SupplierpaymentService/Delete",
                                data: srequest
                            }).success(function (result) {

                                if (result) {
                                    $rootScope.Toaster.Info("Successfully deleted");
                                    $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                    $scope.igrid.UpdateData($scope.SearchResult);
                                }

                            }).error(function (result) {
                            });
                        }
                    },
                    cancel: function () {
                        //close
                    },
                },

                onContentReady: function () {
                    // bind to events
                    var jc = this;
                    jc.$$formSubmit.trigger('focus');
                    this.$content.find('form').on('submit', function (e) {
                        // if the user submits the form by pressing enter in the field.
                        e.preventDefault();
                        jc.$$formSubmit.trigger('click'); // reference the button and click it
                    });
                }
            });
        }
        $scope.PublishAdviceConfirmation = function (selected) {
            $.confirm({
                title: 'Advice Slip',
                content: 'Do you want to send Payment Advice slip to the supplier?',
                confirmButton: 'Yes',
                confirmButtonClass: 'btn-info',
                icon: 'fa fa-question-circle',
                animation: 'scale',
                animationClose: 'top',
                opacity: 0.5,
                buttons: {
                    formSubmit: {
                        text: 'Submit',
                        btnClass: 'btn-blue',
                        action: function () {
                            $scope.SendAdviceSlip(selected);
                        }
                    },
                    cancel: function () {
                        //close
                    },
                },

                onContentReady: function () {
                    // bind to events
                    var jc = this;
                    jc.$$formSubmit.trigger('focus');
                    this.$content.find('form').on('submit', function (e) {
                        // if the user submits the form by pressing enter in the field.
                        e.preventDefault();
                        jc.$$formSubmit.trigger('click'); // reference the button and click it
                    });
                }
            });
        };
        $scope.SendAdviceSlip = function (selected) {
            if (selected) {
                var ids = [];
                $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                    ids.push(v.id);
                    if (ids.length == 50) {
                        restService.Get('SupplierpaymentService/Publish/' + $scope.UserInfo.OrganizationID + '/' + ids.join('_'), true).then(function (results) {
                            $rootScope.Toaster.Info("Payment Advice Slip Sent Successfully.")
                        });
                        ids = [];
                    }
                })
                if (ids.length > 0) {
                    restService.Get('SupplierpaymentService/Publish/' + $scope.UserInfo.OrganizationID + '/' + ids.join('_'), true).then(function (results) {
                        $rootScope.Toaster.Info("Payment Advice Slip Sent Successfully.")
                    });
                }
            }
            else {
                restService.Get('SupplierpaymentService/Publish/' + $scope.UserInfo.OrganizationID + '/' + $scope.SupplierPayment.id, true).then(function (results) {
                    $rootScope.Toaster.Info("Payment Advice Slip Sent Successfully.")
                });
            }
        };

        $scope.PrintCheuqeAdvice = function (id) {
            $rootScope.PrintPDF('SupplierpaymentService/PrintCheuqeAdvicePDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.SupplierPayment.id : id),
                'paymentadvice.pdf', 'application/pdf');
        }

        $scope.DownloadCheuqeAdvice = function (id) {
            $rootScope.Download('SupplierpaymentService/PrintCheuqeAdvicePDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.SupplierPayment.id : id),
                'paymentadvice.pdf', 'application/pdf');
            //    $timeout(function () {
            //        var link = document.createElement('a');
            //        link.download = "paymentadvice.pdf";
            //        link.href = 'SupplierpaymentService/PrintCheuqeAdvicePDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.SupplierPayment.id : id);
            //        document.body.appendChild(link);
            //        link.click();
            //        $timeout(function () {
            //            link.remove();
            //        }, 100);
            //    }, 10);
        }

        $scope.SaveAll = function () {
            var dialog = ngDialog.open({
                template: 'SaveAllDialog',
                data: [$scope, $scope.SupplierPayment],
                width: '750px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SupplierPayment = {};
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.Save = function () {
                        $scope.SupplierPayment.Supplierpaymentinvoice = [];
                        var bank = $scope.Parent.Banks.find(function (item) {
                            return item.id == $scope.SupplierPayment.bankid;
                        });
                        $scope.SupplierPayment.bankname = bank && bank != null ? bank.name : "";
                        $.each($scope.Parent.LoadPendinginvoice, function (k, v) {
                            if (v.settlementamount != 0) {
                                $scope.SupplierPayment.Supplierpaymentinvoice.push({
                                    'supplierid': -1,
                                    'invoiceid': v.lrinvoiceid,
                                    'companyid': v.companyid,
                                    'invoiceno': v.invoiceno,
                                    'invoicedate': v.invoicedate,
                                    'paidamount': v.isinvoice || v.isjobwork || item.iscreditnote ? v.settlementamount : v.settlementamount * -1,
                                    'discountamount': 0,
                                    'isinvoice': v.isinvoice,
                                    'isjobwork': v.isjobwork,
                                    'isdebitnote': v.isdebitnote,
                                    'iscreditnote': v.iscreditnote,
                                    'islrinvoicereturn': v.isdebitnote,
                                    'supplier': v.supplier,
                                    'supplierid': v.supplierid,
                                    'agentcommission': v.agentcommission,
                                    'agentid': v.agentid,
                                    'agentcommissionvalue': v.agentcommissionvalue,
                                    'lrentryno': v.lrentryno,
                                    'slipno': v.slipno,
                                    'entrydate': v.entrydate,
                                });

                            }
                        });
                        if ($scope.Parent.SupplierPayment.bankcheques != 'undefined'
                            && $scope.Parent.SupplierPayment.bankcheques != null) {
                            $scope.SupplierPayment.bankcheques = [];
                            $.each($scope.Parent.SupplierPayment.bankcheques, function (k, v) {
                                $scope.SupplierPayment.bankcheques.push({
                                    bankid: v.accountname,
                                    chequedate: v.chequedate,
                                    chequeno: v.chequeno,
                                    amount: v.amount,
                                    sourcetype: 'SPMT',
                                    companyid: v.companyid,
                                    accountname: v.accountname
                                });
                            });
                        }
                        $scope.closeThisDialog($scope.SupplierPayment);
                    }
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
                            url: "SupplierpaymentService/SaveAll",
                            data: srequest
                        }).success(function (result) {

                        }).error(function (result) {
                        });
                    }
                }
            });
        }


        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName
            if ($scope.SearchCondition.receiptno != 'undefined' && $scope.SearchCondition.receiptno != null &&
                $scope.SearchCondition.receiptno != '')
                title += " Receipt No : " + $scope.SearchCondition.receiptno;

            var subtitle = 'SUPPLIER PAYMENT - INFORMATION'
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
            $scope.New();
            $scope.setFocusfirst();
        }, 10);

    }]);

