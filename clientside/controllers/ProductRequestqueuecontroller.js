
var app = angular.module('InfinityLab');

app.controller('ProductRequestQueueController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout) {

        $scope.SearchRef = restService.SearchReference;
        $rootScope.AccordionName = $location.path();

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });
        $scope.PatternList = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.PatternList = results;
        });
        $scope.LocationList = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.PriorityList = restService.Get('ReferencelistService/PRIORITY', true).then(function (results) {
            $scope.PriorityList = results;
        });

        $scope.PrfReson = restService.Get('ReferencelistService/PRFRESON', true).then(function (results) {
            $scope.PrfReson = results;
        });

        $scope.StyleList = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.StyleList = results;
        });

        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });
        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });

        $scope.MaterialList = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.MaterialList = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.DeliveryMode = [{ id: 0, name: 'Any' },
        { id: 1, name: 'Bag' },
        { id: 2, name: 'Box' },
        { id: 3, name: 'Pcs' },
        { id: 4, name: 'Meter' }];

        $scope.ErrorStyle = "success";
        $scope.SearchResult = [];
        $scope.SelectedResult = [];

        $scope.Entity = {
            "supplierid": 0,
            "locationid": 0,
            "requestedby": 0,
            "fromdate": null,
            "todate": null,
            "reasonid": 0,
            "priorityid": 0,
            "pending": true,
            "printingname": '',
            "productid": 0,
            "brandid": 0,
            "patternid": 0,
            "styleid": 0,
            "sizeid": 0,
            "colourid": 0,
            "materialid": 0,
            "id": 0
        };
        $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorMessage = "";

        $scope.Search = function () {
            if ($scope.ProductRequest.locationid == null || $scope.ProductRequest.locationid == 0) {
                $rootScope.Toaster.Error("Please select Location !");
                return;
            }
            var request = {
                "Item": (JSON.parse(JSON.stringify($scope.ProductRequest))),
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ProductrequestformService/SearchOrders",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No PO Request found.");
                }
                $scope.ConfigureGrid();
                $scope.SearchQtyChanged();
            });
        };

        function DateFormatter(value, format) {
            var result = jQuery.format.date(value, format);
            if (result == '0001-01-01' || result == '01-01-0001') return '';
            return result;
        }

        $scope.SearchResult = [];
        $scope.SelectedResult = [];
        $scope.lgrid = null;
        $scope.rgrid = null;
        $scope.ConfigureGrid = function () {
            if ($scope.lgrid != null)
                $scope.lgrid.destroy();
            var igrid = new InfyGrid('grdleft', '470px', '100%', true, true, true, 100);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 140, $scope.Suppliers);
            igrid.AddSimpleColumn('productname', 'Product', 100);
            igrid.AddSimpleColumn('description', 'Description', 150);
            igrid.AddSimpleColumn('brandname', 'Brand', 60);
            igrid.AddSimpleColumn('sizename', 'Size', 60);
            igrid.AddSimpleColumn('designid', 'Design', 60);
            igrid.AddCustomColumn('isinterstatesale', 'Qty', 60, function (item, col) {
                return item.qty - (item.rqty + item.cqty);
            }).total = true;
            igrid.AddSimpleColumn('pricerange', 'Price', 50).cssClass = 'rightalign';;
            igrid.AddCustomColumn('requestedon', 'Date', 80, function (item, col) {
                return DateFormatter(item.requestedon, 'dd-MM-yyyy');
            });
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.lgrid = igrid;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.SearchQtyChanged();
                }, 10);
            };
            $scope.lgrid.UpdateData($scope.SearchResult);

            if ($scope.rgrid != null)
                $scope.rgrid.destroy();
            var igrid = new InfyGrid('grdright', '470px', '100%', true, true, true, 100);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SelectedResult);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 140, $scope.Suppliers);
            igrid.AddSimpleColumn('productname', 'Product', 100);
            igrid.AddSimpleColumn('description', 'Description', 150);
            igrid.AddSimpleColumn('sizename', 'Size', 100);
            var column = igrid.AddColumn({
                id: "oqty",
                name: "Qty",
                field: "oqty",
                width: 60,
                cssClass: "cell-title",
                editor: Slick.Editors.Text,
                'sortable': true
            });
            igrid.options.autoEdit = true;
            column.total = true;
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'qty') {
                    $timeout(function () {
                        $scope.SelectQtyChanged();
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            $scope.rgrid = igrid;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.SelectQtyChanged();
                }, 10);
            };

            $scope.rgrid.UpdateData($scope.SelectedResult);
        }

        $scope.HasDiffer = function (actual, item) {
            return actual == null || item == null ||
                actual.productid != item.productid ||
                actual.sizeid != item.sizeid ||
                actual.materialid != item.materialid ||
                actual.patternid != item.patternid ||
                actual.colourid != item.colourid ||
                actual.styleid != item.styleid;
        }

        $scope.SearchItemByName = function () {
            var request = {
                "Item": {
                    'name': $scope.ProductRequest.Details.printingname, 'productid': $scope.ProductRequest.Details.productid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/SearchByName",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {
                    $scope.ProductRequest.Details = result[0];
                    $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify($scope.ProductRequest.Details)));
                }
                else if (result.length > 1)
                    $scope.ShowProductSearch(result);
            });
        };


        $scope.Save = function () {

            var request = {
                "item": $scope.Brand,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "EmployeeService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.ErrorMessage = "Successfully saved";
                    $scope.ErrorStyle = "success";
                    $scope.Brand = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });
        }


        $scope.AddItems = function () {
            var selected = [];
            $.each($scope.SearchResult, function (k, v) {
                if (typeof (v.selected) != 'undefined' && v.selected == true)
                    selected.push(v);
            });
            $.each(selected, function (k, v) {
                $scope.SearchResult.splice($scope.SearchResult.indexOf(v), 1);
                var exist = $scope.SelectedResult.find(function (item) {
                    item.productrequestformid == v.productrequestformid;
                });
                if (exist == null) {
                    v.oqty = v.qty - (v.rqty + v.cqty);
                    $scope.SelectedResult.push(v);
                }
            });

            if (selected != null && selected.length == 0) {
                $rootScope.Toaster.Info("Please select an item for Purchase Order.");
                return;
            }
            $scope.SelectQtyChanged();
            $scope.SearchQtyChanged();
            $scope.lgrid.UpdateData($scope.SearchResult);
            $scope.rgrid.UpdateData($scope.SelectedResult);
        };

        $scope.CancelItems = function () {
            var selected = [];
            $.each($scope.SelectedResult, function (k, v) {
                if (v.oqty > 0) {
                    if (v.oqty <= v.qty - (v.cqty + v.rqty)) {
                        v.reverse = v.cqty;
                        v.cqty += v.oqty;
                        selected.push(v);
                    }
                    else {
                        v.selected = false;
                    }
                }
            });

            if (selected != null && selected.length == 0) {
                $rootScope.Toaster.Info("Not selected or Cancelled qty can't be more then the requested qty");
                return;
            }

            var request = {
                "Items": selected,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ProductrequestformService/CancelOrders",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {

                    $.each(result, function (k, v) {
                        var existitem = $scope.SelectedResult.find(function (item) {
                            return item.productrequestformid == v.productrequestformid;
                        });
                        if (existitem != null) {
                            if (v.cqty > 0) {
                                existitem.cqty = v.cqty;
                                existitem.oqty = existitem.qty - (existitem.rqty + existitem.cqty);
                            } else {
                                existitem.cqty = v.reverse;
                                existitem.selected = false;
                            }
                        }
                    });
                    $rootScope.Toaster.Error("PRF Cancelled.");
                    $scope.SelectQtyChanged();
                    $scope.SearchQtyChanged();
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });

        };

        $scope.Remove = function () {
            var selected = [];

            $.each($scope.rgrid.GetSelectedRows(), function (k, v) {
                selected.push(v);
            });

            $.each(selected, function (k, v) {
                $scope.SelectedResult.splice($scope.SelectedResult.indexOf(v), 1);
                $scope.SearchResult.push(v);
            });
            $scope.SelectQtyChanged();
            $scope.SearchQtyChanged();
            $scope.lgrid.UpdateData($scope.SearchResult);
            $scope.rgrid.UpdateData($scope.SelectedResult);
        };

        $scope.Clear = function () {
            $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchResult = [];
            $scope.SelectedResult = [];
            $scope.SelectQtyChanged();
            $scope.SearchQtyChanged();
        };

        $scope.ShowProductSearch = function (list) {
            $.each(list, function (k, v) {
                v.display = '';
                if (v.productname != null && v.productname.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.productname;
                if (v.brandname != null && v.brandname.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.brandname;
                if (v.sizename != null && v.sizename.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.sizename;
                if (v.patternname != null && v.patternname.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.patternname;
                if (v.stylename != null && v.stylename.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.stylename;
                if (v.colourname != null && v.colourname.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.colourname;
                if (v.materialname != null && v.materialname.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.materialname;
            });

            var dialog = ngDialog.open({
                template: 'ProductSearch',
                data: { 'scope': $scope, 'list': list },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.PopupList = $scope.ngDialogData.list;

                    $scope.Selected = function (item) {
                        $scope.ngDialogData.result = item;
                        $scope.closeThisDialog($scope.ngDialogData);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.ProductRequest.Details = data.value.result;
                        $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify(data.value.result)));
                    }
                }
            });
        }

        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;
            if (typeof (value) == 'undefined' || value == '')
                return 0
            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;
            return nvalue;
        };

        $scope.SelectQtyChanged = function () {
            $scope.SelectQty = 0;

            $.each($scope.SelectedResult, function (k, v) {
                v.selected = false;
            });

            $.each($scope.rgrid.GetSelectedRows(), function (k, v) {
                if (v.selected) {
                    $scope.SelectQty += $scope.GetNumber(v.oqty);
                }
            });
        };

        $scope.SearchQtyChanged = function () {
            $scope.SearchQty = 0;
            $.each($scope.SearchResult, function (k, v) {
                v.selected = false;
            });
            $.each($scope.lgrid.GetSelectedRows(), function (k, v) {
                v.selected = true;
                $scope.SearchQty += (v.qty - (v.rqty + v.cqty));
            });
        };

        $scope.SelectedAll = function (selected, list) {
            $.each(list, function (k, v) {
                v.selected = selected;
            });
        };

        $scope.ShowPurchaseOrderGenerator = function (list) {
            var items = [];
            $.each(list, function (k, v) {
                var exist = items.find(function (item) {
                    return item.itemid == v.itemid && item.designid == v.designid;
                });
                if (exist == null) {
                    var item = (JSON.parse(JSON.stringify(v)));
                    item.productrequestformids = [{ 'id': v.productrequestformid, 'qty': v.oqty }];
                    items.push(item);
                } else {
                    exist.productrequestformids.push({ 'id': v.productrequestformid, 'qty': v.oqty });
                    exist.oqty += v.oqty;
                }
            });

            if ($scope.ProductRequest.supplierid == 'undefined' || $scope.ProductRequest.supplierid == null
                || $scope.ProductRequest.supplierid == 0)
                $scope.ProductRequest.supplierid = items[0].supplierid;

            var dialog = ngDialog.open({
                template: 'PurchaseOrderGenerator',
                data: { 'scope': $scope, 'list': items },
                width: '750px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.PopupList = $scope.ngDialogData.list;
                    $scope.supplierid = $scope.ngDialogData.scope.ProductRequest.supplierid;
                    $scope.shopcontactemployeeid = ($scope.PopupList[0] != 'undefined' && $scope.PopupList[0] != null) ? $scope.PopupList[0].requestedby : 0;
                    $scope.locationid = 0;
                    $scope.companyid = $scope.ngDialogData.scope.BuyingCompany[0].id;
                    $scope.deliverylocationid = 0;
                    $scope.podate = new Date();
                    $scope.expectedate = null;
                    $scope.SupplierList = $scope.ngDialogData.scope.Suppliers;
                    $scope.LocationList = $scope.ngDialogData.scope.LocationList;
                    $scope.CompanyList = $scope.ngDialogData.scope.BuyingCompany;
                    $scope.EmployeeList = $scope.ngDialogData.scope.Employees;
                    $scope.DeliveryModeList = $scope.ngDialogData.scope.DeliveryMode;
                    $scope.Save = function (item) {

                        if ($scope.supplierid == null || $scope.supplierid == 0) {
                            $rootScope.Toaster.Info("Please select Supplier");
                            return;
                        }
                        if ($scope.locationid == null || $scope.locationid == 0) {
                            $rootScope.Toaster.Info("Please select Location");
                            return;
                        }
                        if ($scope.deliverylocationid == null || $scope.deliverylocationid == 0) {
                            $rootScope.Toaster.Info("Please select Delivery Location");
                            return;
                        }

                        var poRequest = {
                            'Purchaseorderitems': $scope.PopupList, isactive: true,
                            version: 1,
                            supplierid: $scope.supplierid,
                            locationid: $scope.locationid,
                            companyid: $scope.companyid,
                            deliverylocationid: $scope.deliverylocationid,
                            shopcontactemployeeid: $scope.shopcontactemployeeid,
                            mode: 2,
                            podate: $scope.podate,
                            expectedate: $scope.expectedate
                        };
                        var request = {
                            "item": poRequest,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "PurchaseorderService/Save",
                            data: srequest
                        }).success(function (result) {

                            if (result != null && result.id > 0) {
                                $scope.ngDialogData.result = result;
                                $scope.ngDialogData.success = true;
                                $scope.closeThisDialog($scope.ngDialogData);

                                $rootScope.Toaster.Info("PO generated successfully with PO No : " + result.pono);
                            }
                            else {
                                $scope.ErrorMessage = result.ErrorMessage;
                                $scope.ErrorStyle = "danger";
                            }

                        }).error(function (result) {
                        });

                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document' &&
                        data.value.success) {
                        $scope.Search();
                        $scope.SelectedResult = [];
                    }
                }
            });
        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);

    }]);