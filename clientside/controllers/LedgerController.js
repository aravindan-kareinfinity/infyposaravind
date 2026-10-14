
var app = angular.module('InfinityLab');

app.controller('ledgerController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.BankList = restService.Get('ReferencelistService/BANK', true).then(function (results) {
            $scope.BankList = results;
        });
        $scope.OBModes = [
            { 'id': 'D', 'name': 'Dr' },
            { 'id': 'C', 'name': 'Cr' }];
        $scope.InventoryTCS = [];
        restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.PurchaseTaxList = results;
            $.each(results, function (k, v) {
                if (v.type == 5) {
                    $scope.InventoryTCS.push((JSON.parse(JSON.stringify(v))));
                }
            })
        });
        restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.SalesTaxList = results;
            $.each(results, function (k, v) {
                if (v.type == 5) {
                    $scope.InventoryTCS.push((JSON.parse(JSON.stringify(v))));
                }
            })
        });
        $scope.Entity = {
            "id": 0,
            "displayname": "",
            "contactperson": "",
            "contactnumber": "",
            "contactaddress": "",
            "parentinfo": null,
            "emailid": "",
            "typeid": 0,
            "refid": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "attributes": {},
            "createdon": new Date(),
            "modifiedon": new Date()
        }
        $scope.Ledger = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.TagValueList = [];
        $scope.LocationList = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.LocationList = results;
            $.each(results, function (k, v) {
                if (!(v.additionalinfo && v.additionalinfo.iswarehouse)) {
                    $scope.TagValueList.push({ 'id': v.id, 'value': '' });
                }
            })
            if ($scope.Ledger.attributes.tagvalues == undefined || $scope.Ledger.attributes.tagvalues == null)
                $scope.Ledger.attributes.tagvalues = (JSON.parse(JSON.stringify($scope.TagValueList)));
            results.push({ 'id': -999, 'name': 'ALL Location' });
        });
        $scope.StateList = restService.Get('ReferencelistService/STATE', true).then(function (results) {
            $scope.StateList = results;
        });
        $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.CityList = results;
        });
        $scope.AssetTypes = restService.Get('ReferencelistService/FULL-ASSETTYPE', true).then(function (results) {
            $scope.AssetTypes = results;
        });
        $scope.IGSTTaxLedgers = [];
        $scope.CGSTTaxLedgers = [];
        $scope.SGSTTaxLedgers = [];

        $scope.Customers = restService.Get('ReferencelistService/CUSTOMERLEDGER', true).then(function (results) {
            $scope.Customers = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIERLEDGER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transports = results;
        });
        $scope.TDSGroups = restService.Get('ReferencelistService/TDSGROUP', true).then(function (results) {
            $scope.TDSGroups = results;
        });

        $scope.TagList = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Inventory Sales' },
            { 'id': 2, 'name': 'Inventory Purchase' },
            { 'id': 3, 'name': 'Inventory GST' },
            { 'id': 4, 'name': 'Inventory TDS' },
            { 'id': 5, 'name': 'Inventory TCS' },
            { 'id': 6, 'name': 'Inventory Other Taxes' },
            { 'id': 7, 'name': 'Inventory Interest' },
            { 'id': 8, 'name': 'Inventory Stock' },
            { 'id': 10, 'name': 'Loyelty/Rewards' },
            { 'id': 11, 'name': 'Gift Coupon' },

            { 'id': 12, 'name': 'CASH Sales' },
            { 'id': 13, 'name': 'CARD Sales' },
            { 'id': 14, 'name': 'UPI Sales' },
            { 'id': 15, 'name': 'CHQ Sales' },
            { 'id': 16, 'name': 'CREDIT Sales' },
            { 'id': 17, 'name': 'VOUCHER Sales' },
            { 'id': 18, 'name': 'CUSTOMER Ledger' },
            { 'id': 19, 'name': 'JOBWORK Ledger' },

            { 'id': 20, 'name': 'P&L Expense' },
            { 'id': 21, 'name': 'Daily Expense' },
            { 'id': 22, 'name': 'Weekly Expense' },
            { 'id': 23, 'name': 'Monthly Expense' },
            { 'id': 24, 'name': 'Yearly Expense' },
            { 'id': 25, 'name': 'Asset Depreciation' },
            { 'id': 26, 'name': 'Salary Expense' },
            { 'id': 27, 'name': 'Sales Margin % Expense' },
            { 'id': 28, 'name': 'Purchase Margin % Expense' },
        ];

        $scope.TaxModes = [{ 'id': 0, 'name': 'Zero Tax' },
        { 'id': 1, 'name': 'DUTY' },
        { 'id': 2, 'name': 'CESS' },
        { 'id': 3, 'name': 'IGST' },
        { 'id': 4, 'name': 'SGST' },
        { 'id': 5, 'name': 'CGST' },
        { 'id': 6, 'name': 'TDS' },
        { 'id': 7, 'name': 'TCS' },
        { 'id': 8, 'name': 'SURCHARGE' }];

        $scope.Modes = [
            { 'id': 1, 'name': 'Normal' },
            { 'id': 2, 'name': 'Detail' },
            { 'id': 3, 'name': 'Export' }];

        $scope.TDSModes = [
            { 'id': 1, 'name': 'Purchase' },
            { 'id': 2, 'name': 'Payment' }];

        $scope.TDSEligibleModes = [
            { 'id': 0, 'name': 'All' },
            { 'id': 1, 'name': 'Single Transaction' },
            { 'id': 2, 'name': 'Total Transcation' }];

        $scope.FilterTypes = [{ 'id': 1, 'name': 'Inventory Supplier' },
        { 'id': 2, 'name': 'Inventory Customer' },
        { 'id': 18, 'name': 'Inventory Bank' },
        { 'id': 3, 'name': 'GST Available' },
        { 'id': 4, 'name': 'PAN Available' },
        { 'id': 5, 'name': 'Managable Assets' },
        { 'id': 6, 'name': 'GST Missing' },
        { 'id': 7, 'name': 'PAN Missing' },
        { 'id': 8, 'name': 'Billwise Enabled' },
        { 'id': 9, 'name': 'Billwise Disabled' },
        { 'id': 10, 'name': 'TDS Ledgers' },
        { 'id': 11, 'name': 'GST Enabled' },
        { 'id': 12, 'name': 'GST Ledgers' },
        { 'id': 13, 'name': 'IGST Ledgers' },
        { 'id': 14, 'name': 'SGST Ledgers' },
        { 'id': 15, 'name': 'CGST Ledgers' },
        { 'id': 16, 'name': 'TCS Ledgers' },
        { 'id': 17, 'name': 'CESS Ledgers' },
        { 'id': 18, 'name': 'Inventory Bank' },
        { 'id': 20, 'name': 'P&L Expense' },
        { 'id': 21, 'name': 'Daily Expense' },
        { 'id': 22, 'name': 'Weekly Expense' },
        { 'id': 23, 'name': 'Monthly Expense' },
        { 'id': 24, 'name': 'Yearly Expense' },
        { 'id': 25, 'name': 'Asset Depreciation' },
        { 'id': 26, 'name': 'Salary Expense' },
        { 'id': 27, 'name': 'Sales Margin % Expense' },
        { 'id': 28, 'name': 'Purchase Margin % Expense' },
        { 'id': 37, 'name': 'Inventory Interest' },
        { 'id': 38, 'name': 'Inventory Stock' },
        { 'id': 39, 'name': 'Cash Related' },
        ];

        $scope.ItemTypes = [{ 'id': 1, 'name': 'Capital' },
        { 'id': 2, 'name': 'Fixed asset' },
        { 'id': 19, 'name': 'Current asset' },
        { 'id': 3, 'name': 'Liablities' },
        { 'id': 20, 'name': 'Current Liablities' },
        { 'id': 4, 'name': 'Direct Income' },
        { 'id': 5, 'name': 'Direct Expense' },
        { 'id': 6, 'name': 'Cash-In-Hand' },
        { 'id': 7, 'name': 'Bank Account' },
        { 'id': 8, 'name': 'Stock-In-Hand' },
        { 'id': 9, 'name': 'Indirect Incomes' },
        { 'id': 10, 'name': 'Indirect Expenses' },
        { 'id': 11, 'name': 'Sundry Debtors' },
        { 'id': 12, 'name': 'Sundry Creditors' },
        { 'id': 13, 'name': 'Petty Cash' },
        { 'id': 14, 'name': 'Sales' },
        { 'id': 15, 'name': 'Purchase' },
        { 'id': 16, 'name': 'Tax & Duties' },
        { 'id': 17, 'name': 'Petty Cash Direct Expense' },
        { 'id': 18, 'name': 'Petty Cash In-Direct Expense' },
        { 'id': 21, 'name': 'Petty Cash Direct Income' },
        { 'id': 22, 'name': 'Petty Cash In-Direct Income' },
        { 'id': 30, 'name': 'Rounding' }];


        $scope.SearchResult = [];
        $scope.IsfromVoucher = typeof ($scope.ngDialogData) != 'undefined' && $scope.ngDialogData.length > 0;
        $scope.ShowNew = ($scope.IsfromVoucher);
        $scope.PageStyle = $scope.IsfromVoucher ? "col-md-12" : "col-md-6";
        $scope.SwitchPanel = function () {
            $scope.ShowNew = !$scope.ShowNew;
        }

        $scope.Save = function () {
            var request = {
                "item": $scope.Ledger,
                "securitytoken": "String content"
            }

            if ($scope.Ledger.attributes == undefined || $scope.Ledger.attributes == null)
                $scope.Ledger.attributes = {};

            if ($scope.Ledger.typeid != 11 && $scope.Ledger.typeid != 12) {
                $scope.Ledger.billwise = false;
            }

            if (($scope.Ledger.typeid == 6 ||
                $scope.Ledger.typeid == 14 ||
                $scope.Ledger.typeid == 15) &&
                !$scope.Ledger.attributes.hasgst) {
                $scope.Ledger.attributes.branchids = [];
                $.each($scope.LocationList, function (k, v) {
                    if (v.selected)
                        $scope.Ledger.attributes.branchids.push(v.id);
                });
            }

            if ($scope.Ledger.typeid == 16 && ($scope.Ledger.taxmode == 3 || $scope.Ledger.taxmode == 4 || $scope.Ledger.taxmode == 5)) {

                if ($scope.Ledger.taxpercentage == undefined ||
                    $scope.Ledger.taxpercentage == null ||
                    $scope.Ledger.taxpercentage == 0) {
                    $rootScope.Toaster.Info("Please enter the tax percentage");
                    return;
                }

                $scope.Ledger.attributes.invptaxids = [];
                $scope.Ledger.attributes.invstaxids = [];
                $.each($scope.PurchaseTaxList, function (k, v) {
                    if (v.selected)
                        $scope.Ledger.attributes.invptaxids.push(v.id);
                });
                $.each($scope.SalesTaxList, function (k, v) {
                    if (v.selected)
                        $scope.Ledger.attributes.invstaxids.push(v.id);
                });
            }

            if ($scope.Ledger.typeid == 16 && ($scope.Ledger.taxmode == 7)) {
                $scope.Ledger.attributes.invptaxids = [];
                $.each($scope.InventoryTCS, function (k, v) {
                    if (v.selected)
                        $scope.Ledger.attributes.invptaxids.push(v.id);
                });
            }

            if ($scope.bintreedata != null && $scope.bintreedata.length > 0) {
                if ($scope.bintreedata[0].modified) {
                    request.item.parenthierarchy = $scope.bintreedata[0];
                }
                //    if ($scope.CurrentNode != null) {
                //        if (!$scope.CurrentNode.selected) {

                //        }
                //    }
            }

            if ($scope.Ledger.typeid == 11 && $scope.Ledger.attributes.iscustomer) {
                var customer = $scope.Customers.find(function (item) {
                    return item.id == $scope.Ledger.customerid;
                });
                if (customer && customer != null) {
                    if ($scope.Ledger.displayname != customer.name) {
                        $rootScope.Toaster.Info("Inventory customer name and ledger name should be same, name updated, save again");
                        $scope.Ledger.displayname = customer.name;
                    }
                } else {
                    $rootScope.Toaster.Info("Please select the inventory customer");
                    return;
                }
            }

            if ($scope.Ledger.typeid == 12 && $scope.Ledger.attributes.issupplier) {
                var supplier = $scope.Suppliers.find(function (item) {
                    return item.id == $scope.Ledger.supplierid;
                });
                if (supplier && supplier != null) {
                    if ($scope.Ledger.displayname != supplier.name) {
                        $rootScope.Toaster.Info("Inventory supplier name and ledger name should be same, name updated, save again");
                        $scope.Ledger.displayname = supplier.name;
                    }
                }
                else {
                    $rootScope.Toaster.Info("Please select the inventory supplier");
                    return;
                }
            }



            $scope.Ledger.opening = 0;
            if ($scope.Ledger.obvalue && $scope.Ledger.obvalue != null) {
                var obvalue = parseFloat($scope.Ledger.obvalue);
                if (isNaN(obvalue))
                    obvalue = 0;
                $scope.Ledger.opening = obvalue;
            }
            if ($scope.Ledger.opening != 0) {
                if ($scope.Ledger.obmode == undefined || $scope.Ledger.obmode == null) {
                    $rootScope.Toaster.Info("Please select opening balance is either credit or debit");
                    return;
                }
                if ($scope.Ledger.obmode == 'C') {
                    $scope.Ledger.opening = Math.abs($scope.Ledger.opening) * -1;
                }
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LedgerService/Save",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");

                    $scope.ProcessLedger4Tax(result);

                    if ($scope.IsfromVoucher) {
                        $scope.closeThisDialog($scope.Ledger);
                    }
                    else {
                        $scope.Ledger = (JSON.parse(JSON.stringify($scope.Entity)));
                        $scope.Ledger.attributes.tagvalues = (JSON.parse(JSON.stringify($scope.TagValueList)));
                    }
                    //$scope.Search();
                }
                else {
                    $rootScope.Toaster.Info(result.ErrorMessage);
                }

            }).error(function (result) {
            });

        }

        $scope.CostAllocationChanged = function () {
            var val = 0;
            $.each($scope.Ledger.attributes.tagvalues, function (k, v) {
                var v = parseFloat(v.value);
                if (!isNaN(v)) {
                    val += v;
                }
                else {
                    v.value = '';
                }
            })
            $scope.Ledger.attributes.tagvalue = val;
        }

        $scope.AssetTypeChanged = function () {
            var assettype = $scope.AssetTypes.find(function (item) {
                return item.id == $scope.Ledger.attributes.assettypeid
            });

            if ($scope.Ledger.attributes == undefined ||
                $scope.Ledger.attributes == null)
                $scope.Ledger.attributes = {};

            if ($scope.Ledger.attributes.asset == undefined ||
                $scope.Ledger.attributes.asset == null)
                $scope.Ledger.attributes.asset = {};

            if (assettype &&
                assettype.additionalinfo &&
                assettype.additionalinfo.assetconfig) {
                $scope.Ledger.attributes.asset.config = assettype.additionalinfo.assetconfig;
            }
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.displayname + "?",
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
                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "LedgerService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.egrid.UpdateData($scope.SearchResult);
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
            else if (action == "Edit") {
                $scope.ShowNew = true;

                request = {
                    "Item": { 'id': item.id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "LedgerService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.Ledger = result[0];
                    $scope.Ledger.obvalue = '';
                    $scope.Ledger.obmode = '';
                    if ($scope.Ledger.opening && $scope.Ledger.opening != 0) {
                        $scope.Ledger.obvalue = Math.abs($scope.Ledger.opening);
                        $scope.Ledger.obmode = $scope.Ledger.opening < 0 ? 'C' : 'D';
                    }

                    item.customer = item.customerid > 0;
                    item.supplier = item.supplierid > 0;
                    if ($scope.Ledger.attributes) {
                        if ($scope.Ledger.attributes.tagvalues == undefined || $scope.Ledger.attributes.tagvalues == null) {
                            $scope.Ledger.attributes.tagvalues = (JSON.parse(JSON.stringify($scope.TagValueList)));
                        }
                        else {
                            $.each($scope.TagValueList, function (k, v) {
                                var exist = $scope.Ledger.attributes.tagvalues.find(function (sitem) {
                                    return sitem.id == v.id;
                                });
                                if (exist == undefined || exist == null) {
                                    $scope.Ledger.attributes.tagvalues.push({ 'id': v.id, 'value': '' });
                                } else if (v.value == 0) {
                                    v.value = '';
                                }
                            })
                        }
                    }

                    if ($scope.Ledger.typeid == 6 &&
                        $scope.Ledger.attributes &&
                        $scope.Ledger.attributes.branchids &&
                        $scope.Ledger.attributes.branchids != null) {
                        $.each($scope.LocationList, function (k, v) {
                            v.selected = $scope.Ledger.attributes.branchids.indexOf(v.id) >= 0;
                        });
                    }

                    if ($scope.Ledger.typeid == 16 && ($scope.Ledger.taxmode == 3 || $scope.Ledger.taxmode == 4 || $scope.Ledger.taxmode == 5)) {
                        if ($scope.Ledger.attributes.invptaxids &&
                            $scope.Ledger.attributes.invptaxids != null) {
                            $.each($scope.PurchaseTaxList, function (k, v) {
                                v.selected = $scope.Ledger.attributes.invptaxids.indexOf(v.id) >= 0;
                            });
                        }
                        if ($scope.Ledger.attributes.invstaxids &&
                            $scope.Ledger.attributes.invstaxids != null) {
                            $.each($scope.SalesTaxList, function (k, v) {
                                v.selected = $scope.Ledger.attributes.invstaxids.indexOf(v.id) >= 0;
                            });
                        }
                    }

                    if ($scope.Ledger.typeid == 16 && ($scope.Ledger.taxmode == 7)) {
                        if ($scope.Ledger.attributes.invttaxids &&
                            $scope.Ledger.attributes.invttaxids != null) {
                            $.each($scope.InventoryTCS, function (k, v) {
                                v.selected = $scope.Ledger.attributes.invttaxids.indexOf(v.id) >= 0;
                            });
                        }
                    }


                    $scope.LoadConfiguration();
                }).error(function (result) {
                });

            }
        }

        $scope.AddNew = function () {
            $scope.Ledger = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.Ledger.attributes.tagvalues = (JSON.parse(JSON.stringify($scope.TagValueList)));
            $scope.ShowNew = true;
            $.each($scope.LocationList, function (k, v) {
                v.selected = false;
            });
            $.each($scope.InventoryTCS, function (k, v) {
                v.selected = false;
            });
            $.each($scope.PurchaseTaxList, function (k, v) {
                v.selected = false;
            });
            $.each($scope.SalesTaxList, function (k, v) {
                v.selected = false;
            });
        }

        $scope.SearchCondition = {
            id: "",
            displayname: "",
            typeid: 0
        }

        $scope.Search = function () {
            $scope.ShowNew = false;
            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LedgerService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $.each(result, function (k, v) {
                    if (v.opening != 0) {
                        if (v.opening < 0) {
                            v.obmode = 'C';
                            v.obvalue = v.opening * -1;
                        }
                        else {
                            v.obmode = 'D';
                            v.obvalue = v.opening;
                        }
                    }
                })
                $scope.ConfigGrid();
            }).error(function (result) {
            });
        }

        $scope.egrid = null;
        $scope.ConfigGrid = function () {
            if ($scope.egrid != null)
                $scope.egrid.destroy();
            var igrid = new InfyGrid('gridResultContainer', '100px', '100%', false, true, true, 40);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('typeid', 'Type', 150, $scope.ItemTypes);
            igrid.AddSimpleColumn('displayname', 'Name', 200);
            var column = igrid.AddCustomColumn('parent', 'Head', 250, function (item, col) {
                return item.parentinfo ? item.parentinfo.detail : $scope.SearchRef(item.typeid, $scope.ItemTypes);
            });

            if ($scope.SearchCondition.modeid == undefined ||
                $scope.SearchCondition.modeid == null)
                $scope.SearchCondition.modeid = 0;

            if ($scope.SearchCondition.modeid == 0) {

                if ($scope.SearchCondition &&
                    $scope.SearchCondition.typeid &&
                    ($scope.SearchCondition.typeid == 12 ||
                        $scope.SearchCondition.typeid == 11)) {
                    igrid.AddSimpleColumn('gstno', 'GST No', 100);

                    var column = igrid.AddCustomColumn('pan', 'PAN', 150, function (item, col) {
                        if (item.attributes && item.attributes.panno && item.attributes.panno != null)
                            return item.attributes.panno;
                        return '';
                    });

                    var column = igrid.AddCustomColumn('tds', 'TDS', 150, function (item, col) {
                        if (item.attributes && item.attributes.hastds)
                            return $scope.SearchRef(item.attributes.ledgertdsid, $scope.TDSTaxLedgers);
                        return '';
                    });

                    igrid.AddSimpleColumn('contactperson', 'Contact', 100);
                    igrid.AddSimpleColumn('contactnumber', 'Mobile', 100);
                    igrid.AddSimpleColumn('emailid', 'Email', 100);
                    igrid.AddSimpleColumn('contactaddress', 'Address', 100);
                }
                else {
                    var column = igrid.AddCustomColumn('additional', 'Additional', 150, function (item, col) {
                        if (item.attributes && item.attributes.assettypeid && item.attributes.assettypeid > 0)
                            return $scope.SearchRef(item.attributes.assettypeid, $scope.AssetTypes);

                        if (item.taxmode > 0)
                            return $scope.SearchRef(item.taxmode, $scope.TaxModes);

                        if (item.attributes && item.attributes.hasgst)
                            return 'IGST :' + $scope.SearchRef(item.attributes.ledgerigstid, $scope.IGSTTaxLedgers) + ',' +
                                'CGST :' + $scope.SearchRef(item.attributes.ledgercgstid, $scope.CGSTTaxLedgers) + ',' +
                                'SGST :' + $scope.SearchRef(item.attributes.ledgersgstid, $scope.SGSTTaxLedgers);

                        if (item.attributes && item.attributes.hastds)
                            return $scope.SearchRef(item.attributes.ledgertdsid, $scope.TDSTaxLedgers);

                        return '';
                    });
                }
            }

            var column = igrid.AddCustomColumn('createdon', 'Created', 250, function (item, col) {
                if (item.cemp && item.cemp != null && item.cemp != '')
                    return $filter('date')(item.createdon, 'dd-MMM-yyyy') + ' @ ' + item.cemp;

                return $filter('date')(item.createdon, 'dd-MMM-yyyy');
            });

            var column = igrid.AddCustomColumn('modifiedon', 'Modified', 250, function (item, col) {
                if (item.memp && item.memp != null && item.memp != '')
                    return $filter('date')(item.modifiedon, 'dd-MMM-yyyy') + ' @ ' + item.memp;

                return $filter('date')(item.modifiedon, 'dd-MMM-yyyy');
            });

            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-info btn-xs'><i class='fa fa-pencil' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.options.showHeaderRow = true;
            igrid.Initialize();
            $scope.egrid = igrid;
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete') {
                    $scope.ManageSearchItem('Delete', item);
                } else if (command == 'Edit') {
                    $timeout(function () {
                        $scope.ManageSearchItem('Edit', item);
                    }, 10);
                }
            });
            $scope.egrid.UpdateData($scope.SearchResult);
        }

        $scope.LoadConfiguration = function () {
            restService.Get('ReferencelistService/LEDGERHIERARCHY/' + $scope.Ledger.typeid, true).then(function (results) {
                var currentNode = null;
                if (results.length == 0) {
                    var name = $scope.SearchRef($scope.Ledger.typeid, $scope.ItemTypes);
                    $scope.bintreedata =
                        [
                            {
                                "name": name, "id": 0,
                                "typeid": $scope.Ledger.typeid,
                                "referencelocationid": $scope.Ledger.typeid,
                                "stylecollapsed": 'location_collapsed',
                                "styleexpanded": 'location_expanded',
                                "stylenormal": 'location_normal',
                                "selected": "selected",
                                "supportchild": true,
                                "supportallocation": false,
                                "modified": false,
                                "level": 0,
                                "nodetype": "location",
                                "children": [],
                                "isactive": true,
                                "version": 1
                            }
                        ];

                    currentNode = $scope.bintreedata[0];
                }
                else {
                    var name = $scope.SearchRef($scope.Ledger.typeid, $scope.ItemTypes);
                    $scope.bintreedata =
                        [
                            {
                                "name": name, "id": results[0].id,
                                "typeid": $scope.Ledger.typeid,
                                "referencelocationid": $scope.Ledger.typeid,
                                "stylecollapsed": 'location_collapsed',
                                "styleexpanded": 'location_expanded',
                                "stylenormal": 'location_normal',
                                "selected": "",
                                "supportchild": true,
                                "supportallocation": false,
                                "modified": false,
                                "level": 0,
                                "isactive": true,
                                "version": results[0].version,
                                "nodetype": "location",
                                "children": results[0].children
                            }
                        ];
                    if ($scope.Ledger.parentinfo == undefined || $scope.Ledger.parentinfo == null)
                        $scope.Ledger.parentinfo = {};
                    currentNode = $scope.SetParentSelection($scope.bintreedata, $scope.Ledger.parentinfo.levels, 0);
                }

                $scope.AssignParent($scope.bintreedata[0], null);
                //if ($scope.CurrentNode != null && $scope.CurrentNode.selected)
                //    $scope.CurrentNode.selected = "";
                $timeout(function () {
                    $scope['bintree'].currentNode = currentNode;
                    $scope['bintree'].currentNode.selected = "selected";
                    $scope.CurrentNode = $scope['bintree'].currentNode;
                    $scope.UpdateParent();
                }, 30);
            });
        }

        $scope.CustomerSelected = function () {
            var customer = $scope.Customers.find(function (item) {
                return item.id == $scope.Ledger.customerid;
            });
            if (customer && customer != null) {
                $scope.Ledger.displayname = customer.name;
                $scope.Ledger.gstno = customer.gstno;
                $scope.Ledger.contactaddress = customer.address;
            }
        }

        $scope.SupplierSelected = function () {
            var supplier = $scope.Suppliers.find(function (item) {
                return item.id == $scope.Ledger.supplierid;
            });
            if (supplier && supplier != null) {
                $scope.Ledger.displayname = supplier.name;
                $scope.Ledger.gstno = supplier.gstno;
                $scope.Ledger.contactaddress = supplier.address;
            }
        }

        $scope.SetParentSelection = function (treedata, levels, level) {
            if (typeof (levels) == 'undefined' || levels == null) return treedata[0];
            var selectednode = null;
            for (var i = 0; i < treedata.length; i++) {
                var v = treedata[i];
                if (v.name == levels[level]) {
                    v.collapsed = false;
                    selectednode = v;
                    if (v.children && v.children != null && v.children.length > 0) {
                        selectednode = $scope.SetParentSelection(v.children, levels, level + 1);
                    }
                }
                else {
                    v.collapsed = true;
                }
            }
            return selectednode;
        }

        $scope.SaveLedgerHierarchy = function () {

            var r = $scope.bintreedata[0];
            var request = {
                "name": r.name, "id": r.id,
                "typeid": r.typeid,
                "stylecollapsed": 'location_collapsed',
                "styleexpanded": 'location_expanded',
                "stylenormal": 'location_normal',
                "selected": r.selected,
                "supportchild": true,
                "supportallocation": false,
                "modified": false,
                "level": 0,
                "isactive": true,
                "version": r.version,
                "nodetype": "location",
                "children": $scope.GetChildrens(r.children, 1)
            };

            var srequest = JSON.stringify({ 'Item': request });

            $http({
                method: 'POST',
                url: "ReferencelistService/SaveLedgerHierarchy",
                data: srequest
            }).success(function (result) {
                r.id = result.id;
            });
        }

        $scope.GetChildrens = function (childrens, level) {
            if (typeof (childrens) == 'undefined' || childrens == null) return null;
            var nchildrens = [];
            $.each(childrens, function (k, v) {
                var data = {
                    "name": v.name,
                    "stylecollapsed": 'location_collapsed',
                    "styleexpanded": 'location_expanded',
                    "stylenormal": 'location_normal',
                    "selected": v.selected,
                    "supportchild": true,
                    "supportallocation": false,
                    "level": level,
                    "nodetype": v.nodetype,
                    "children": $scope.GetChildrens(v.children, level + 1)
                }
                nchildrens.push(data);
            });
            return nchildrens;
        }

        $scope.UpdateParent = function () {
            var currentnode = $scope.CurrentNode;
            var level = 0;
            var levels = [currentnode.name];

            while (currentnode.parentnode && currentnode.parentnode != null) {
                currentnode = currentnode.parentnode;
                level++;
                levels.splice(0, 0, currentnode.name);
            }
            $scope.Ledger.parentinfo = {
                'level': level, 'levels': levels, 'detail': levels.join('/')
            };
        }

        $scope.AssignParent = function (node, parentnode) {
            node.parentnode = parentnode;
            if (typeof (node.children) != 'undefined') {
                $.each(node.children, function (k, v) {
                    $scope.AssignParent(v, node);
                })
            }
        }

        $scope.RemoveParent = function (node) {
            node.parentnode = null;
            if (typeof (node.children) != 'undefined') {
                $.each(node.children, function (k, v) {
                    $scope.RemoveParent(v)
                })
            }
        }

        $scope.CurrentNode = { 'binlist': [], supportallocation: false, selected: "" };
        $scope.Clipboard = {};
        $scope['bintree'] = $scope['bintree'] || {};
        $scope['bintree'].onCurrentNodeChange = function (node) {
            if ($scope.CurrentNode != null && $scope.CurrentNode.selected)
                $scope.CurrentNode.selected = "";
            $scope.CurrentNode = node;
            $scope.CurrentNode.selected = "selected";
            $scope.UpdateParent();
        }

        $scope.ShowStockOpening = function () {
            var dialog = ngDialog.open({
                template: 'StockConfig',
                data: [$scope],
                width: '700px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.SearchRef = restService.SearchReference;
                    $scope.ParentScope = $scope.ngDialogData[0];
                    $scope.Ledgerid = $scope.ParentScope.Ledger.id;
                    $scope.Modes = [
                        { 'id': 'D', 'name': 'Dr' },
                        { 'id': 'C', 'name': 'Cr' }];
                    $scope.OpeningList = [];
                    restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
                        $scope.Company = results;
                    });
                    restService.Get('LedgerstockopeningService/Find/' + $scope.Ledgerid, true).then(function (results) {
                        $scope.OpeningList = results;
                    });
                    $scope.AddItem = function () {
                        $scope.OpeningList.push({
                            'ledgerid': $scope.Ledgerid,
                            'isactive': true,
                            'companyid': $scope.companyid,
                            'ondate': $scope.ondate,
                            'reason': $scope.reason,
                            'value': $scope.value,
                        });
                        $scope.reason = '';
                        $scope.value = '';
                        $scope['setFocusrefdate']();
                    }
                    $scope.DeleteItem = function (item) {
                        $scope.OpeningList.splice($scope.OpeningList.indexOf(item), 1);
                    }
                    $scope.Selected = function (item) {
                        var request = {
                            'Item': {
                                'ledgerid': $scope.Ledgerid,
                                'items': $scope.OpeningList
                            }
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "LedgerstockopeningService/SaveAll",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(0);
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        $scope.Ledger.opening = data.value;
                    }
                }
            });
        }

        $scope.ShowOpening = function () {
            var dialog = ngDialog.open({
                template: 'OpeningConfig',
                data: [$scope],
                width: '700px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.ParentScope = $scope.ngDialogData[0];
                    $scope.Ledgerid = $scope.ParentScope.Ledger.id;
                    $scope.Modes = [
                        { 'id': 'D', 'name': 'Dr' },
                        { 'id': 'C', 'name': 'Cr' }];
                    $scope.OpeningList = [];
                    restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
                        $scope.Company = results;
                    });

                    restService.Get('VoucheritembillwiseService/LedgerOpening/' + $scope.Ledgerid, true).then(function (results) {
                        $scope.OpeningList = results;
                    });

                    $scope.AddItem = function () {
                        $scope.OpeningList.push({
                            'ledgerid': $scope.Ledgerid,
                            'isactive': true,
                            'obcompanyid':$scope.companyid,
                            'referencetype': 'Opening',
                            'referenceno': $scope.refno,
                            'transactionon': $scope.refdate,
                            'referenceinvoiceno': $scope.refno,
                            'referenceinvoicedate': $scope.refdate,
                            'payable': $scope.mode == 'C' ? $scope.amount : 0,
                            'receivable': $scope.mode == 'D' ? $scope.amount : 0
                        });
                        $scope.refno = '';
                        $scope.refdate = '';
                        $scope.amount = '';
                        $scope['setFocusrefno']();
                    }

                    $scope.DeleteItem = function (item) {
                        $scope.OpeningList.splice($scope.OpeningList.indexOf(item), 1);
                    }

                    $scope.EditItem = function (item) {
                        $scope.OpeningList.splice($scope.OpeningList.indexOf(item), 1);
                        $scope.companyid = item.obcompanyid;
                        $scope.refno = item.referenceno;
                        $scope.refdate = item.transactionon;
                        if (item.payable > 0) {
                            $scope.mode = 'C';
                            $scope.amount = item.payable;
                        }
                        else {
                            $scope.mode = 'D';
                            $scope.amount = item.receivable;
                        }
                    }

                    $scope.Selected = function (item) {
                        var request = {
                            'Item': {
                                'ledgerid': $scope.Ledgerid,
                                'companyid': $scope.companyid,
                                'customerid': $scope.ParentScope.Ledger.customerid,
                                'supplierid': $scope.ParentScope.Ledger.supplierid,
                                'openinglist': $scope.OpeningList
                            }
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "VoucheritembillwiseService/SaveOpening",
                            data: srequest
                        }).success(function (result) {
                            var cr = 0;
                            var dr = 0;
                            $.each($scope.OpeningList, function (k, v) {
                                var payable = parseFloat(v.payable);
                                var receivable = parseFloat(v.receivable);
                                cr += isNaN(payable) ? 0 : payable;
                                dr += isNaN(receivable) ? 0 : receivable;
                            });
                            if (dr > cr)
                                $scope.closeThisDialog(dr - cr);
                            else
                                $scope.closeThisDialog(cr - dr);
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        $scope.Ledger.opening = data.value;
                    }
                }
            });
        }

        $scope.OnContextMenuSelected = function (item, menu, style) {
            $timeout(function () {

                if (typeof (item.node) == 'undefined')
                    return;

                var currentnode = item.node;

                if (currentnode.level == 0 && menu != 'Create')
                    return;

                if (menu == 'Delete') {
                    if (typeof (currentnode.parentnode) != 'undefined') {
                        currentnode.parentnode.children.splice(currentnode.parentnode.children.indexOf(currentnode), 1);
                        $scope.CurrentNode = { 'binlist': [], supportallocation: false };
                        $scope.SaveLedgerHierarchy();
                    }
                    return;
                }

                var node = currentnode;
                if (menu != 'Edit')
                    node = {
                        'nodetype': menu, name: '', code: '',
                        'level': node.level + 1, parentnode: node,
                        "stylecollapsed": style + '_collapsed',
                        "styleexpanded": style + '_expanded',
                        "stylenormal": style + '_normal',
                        "selected": "selected",
                        "label": '',
                        "children": []
                    };

                var dialog = ngDialog.open({
                    template: 'GroupConfig',
                    data: [$scope, item, menu, node],
                    width: '600px',
                    controller: ['$scope', '$timeout', function ($scope, $timeout) {
                        $scope.parentScope = $scope.ngDialogData[0];
                        $scope.item = $scope.ngDialogData[1];
                        $scope.menu = $scope.ngDialogData[2];
                        $scope.node = $scope.ngDialogData[3];
                        $scope.Selected = function (item) {
                            $scope.closeThisDialog({ 'node': $scope.node, 'menu': $scope.menu });
                        };
                    }],
                    className: 'ngdialog-theme-plain'
                });
                dialog.closePromise.then(function (data) {
                    {
                        if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                            && data.value != '$document') {
                            if (data.value.menu == 'Edit') return;
                            data.value.node.parentnode.children.push(data.value.node);

                            $scope.SaveLedgerHierarchy();

                            if ($scope.CurrentNode != null && $scope.CurrentNode.selected)
                                $scope.CurrentNode.selected = "";
                            $timeout(function () {
                                $scope['bintree'].currentNode = data.value.node;
                                $scope['bintree'].currentNode.selected = "selected";
                                $scope.CurrentNode = data.value.node;
                                $scope.UpdateParent();
                            }, 30);
                        }
                    }
                });
            }, 10);
        }


        $scope.allmenuOptions = [
            ['Create', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Create', 'room');
            }],
            ['Delete', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Delete', 'delete');
            }],
            ['Edit', function ($itemScope) {
                $scope.OnContextMenuSelected($itemScope, 'Edit', 'edit');
            }]
        ];


        $scope.DeleteList = function () {
            if (typeof ($scope.bintree.currentNode) == 'undefined')
                return;
            var currentnode = $scope.bintree.currentNode;
            var list = this.sgrid.GetSelectedRows();
            if (list.length > 0) {
                $scope.bintreedata[0].DeletedBinList = $scope.bintreedata[0].DeletedBinList || [];
                $.each(list, function (k, v) {
                    if (typeof (v.id) != 'undefined' && v.id > 0)
                        $scope.bintreedata[0].DeletedBinList.push(v.id);
                    currentnode.binlist.splice(currentnode.binlist.indexOf(v), 1);
                });
                $scope.sgrid.UpdateData(currentnode.binlist);
            }
        }

        $scope.RemoveExistingLedger4Tax = function (ledgers, ledger) {
            var exist = ledgers.find(function (item) {
                return item.id == ledger.id;
            })
            if (exist && exist != null) {
                ledgers.splice(ledgers.indexOf(exist), 1);
            }
        }

        $scope.ProcessLedger4Tax = function (v) {
            $scope.RemoveExistingLedger4Tax($scope.IGSTTaxLedgers, v);
            $scope.RemoveExistingLedger4Tax($scope.CGSTTaxLedgers, v);
            $scope.RemoveExistingLedger4Tax($scope.TDSTaxLedgers, v);
            $scope.RemoveExistingLedger4Tax($scope.IGSTTaxLedgers, v);

            if (v.typeid == 16) {
                switch (v.taxmode) {
                    case 3:
                        $scope.IGSTTaxLedgers.push(v);
                        $scope.IGSTTaxLedgers = (JSON.parse(JSON.stringify($scope.IGSTTaxLedgers)));
                        break;
                    case 4:
                        $scope.SGSTTaxLedgers.push(v);
                        $scope.SGSTTaxLedgers = (JSON.parse(JSON.stringify($scope.SGSTTaxLedgers)));
                        break;

                    case 5:
                        $scope.CGSTTaxLedgers.push(v);
                        $scope.CGSTTaxLedgers = (JSON.parse(JSON.stringify($scope.CGSTTaxLedgers)));
                        break;

                    case 6:
                        $scope.TDSTaxLedgers.push(v);
                        $scope.TDSTaxLedgers = (JSON.parse(JSON.stringify($scope.TDSTaxLedgers)));
                        break;
                }
            }
        }

        $scope.RemoveTDSItem = function (item) {
            $scope.Ledger.attributes.tdsrules.splice($scope.Ledger.attributes.tdsrules.indexOf(item), 1);
        }

        $scope.AddTDSItem = function () {
            if ($scope.Ledger.attributes.tdsrules == undefined ||
                $scope.Ledger.attributes.tdsrules == null)
                $scope.Ledger.attributes.tdsrules = [];

            $scope.Ledger.attributes.tdsrules.push({
                'value': $scope.Ledger.attributes.tds.value,
                'eligible': $scope.Ledger.attributes.tds.eligible,
                'mode': $scope.Ledger.attributes.tds.mode,
            });
        }

        $scope.ShowParentGroup = function () {

            var dialog = ngDialog.open({
                template: 'GroupConfig',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {

                    $scope.SearchRef = restService.SearchReference;
                    $scope.parentScope = $scope.ngDialogData[0];
                    $scope.typeid = $scope.parentScope.SearchCondition.typeid;
                    restService.Get('ReferencelistService/LEDGERHIERARCHY/' + $scope.typeid, true).then(function (results) {
                        var currentNode = null;
                        if (results.length == 0) {
                            var name = $scope.SearchRef($scope.typeid, $scope.parentScope.ItemTypes);
                            $scope.bintreedata =
                                [
                                    {
                                        "name": name, "id": 0,
                                        "typeid": $scope.typeid,
                                        "referencelocationid": $scope.typeid,
                                        "stylecollapsed": 'location_collapsed',
                                        "styleexpanded": 'location_expanded',
                                        "stylenormal": 'location_normal',
                                        "selected": "selected",
                                        "supportchild": true,
                                        "supportallocation": false,
                                        "modified": false,
                                        "level": 0,
                                        "nodetype": "location",
                                        "children": [],
                                        "isactive": true,
                                        "version": 1
                                    }
                                ];

                            currentNode = $scope.bintreedata[0];
                        }
                        else {
                            var name = $scope.SearchRef($scope.typeid, $scope.parentScope.ItemTypes);
                            $scope.bintreedata =
                                [
                                    {
                                        "name": name, "id": results[0].id,
                                        "typeid": $scope.typeid,
                                        "referencelocationid": $scope.typeid,
                                        "stylecollapsed": 'location_collapsed',
                                        "styleexpanded": 'location_expanded',
                                        "stylenormal": 'location_normal',
                                        "selected": "",
                                        "supportchild": true,
                                        "supportallocation": false,
                                        "modified": false,
                                        "level": 0,
                                        "isactive": true,
                                        "version": results[0].version,
                                        "nodetype": "location",
                                        "children": results[0].children
                                    }
                                ];

                            $scope.Ledger.parentinfo = {};
                            currentNode = $scope.SetParentSelection($scope.bintreedata, $scope.Ledger.parentinfo.levels, 0);
                        }

                        $scope.AssignParent($scope.bintreedata[0], null);
                        //if ($scope.CurrentNode != null && $scope.CurrentNode.selected)
                        //    $scope.CurrentNode.selected = "";
                        $timeout(function () {
                            $scope['bintree'].currentNode = currentNode;
                            $scope['bintree'].currentNode.selected = "selected";
                            $scope.CurrentNode = $scope['bintree'].currentNode;
                            $scope.UpdateParent();
                        }, 30);
                    });

                    $scope.Selected = function (item) {
                        $scope.closeThisDialog({ 'node': $scope.node, 'menu': $scope.menu });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {

                    }
                }
            });
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''

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

        $scope.panelheight = { 'height': '400px' };
        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 150) + 'px';
            }

            var IGSTTaxLedgers = [];
            var CGSTTaxLedgers = [];
            var SGSTTaxLedgers = [];
            var TDSTaxLedgers = [{ 'id': -1, 'displayname': 'Dynamic' }];
            var SundryCreditorLedgers = [];
            var request = {
                "Item": {},
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LedgerService/Search",
                data: srequest
            }).success(function (result) {

                $.each(result, function (k, v) {
                    if (v.typeid == 16) {
                        switch (v.taxmode) {
                            case 3:
                                IGSTTaxLedgers.push(v);
                                break;
                            case 4:
                                SGSTTaxLedgers.push(v);
                                break;
                            case 5:
                                CGSTTaxLedgers.push(v);
                                break;
                            case 6:
                                TDSTaxLedgers.push(v);
                                break;
                        }
                    }
                    else if (v.typeid == 12) {
                        SundryCreditorLedgers.push({ 'id': v.id, 'name': v.displayname });
                    }
                });
                $scope.TDSTaxLedgers = TDSTaxLedgers;
                $scope.IGSTTaxLedgers = IGSTTaxLedgers;
                $scope.CGSTTaxLedgers = CGSTTaxLedgers;
                $scope.SGSTTaxLedgers = SGSTTaxLedgers;
                $scope.SundryCreditorLedgers = SundryCreditorLedgers;

                if (typeof ($location.search().id) != 'undefined' &&
                    $location.search().id.length > 0) {
                    $scope.ManageSearchItem('Edit', { 'id': $location.search().id });
                }
                else if ($scope.IsfromVoucher) {
                    $scope.ManageSearchItem('Edit', { 'id': $scope.ngDialogData[1] });
                }


            }).error(function (result) {
            });

        }, 100);


    }]);