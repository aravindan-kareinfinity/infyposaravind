
var app = angular.module('InfinityLab');

app.controller('BuyBackController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', 'restService', '$timeout', 'Excel', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, restService, $timeout, Excel, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.CanSave = true;
        Number.prototype.round = function (places) {
            return +(Math.round(this + "e+" + places) + "e-" + places);
        }

        $scope.GetNumber = function (value) {
            value = parseFloat(value);
            if (typeof (value) == 'undefined' || value == null || isNaN(value))
                return 0;
            return value;
        };


        $scope.StatusList = [{ 'id': -1, 'name': 'All' }, { 'id': 0, 'name': 'Received' },
        { 'id': 1, 'name': 'Exchanged' },  { 'id': 2, 'name': 'Credit Note' }, { 'id': 3, 'name': 'Debit Note' },
        { 'id': 4, 'name': 'Return' }, { 'id': 5, 'name': 'Sold' }];

        $scope.Schemes = restService.Get('ReferencelistService/SCHEMES', true).then(function (results) {
            $scope.Schemes = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.Items = restService.Get('ReferencelistService/ITEMS', true).then(function (results) {
            $scope.Items = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.Locations = results;
            results.push({ 'id': 0, 'name': 'WAREHOUSE', 'parentgroup': 0 });
            $scope.BuyBack.locationid = 0;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Permission = UserDetails.Info;
        $scope.SearchRef = restService.SearchReference;
        $scope.returnqty = 0;

        $scope.Entity = {
            "mode": 1,
            "productid": 0,
            "companyid": 0,
            "id": 0,
            "isactive": false,
            "createdby": 0,
            "modifiedby": 0,
            "version": 0,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "itemid": 0,
            "qty": 0.0,
            "piece": 0.0,
            "barcode": null,
            "stockid": 0,
            "lrinvoiceitemsid": 0,
            "taxid": 0,
            "rate": 0.0,
            "tax": 0.0,
            "total": 0,
            "returnno": null,
            "buybackdate": new Date(),
            "lrinvoiceid": 0,
            "lrid": 0,
            "discount": 0.0,
            "amount": 0.0,
            "lrinvoicereturnid": 0,
            "Items": [],
            "Lrinvoicereturngroups": [],
            "transportid": "0",
            "bundles": "0",
            "freightamt": "0.0",
            "freight": 0,
            "weight": "0.0",
            "remarks": ""
        };


        $scope.New = function () {
            $scope.LoadReturnList = [];
            $scope.BuyBack = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.BuyBack.buybackdate = new Date();
        }
        $scope.New();

       

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.Debitnote = false;

        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Debitnote = false;
            $scope.PrintDocument = { 'pages': [] };
            //$scope.PurchaseReturn = (JSON.parse(JSON.stringify($scope.Entity)));
        }


        $scope.SearchRequest = {
            buybackdate: "",
            status: -1
        };

        $scope.SchemeChanged = function () {
            var scheme = $scope.Schemes.find(function (item) {
                return item.id == $scope.BuyBack.schemeid;
            });

            if (typeof (scheme) != 'undefined' && scheme != null) {
                $scope.BuyBack.schemediscount = scheme.schemediscount;
                $scope.BuyBack.productid = scheme.productid;
                $scope.BuyBack.brandid = scheme.brandid;
                $scope.FilterItemList();
            }
        }

        $scope.FilterItemList = function () {
            var productid = typeof ($scope.BuyBack.productid) ? 0 : $scope.BuyBack.productid;
            var brandid = typeof ($scope.BuyBack.brandid) ? 0 : $scope.BuyBack.brandid;

            $scope.ItemList = [];
            $.each($scope.Items, function (k, v) {
                if ((productid == 0 || v.productid == productid) && (brandid == 0 || v.brandid == brandid))
                    $scope.ItemList.push(v);
            });
        }

      

        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = Excel.tableToExcel(tableid, title, 1, 'PURCHASE RETURN SUMMARY REPORT');
            $timeout(function () {
                var link = document.createElement('a');
                link.download = title + ".xls";
                link.href = $scope.exportHref;
                document.body.appendChild(link);
                link.click();
                link.remove();
            }, 100);
        }



        $scope.ErrorMessage = "";



        $scope.LoadReturnList = [];

        $scope.SearchCondition = {
            receivedbyemployeid: "",
            paidbyemployeeid: "",
            paidon: "",
            showSearchResult: false,
            searchtype: 0
        }

        $scope.AddReturnItems = function (items) {


            $.each(items, function (k, v) {


                v.returnqty = v.qty;

                if (!isNaN(v.returnqty)) {

                    var returnqty = v.returnqty;


                    var group = $scope.BuyBack.Lrinvoicereturngroups.find(function (item) {
                        return item.supplierid == v.supplierid;
                    });
                    if (group && group != null) {
                        group.qty += returnqty;
                        if ($scope.BuyBack.Lrinvoicereturngroups.length > 1) {
                            var index = $scope.BuyBack.Lrinvoicereturngroups.indexOf(group);
                            $scope.BuyBack.Lrinvoicereturngroups.splice(index, 1);
                            $scope.BuyBack.Lrinvoicereturngroups.splice(0, 0, group);
                        }
                    }
                    else {
                        var supplier = $scope.Suppliers.find(function (item) {
                            return item.id == v.supplierid;
                        });
                        group = {
                            'supplierid': v.supplierid, 'suppliername': supplier.name, 'qty': returnqty
                        };
                        $scope.BuyBack.Lrinvoicereturngroups.splice(0, 0, group);
                    }


                    var existing = $scope.BuyBack.Items.find(function (item) {
                        return item.barcode == v.barcode;
                    });

                    if (v.barcode != '' && v.barcode != null && existing != null) {
                        existing.qty = parseFloat(existing.qty);
                        if (isNaN(existing.qty))
                            existing.qty = 0;
                        existing.availableqty = v.qty;
                        existing.qty += returnqty;
                        existing.tax = (existing.taxpercentage * existing.qty * (existing.rate - existing.discountperitem) / 100).round(2);
                        existing.discount = (existing.discountperitem * existing.qty).round(2);
                        existing.receivable = (existing.qty * (existing.rate)).round(2);
                    }
                    else {
                        v.version = 1;
                        v.availableqty = v.qty;
                        v.qty = returnqty;
                        v.taxperitem = v.tax;
                        v.discountperitem = (v.rate * v.discountpercentage / 100).round(2);
                        v.tax = (v.taxpercentage * v.qty * (v.rate - v.discount) / 100).round(2);
                        v.discount = (v.discountperitem * v.qty).round(2);
                        v.receivable = (v.qty * (v.rate)).round(2);
                        v.isactive = true;
                        $scope.BuyBack.Items.push(v);
                    }

                }
            });

            if ($scope.SearchInvoiceCondition.mode == 1 && $scope.BuyBack.mode == 1)
                $scope.SearchInvoiceCondition.returnqty = null;
            $scope.CalculateTotal();
            $scope['setFocusbarcode']();
        };

        $scope.CalculateTotal = function (onlytotal) {

            if (typeof (onlytotal) == 'undefined' || !onlytotal) {
                $scope.BuyBack.tax = 0;
                $scope.BuyBack.discount = 0;
                $scope.BuyBack.receivable = 0;
                $scope.BuyBack.total = 0;
                $scope.BuyBack.qty = 0;
                $scope.BuyBack.piece = 0;
                $.each($scope.BuyBack.Items, function (k, v) {
                    $scope.BuyBack.tax = $scope.GetNumber(v.tax) + $scope.BuyBack.tax;
                    $scope.BuyBack.discount = $scope.GetNumber(v.discount) + $scope.BuyBack.discount;
                    $scope.BuyBack.receivable = $scope.GetNumber(v.receivable) + $scope.BuyBack.receivable;
                    $scope.BuyBack.qty = $scope.GetNumber(v.qty) + $scope.BuyBack.qty;
                    $scope.BuyBack.piece += $scope.GetNumber(v.qty);
                });
            }

            $scope.BuyBack.tax = $scope.BuyBack.tax.round(2);
            $scope.BuyBack.discount = $scope.BuyBack.discount.round(2);
            $scope.BuyBack.receivable = $scope.BuyBack.receivable.round(2);
            $scope.BuyBack.total = ($scope.BuyBack.receivable + $scope.BuyBack.tax - $scope.BuyBack.discount).round(0) + $scope.GetNumber($scope.BuyBack.freightamt);
        }

        $scope.ManagePurchaseItem = function (action, item) {
            if (action == 'Delete') {
                var index = $scope.BuyBack.Items.indexOf(item);
                $scope.BuyBack.Items.splice(index, 1);
                $scope.CalculateTotal();

                var ditem = item;

                var group = $scope.BuyBack.Lrinvoicereturngroups.find(function (sitem) {
                    return sitem.supplierid == ditem.supplierid;
                });
                if (group && group != null) {
                    group.qty -= $scope.GetNumber(ditem.qty);
                }
            }
            else if (action == 'Edit') {
                item.newrate = item.rate;
                item.newqty = item.qty;
                item.newdiscountperitem = item.discountperitem;
                item.newdiscountpercentage = item.discountpercentage;
                item.newtax = item.tax;
                item.newtaxpercentage = item.taxpercentage;
                item.newdiscount = item.discount;
                item.newreceivable = item.receivable;
                item.newdesignid = item.designid;
                $scope.taxpercentagechanged(item);
                item.editable = true;
            }
            else if (action == 'Cancel') {
                item.editable = false;
            }
            else {
                //if ($scope.GetNumber(item.newqty) > $scope.GetNumber(item.availableqty)) {
                //    $rootScope.Toaster.Info("Return Qty must be less than or equal to available Qty");
                //    return;
                //}

                var group = $scope.BuyBack.Lrinvoicereturngroups.find(function (sitem) {
                    return sitem.supplierid == item.supplierid;
                });

                if (typeof (group) == 'undefined')
                    group = $scope.BuyBack.Lrinvoicereturngroups[0];

                group.qty -= $scope.GetNumber(item.qty);
                $scope.taxpercentagechanged(item);
                item.tax = item.newtax;
                item.rate = item.newrate;
                item.qty = item.newqty;
                item.receivable = item.newreceivable;
                item.taxpercentage = item.newtaxpercentage;
                item.discount = $scope.GetNumber(item.newdiscount);
                item.designid = item.newdesignid;
                item.discountperitem = item.newdiscountperitem;
                item.discountpercentage = $scope.GetNumber(item.newdiscountpercentage);;
                item.editable = false;
                group.qty += $scope.GetNumber(item.qty);
                $scope.CalculateTotal();
            }
        }



        $scope.Save = function () {

            if (typeof ($scope.BuyBack.customerid) == 'undefined' || $scope.BuyBack.customerid == null) {
                $rootScope.Toaster.Info("Please Select customer.!");
                return;
            }
            
            if (typeof ($scope.BuyBack.locationid) == 'undefined' || $scope.BuyBack.locationid == null) {
                $rootScope.Toaster.Info("Please Select Stock Location.!");
                return;
            }

            if ($scope.BuyBack.buybackdate == null || $scope.BuyBack.buybackdate == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info("Please Select Date.!");
                return;
            }

            var request = {
                "Item": (JSON.parse(JSON.stringify($scope.BuyBack))),
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BuybackService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && typeof (result.id) != 'undefined' && result.id > 0) {
                    $rootScope.Toaster.Info("BuyBack Saved Successfully with No : " + result.buybackno);
                }
                else if (typeof (result.ErrorMessage) != 'undefined') {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
                else {
                    $rootScope.Toaster.Error("Can't Save, check the data and save again");
                }

            }).error(function (result) {
            });

        }


        $scope.SearchRegister = function () {
            $scope.LRInboxList = [];
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BuybackService/Search",
                data: srequest
            }).success(function (result) {
                if ($scope.igrid == null)
                    $scope.ConfigGrid();
                $scope.igrid.UpdateData(result);
            }).error(function (result) {
            });
        }

        $scope.ManageList = function (status, statusid) {

            var popuplist = [];
            $.each(this.igrid.GetSelectedRows(), function (k, v) {

                if (v.status == 0) {
                    popuplist.push(v);
                }
            });

            if (popuplist.length == 0) {
                $rootScope.Toaster.Error('Please select pending items');
                return;
            }
            var dialog = ngDialog.open({
                template: 'Dialog_Replacement',
                data: [$scope, this.igrid.GetSelectedRows(), status, statusid],
                width: '850px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', 'ngDialog', function ($scope, $timeout, $http, $rootScope, ngDialog) {

                    $scope.ShowList = true;
                    $scope.SelectedItem = null;
                    $scope.AllowProductChange = $scope.ngDialogData[2] == 'Alternate';

                    $scope.SelectItem = function () {
                        var item = $scope.ItemNameList.find(function (item) {
                            return item.id == $scope.itemid;
                        });
                        $scope.SelectedItem.replacement.itemid = $scope.itemid;
                        $scope.SelectedItem.replacement.productid = $scope.productid;
                        $scope.SelectedItem.replacement.printingname = typeof (item) != 'undefined' ? item.name : '';
                        $scope.ShowList = true;
                    }

                    $scope.CurrentProductid = -1;
                    $scope.FilterItemList = function (productid) {
                        if (typeof (productid) == 'undefined') productid = $scope.productid;
                        if ($scope.CurrentProductid != productid) {
                            $scope.ItemList = [];
                            $scope.CurrentProductid = productid;
                            $.each($scope.ItemNameList, function (k, v) {
                                if (v.productid == productid)
                                    $scope.ItemList.push(v);
                            });
                        }
                    }

                    $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
                        $scope.ProductList = results;
                    });

                    $scope.ItemNameList = restService.Get('ReferencelistService/ITEMS', true).then(function (results) {
                        $scope.ItemNameList = results;
                    });

                    $scope.ShowProductDialog = function (item) {
                        $scope.SelectedItem = item;
                        $scope.ShowList = false;

                    }
                    $scope.PopupList = $scope.ngDialogData[1];
                    $.each($scope.PopupList, function (k, v) {
                        v.replacement = (JSON.parse(JSON.stringify(v)));
                    });
                    $scope.Transports = $scope.ngDialogData[0].Transports;

                    $scope.Select = function () {
                        var BuyBack = {
                            'changestatus': $scope.ngDialogData[2], 'status': $scope.ngDialogData[3],
                            'BuyBackdate': $scope.BuyBackdate, 'Replacements': $scope.PopupList
                        }

                        var srequest = {
                            "Item": (JSON.parse(JSON.stringify(BuyBack))),
                            "securitytoken": "String content"
                        }
                        $http({
                            method: 'POST',
                            url: "BuybackService/ChangeStatus",
                            data: srequest
                        }).success(function (result) {
                            if (typeof (result) != 'undefined' && result.errormessage != null && result.errormessage.length > 0) {
                                $rootScope.Toaster.Error(result.errormessage);
                            }
                            else {
                                $scope.closeThisDialog();
                            }
                        }).error(function (result) {
                            $rootScope.Toaster.Error(result);
                        });
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {

                    }
                }
            });

        }
        $scope.igrid = null;
        $scope.ConfigGrid = function () {
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('status', 'Status', 200, $scope.StatusList);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 200, $scope.Suppliers);
            igrid.AddFormatedColumn('BuyBackdate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 150);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('productid', 'Product', 200, $scope.Products);
            igrid.AddSimpleColumn('barcode', 'Barcode', 150);
            igrid.AddSimpleColumn('printingname', 'Item', 150);
            igrid.AddFormatedColumn('qty', 'Qty', 50);
            igrid.AddFormatedColumn('receivable', 'Receivable', 100);

            igrid.Initialize();
            $scope.igrid = igrid;
        }

        $scope.SearchItems = function (itemid) {
            var dialog = ngDialog.open({
                template: 'ClientSide/Views/Item.html?' + new Date(),
                data: [$scope, itemid],
                width: '950px',
                controller: 'ItemController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                    return;

                if (!data.value.IsEdit) {
                    $scope.Items.push(data.value);
                }
            });
        }
       

        $scope.SearchCustomer = function () {

            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.PopupList = [];
                    $scope.Customer = null;
                    $scope.CloseIfOne = false;

                    $scope.Select = function () {
                        $scope.Customer = null;
                        $.each($scope.PopupList, function (k, v) {
                            if (v.selected)
                                $scope.Customer = v;
                        });
                        if ($scope.Customer != null)
                            $scope.closeThisDialog($scope.Customer);
                    };

                    $scope.Create = function () {
                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno,
                                'gstno': $scope.gstno, 'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };

                    $scope.hasvalue = function (val) {
                        return typeof (val) != 'undefined' && val != null &&
                            val != '' && val.length > 0;
                    }

                    $scope.Search = function (closeIfOne) {

                        $scope.CloseIfOne = typeof (closeIfOne) != 'undefined' ? closeIfOne : false;
                        if (!$scope.hasvalue($scope.name) &&
                            !$scope.hasvalue($scope.area) &&
                            !$scope.hasvalue($scope.contactno) &&
                            !$scope.hasvalue($scope.gstno) &&
                            !$scope.hasvalue($scope.loyeltyno))
                            return;

                        var request = {
                            "Item": {
                                'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno,
                                'hasgst': $scope.hasgst, 'loyeltyno': $scope.loyeltyno
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Search",
                            data: srequest
                        }).success(function (result) {
                            if (result.length == 1 && $scope.CloseIfOne) {
                                $scope.closeThisDialog(result[0]);
                            }
                            $scope.PopupList = result;
                        });
                    };

                    $timeout(function () {
                        $scope.setFocuscard_no();
                    }, 10);

                    $scope.contactno = $scope.ngDialogData[0].BuyBack.customerphoneno;
                    if (typeof ($scope.contactno) != 'undefined' && $scope.contactno != null) {
                        if ($scope.contactno.length != 10 || $scope.ngDialogData[0].GetNumber($scope.contactno) <= 0) {
                            $scope.name = $scope.contactno;
                            $scope.contactno = '';
                        }
                        $scope.Search(true);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {
                        $scope.BuyBack.customerid = data.value.id;
                        $scope.BuyBack.customername = data.value.name;
                        $scope.BuyBack.customerphoneno = data.value.mobileno;
                        $scope.BuyBack.customeraddress = data.value.address;
                    }
                }
            });
        }

        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);

