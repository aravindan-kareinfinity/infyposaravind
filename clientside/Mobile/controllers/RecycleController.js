var app = angular.module('InfinityLab');

app.controller('RecycleController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', 'restService', '$timeout', 'Excel', 'ngDialog',
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

        $scope.discountchanged = function (item) {
            item.newdiscountperitem = (item.newdiscount / $scope.GetNumber(item.newqty)).round(2);
            item.newtax = ($scope.GetNumber(item.newrate - (item.newdiscount / $scope.GetNumber(item.newqty)))
                * $scope.GetNumber(item.newqty) * item.newtaxpercentage / 100).round(2);
            item.newreceivable = ((item.newrate) * $scope.GetNumber(item.newqty)).round(2);
            item.newdiscountpercentage = (item.newdiscount / item.newreceivable * 100).round(2);
        }

        $scope.taxpercentagechanged = function (item) {

            item.newdiscountperitem = (item.newrate * item.newdiscountpercentage / 100).round(2);
            item.newdiscount = (item.newdiscountperitem * $scope.GetNumber(item.newqty)).round(2);
            item.newtax = ($scope.GetNumber(item.newrate - (item.newdiscount / $scope.GetNumber(item.newqty)))
                * $scope.GetNumber(item.newqty) * item.newtaxpercentage / 100).round(2);
            item.newreceivable = ((item.newrate) * $scope.GetNumber(item.newqty)).round(2);
        }

        $scope.StatusList = [{ 'id': -1, 'name': 'All' }, { 'id': 0, 'name': 'Pending' }, { 'id': 1, 'name': 'Replacement' },
        { 'id': 2, 'name': 'Credit Note' }, { 'id': 3, 'name': 'Debit Note' }, { 'id': 4, 'name': 'Return' },
        { 'id': 5, 'name': 'Alternate' }, { 'id': 6, 'name': 'Replaced Items' }];

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
            "returndate": new Date(),
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
            $scope.DOA = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.DOA.returndate = new Date();
        }
        $scope.New();

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.Locations = results;
            results.push({ 'id': 0, 'name': 'WAREHOUSE', 'parentgroup': 0 });
            $scope.DOA.locationid = 0;
        });

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
            returndate: "",
            status: -1
        };

        $scope.LocationChanged = function () {
            var location = $scope.Locations.find(function (item) {
                return item.id == $scope.DOA.locationid;
            });
            if (location && location != null && typeof (location.productgroup) != 'undefined') {
                $scope.DOA.companyid = location.productgroup;
            }
        }

        $scope.fileuploadedcomplete = function (value) {
            $scope.SearchInvoiceCondition.mode = "1";
            $scope.uploadfile = null;
            var barcodes = [];
            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    barcodes.push(barcode);
                }
            });

            var searchcondtion = {
                'locationid': $scope.DOA.locationid, 'barcodes': barcodes, 'mode': '1', 'allowall': true, 'companyid': $scope.DOA.companyid
            };
            $scope.SearchItemInternal(searchcondtion);
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




        $scope.SearchItems = function () {
            $scope.SearchInvoiceCondition.mode = '1';
            $scope.SearchInvoiceCondition.allowall = true;
            var request = {
                "Item": $scope.SearchInvoiceCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DeathonarrivalService/SearchItems",
                data: srequest
            }).success(function (result) {
                $scope.AddReturnItems(result.Items);
            }).error(function (result) {
            });
        };

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


                    var group = $scope.DOA.Lrinvoicereturngroups.find(function (item) {
                        return item.supplierid == v.supplierid;
                    });
                    if (group && group != null) {
                        group.qty += returnqty;
                        if ($scope.DOA.Lrinvoicereturngroups.length > 1) {
                            var index = $scope.DOA.Lrinvoicereturngroups.indexOf(group);
                            $scope.DOA.Lrinvoicereturngroups.splice(index, 1);
                            $scope.DOA.Lrinvoicereturngroups.splice(0, 0, group);
                        }
                    }
                    else {
                        var supplier = $scope.Suppliers.find(function (item) {
                            return item.id == v.supplierid;
                        });
                        group = {
                            'supplierid': v.supplierid, 'suppliername': supplier.name, 'qty': returnqty
                        };
                        $scope.DOA.Lrinvoicereturngroups.splice(0, 0, group);
                    }


                    var existing = $scope.DOA.Items.find(function (item) {
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
                        $scope.DOA.Items.push(v);
                    }

                }
            });

            if ($scope.SearchInvoiceCondition.mode == 1 && $scope.DOA.mode == 1)
                $scope.SearchInvoiceCondition.returnqty = null;
            $scope.CalculateTotal();
            $scope['setFocusbarcode']();
        };

        $scope.CalculateTotal = function (onlytotal) {

            if (typeof (onlytotal) == 'undefined' || !onlytotal) {
                $scope.DOA.tax = 0;
                $scope.DOA.discount = 0;
                $scope.DOA.receivable = 0;
                $scope.DOA.total = 0;
                $scope.DOA.qty = 0;
                $scope.DOA.piece = 0;
                $.each($scope.DOA.Items, function (k, v) {
                    $scope.DOA.tax = $scope.GetNumber(v.tax) + $scope.DOA.tax;
                    $scope.DOA.discount = $scope.GetNumber(v.discount) + $scope.DOA.discount;
                    $scope.DOA.receivable = $scope.GetNumber(v.receivable) + $scope.DOA.receivable;
                    $scope.DOA.qty = $scope.GetNumber(v.qty) + $scope.DOA.qty;
                    $scope.DOA.piece += $scope.GetNumber(v.qty);
                });
            }

            $scope.DOA.tax = $scope.DOA.tax.round(2);
            $scope.DOA.discount = $scope.DOA.discount.round(2);
            $scope.DOA.receivable = $scope.DOA.receivable.round(2);
            $scope.DOA.total = ($scope.DOA.receivable + $scope.DOA.tax - $scope.DOA.discount).round(0) + $scope.GetNumber($scope.DOA.freightamt);
        }



        $scope.ManagePurchaseItem = function (action, item) {
            if (action == 'Delete') {
                var index = $scope.DOA.Items.indexOf(item);
                $scope.DOA.Items.splice(index, 1);
                $scope.CalculateTotal();

                var ditem = item;

                var group = $scope.DOA.Lrinvoicereturngroups.find(function (sitem) {
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

                var group = $scope.DOA.Lrinvoicereturngroups.find(function (sitem) {
                    return sitem.supplierid == item.supplierid;
                });

                if (typeof (group) == 'undefined')
                    group = $scope.DOA.Lrinvoicereturngroups[0];

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


        $scope.QtyChanged = function (item) {
            /*var oldValue = item.receivable;
            var oldtaxValue = item.tax;
            var olddiscount = item.discount;
            item.receivable = isNaN(item.rate * item.qty) ? 0 : item.rate * item.qty;
            item.tax = isNaN(item.taxrate * item.qty) ? 0 : item.taxrate * item.qty;
            item.discount = isNaN(item.discountrate * item.qty) ? 0 : item.discountrate * item.qty;
            $scope.DOA.tax = $scope.DOA.tax - oldtaxValue + item.tax;
            $scope.DOA.receivable = $scope.DOA.receivable - oldValue + item.receivable;
            $scope.DOA.discount = $scope.DOA.discount - olddiscount + item.discount;*/
        };

        $scope.Save = function () {

            if ($scope.DOA.id > 0 && $scope.Permission.p_PORETURN.modify == false) {
                $rootScope.Toaster.Info("No Permission!!");
                return;
            }

            if (typeof ($scope.DOA.locationid) == 'undefined' || $scope.DOA.locationid == null) {
                $rootScope.Toaster.Info("Please Select Stock Location.!");
                return;
            }

            if ($scope.DOA.returndate == null || $scope.DOA.returndate == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info("Please Select Date.!");
                return;
            }

            var request = {
                "Item": (JSON.parse(JSON.stringify($scope.DOA))),
                "securitytoken": "String content"
            }

            request.Item.tax = parseFloat(request.Item.tax).round(2);
            request.Item.discount = parseFloat(request.Item.discount).round(2);
            request.Item.receivable = parseFloat(request.Item.receivable).round(2);
            request.Item.total = parseFloat(request.Item.total).round(0);
            request.Item.rounding = request.Item.total - (request.Item.tax + request.Item.receivable - request.Item.discount);
            request.Item.qty = parseFloat(request.Item.piece).round(2);
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DeathonarrivalService/SaveMultiple",
                data: srequest
            }).success(function (result) {

                if (result != null && typeof (result.Items.length) != 'undefined' &&
                    result.Items.length > 0 && result.Items[0].id > 0) {
                    $scope.CanSave = false;
                    if (result.Items.length > 0) {
                        $rootScope.Toaster.Info("DOA Saved Successfully with No : " + result.Items[0].registerno);
                    }
                }
                else if (typeof (result.errormessage) != 'undefined') {
                    $rootScope.Toaster.Error(result.errormessage);
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
                url: "DeathonarrivalService/Search",
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
                        var doa = {
                            'changestatus': $scope.ngDialogData[2], 'status': $scope.ngDialogData[3],
                            'doadate': $scope.doadate, 'Replacements': $scope.PopupList
                        }

                        var srequest = {
                            "Item": (JSON.parse(JSON.stringify(doa))),
                            "securitytoken": "String content"
                        }
                        $http({
                            method: 'POST',
                            url: "DeathonarrivalService/ChangeStatus",
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
            igrid.AddFormatedColumn('doadate', 'Date', 80, 'dd-MM-yyyy');
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

        $scope.DeleteGroup = function (item) {

            $scope.DOA.Lrinvoicereturngroups.splice($scope.DOA.Lrinvoicereturngroups.indexOf(item), 1);

            var deletelist = [];
            $.each($scope.DOA.Items, function (k, v) {
                if (v.supplierid == item.supplierid) {
                    deletelist.push(v);
                }
            });

            $.each(deletelist, function (k, v) {
                var index = $scope.DOA.Items.indexOf(v);
                $scope.DOA.Items.splice(index, 1);
            });
            $scope.CalculateTotal();
        }

        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);

