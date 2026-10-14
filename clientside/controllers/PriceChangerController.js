
var app = angular.module('InfinityLab');

app.controller('PriceChangerController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {

        $rootScope.AccordionName = $location.path();
        $scope.PriceModes = [
            { 'id': 1, 'name': 'Retail Price' },
            { 'id': 2, 'name': 'Dealer Price' }];
        $scope.SearchModes = [
            { 'id': 1, 'name': 'Warehouse' },
            { 'id': 2, 'name': 'Retail' },
            { 'id': 3, 'name': 'Tax - Intermediate' }];
        $scope.editall = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.ItemList = [];
        $scope.SupportDealerPrice = false;
        $scope.SupportRounding = false;
        $scope.System_b2brounding = 0;
        $scope.System_pricetag = false;
        $scope.LoadingConfiguration = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadingConfiguration, 100);
            }
            else {
                $scope.SupportDealerPrice = $rootScope.UserInfo.appsettings.system_dealerprice || false;
                $scope.System_pricetag = $rootScope.UserInfo.appsettings.system_invcol_pricetag || false;
                $scope.SupportRounding = $rootScope.UserInfo.appsettings.system_supportrounding || false;
                if (false || $scope.UserInfo.appsettings.system_b2brounding)
                    $scope.System_b2brounding = 4;
            }
        }

        $scope.LoadingConfiguration();

        $scope.SalesTax = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.SalesTax = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });

        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.PriceTags = restService.Get('ReferencelistService/PRICETAGS', true).then(function (results) {
            $scope.PriceTags = results;
        });
        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
        });
        $scope.Patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Patterns = results;
        });
        $scope.Styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Styles = results;
        });

        $scope.Entity = {
            "id": 0,
            "locationid": 0,
            "Flourid": 0,
            "sectionid": 0,
            "boxno": "",
            "typeid": 0,
            "capacity": 0,
            "stateid": 0,
            "countryid": 0,
            "productid": 0,
            "brandid": 0,
            "materialid": 0,
            "patternid": 0,
            "colourid": 0,
            "styleid": 0,
            "sizeid": 0,
            "supplierid": 0,
            "barcode": "",
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "mode": 1,
            "pricemode": 1,
            "changedon": new Date(),
            "modifiedon": ""
        };
        $scope.Price = (JSON.parse(JSON.stringify($scope.Entity)));


        $scope.CanSave = typeof ($rootScope.UserInfo.p_PRICECHANGER.modify) == 'undefined' ? false : $rootScope.UserInfo.p_PRICECHANGER.modify;


        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Price = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.compute = function (item, type) {
            if (type == 'discountpercent' || type == 'price')
                item.ndiscount = ($rootScope.GetNumber(item.nprice) * $rootScope.GetNumber(item.npdiscount) / 100).round(2);
            else if (type == 'discount')
                item.npdiscount = ($rootScope.GetNumber(item.ndiscount) / $rootScope.GetNumber(item.nprice) * 100).round(0);
            else if (type == 'profit') {
                item.nprice = (($rootScope.GetNumber(item.ibuyingprice) * $rootScope.GetNumber(item.nprofit)) / 100 + item.ibuyingprice).round(0);
                item.ndiscount = ($rootScope.GetNumber(item.nprice) * $rootScope.GetNumber(item.npdiscount) / 100).round(0);
            } else if (type == 'pdealerprice') {
                item.ndealerprice = (($rootScope.GetNumber(item.ibuyingprice) * $rootScope.GetNumber(item.npdealerprice)) / 100 + item.ibuyingprice).round($scope.System_b2brounding);
            } else if (type == 'dealerprice') {
                if (item.ndealerprice > 0)
                    item.npdealerprice = ($rootScope.GetNumber(item.ndealerprice) / $rootScope.GetNumber(item.ibuyingprice) * 100 - 100).round(0);
                else
                    item.npdealerprice = 0;
            }
            else if (type == 'newsaleprice') {
                item.ndiscount = item.nprice - item.newsaleprice;
                if (item.ndiscount < 0) {
                    item.ndiscount = 0;
                    item.npdiscount = 0;
                    item.nprice = item.newsaleprice;
                    $scope.compute('price', item);
                }
                else
                    $scope.compute('discount', item);
            }
        };

        $scope.ManageAllItems = function (action) {
            $.each($scope.ItemList, function (k, v) {
                if (action == "Edit") {
                    v.edit = true;
                    v.oprice = v.price;
                    v.nprice = v.price;
                    v.ndiscount = v.discount;
                    v.npdiscount = v.pdiscount;
                    v.npdealerprice = v.pdealerprice;
                    v.ndealerprice = v.dealerprice;
                    v.newsaleprice = v.newsaleprice;
                }
                else if (action == "Delete") {
                    $scope.ItemList = [];
                }
                else {
                    v.nprice = 0;
                    v.ndiscount = 0;
                    v.npdiscount = 0;
                    v.npdealerprice = 0;
                    v.ndealerprice = 0;
                    v.newsaleprice = 0;
                    v.edit = false;
                }
            });

            if (action != "Delete")
                $scope.editall = action == "Edit";
        }

        $scope.allitem = {};
        $scope.ManageItems = function (item, action, focusid) {
            if (action == "Edit") {
                item.edit = true;
                item.oprice = item.price;
                item.nprice = item.price;
                item.ndiscount = item.discount;
                item.npdiscount = item.pdiscount;
                item.npdealerprice = item.pdealerprice;
                item.ndealerprice = item.dealerprice;
                item.newsaleprice = item.newsaleprice;

                //$scope.allitem.nprice = item.price;
                //$scope.allitem.ndiscount = item.discount;
                //$scope.allitem.npdiscount = item.pdiscount;
            } else if (action == "Delete") {
                $scope.ItemList.splice($scope.ItemList.indexOf(item), 1);
            }
            else {
                item.nprice = 0;
                item.ndiscount = 0;
                item.npdiscount = 0;
                item.npdealerprice = 0;
                item.ndealerprice = 0;
                item.newsaleprice = 0;
                item.edit = false;
            }

            if (typeof ($scope[focusid]) != 'undefined')
                $scope[focusid]();
        };

        $scope.SearchByBarcode = function (barcode) {
            if ($scope.Price.mode == 2 && ($scope.Price.locationid == null || $scope.Price.locationid == 0)) {
                $rootScope.Toaster.Info("Please select retail location.");
                return;
            }

            //barcode
            if ($scope.ItemList && $scope.ItemList.length > 0) {
                var existing = $scope.ItemList.find(function (item) {
                    return item.barcode == barcode;
                });
                if (existing && existing != null) {
                    var printcopy = parseInt(existing.printcopy);
                    if (isNaN(printcopy))
                        printcopy = 1;
                    else
                        printcopy++;
                    if (existing.stockqty < printcopy) {
                        $rootScope.Toaster.Error(barcode + " (" + printcopy + ") - Stock lesser than the scanned qty (" + existing.stockqty + ")");
                    }
                    else {
                        existing.printcopy = printcopy;
                        $scope.igrid.UpdateData($scope.ItemList);
                        $scope.Price.barcode = '';
                    }
                    return;
                }
            }

            $scope.Price.barcode = '';
            //.replace("#", "%23")
            $scope.fileuploadedcomplete(null, barcode);

            /*if ($scope.Price.mode == 2) {
                restService.Get('SalablegoodsService/Barcode/' + $scope.Price.locationid + '/' + $scope.Price.barcode).then(function (results) {
                    var exist = $scope.ItemList.find(function (itm) {
                        return (itm.salablegoodsid == results.id);
                    })
                    if (exist == null && results.id > 0) {
                        $scope.ItemList.push({
                            'printingname': results.sellingname, 'sellingmode': results.sellingmode, 'buyingprice': results.buyingprice, 'marginpercentage': results.marginpercentage,
                            'barcode': results.barcode, 'oprice': results.price, 'price': results.price, 'discount': results.discount, 'serialno': results.serialno,
                            'nprice': 0, 'odiscount': results.discount, 'pdiscount': (results.discount / results.price * 100).round(0),
                            'ndiscount': 0, 'stockqty': (results.iqty - (results.dqty + results.oqty)),'mrp':results.mrp,
                            'salablegoodsid': results.id, 'edit': false, 'npdiscount': 0
                        });
                        $scope.ConfigGrid($scope.ItemList);
                    }
                    $scope.Price.barcode = '';
                });
            } else {
                restService.Get('StockService/Barcode/' + $scope.Price.barcode).then(function (results) {
                    var exist = $scope.ItemList.find(function (itm) {
                        return (itm.stockid == results.id);
                    })
                    if (exist == null && results.id > 0) {
                        $scope.ItemList.push({
                            'printingname': results.printingname, 'sellingmode': results.sellingmode, 'buyingprice': results.buyingprice, 'marginpercentage': results.marginpercent,
                            'barcode': results.barcode, 'oprice': results.price, 'price': results.price, 'discount': results.discount, 'serialno': results.serialno,
                            'nprice': 0, 'odiscount': results.discount, 'pdiscount': (results.discount / results.price * 100).round(0),
                            'ndiscount':0, 'stockqty': (results.qty - (results.damagedqty + results.transfered + results.returnqty)),
                            'stockid': results.id, 'edit': false, 'npdiscount': 0, 'mrp': results.mrp
                        });
                        $scope.ConfigGrid($scope.ItemList);
                    }
                    $scope.Price.barcode = '';
                });
            }*/

        };

        //$scope.Search = function () {
        //    $scope.SearchCondition.iswarehouse = $scope.Price.mode == 1;
        //    var request = {
        //        "Item": $scope.SearchCondition,
        //        "securitytoken": "String content"
        //    }

        //    var srequest = JSON.stringify(request);

        //    $http({
        //        method: 'POST',
        //        url: "PricechangeService/Search",
        //        data: srequest
        //    }).success(function (result) {
        //        $scope.SearchResult = result;
        //        if (result == null || result.length == 0) {
        //            $rootScope.Toaster.Info("Item not found.");
        //        }
        //    }).error(function (result) {
        //    });

        //}
        $scope.SearchItems = function (id) {

            if ($scope.Price.mode == 2 && ($scope.Price.locationid == null || $scope.Price.locationid == 0)) {
                $rootScope.Toaster.Info("Please select retail location.");
                return;
            }
            if ($scope.Price.productid == 0 && $scope.Price.brandid == 0 && $scope.Price.patternid == 0
                && $scope.Price.materialid == 0 && $scope.Price.colourid == 0 && $scope.Price.styleid == 0 && $scope.Price.sizeid == 0
                && $scope.Price.pricetagid == 0 && $scope.Price.supplierid == 0 && ($scope.Price.barcode == null || $scope.Price.barcode == "")) {
                if (($scope.Price.fromdate == 'undefined' || $scope.Price.fromdate == null) &&
                    ($scope.Price.changedfromdate == 'undefined' || $scope.Price.changedtodate == null)) {
                    if ($scope.Price.mode != 3) {
                        $rootScope.Toaster.Info("Please select atleast one search creteria");
                        return;
                    }
                }
            }
            $scope.ItemList = [];
            if ($scope.Price.mode == 3) {
                if ($scope.Price.pricemode == 2)
                    $scope.Price.iswarehouse = false;
                else
                    $scope.Price.iswarehouse = ($scope.Price.locationid == null || $scope.Price.locationid == 0);
            }
            else {
                $scope.Price.iswarehouse = $scope.Price.mode == 1;
            }
            var request = {
                "Item": $scope.Price,
                "securitytoken": "String content"
            }



            if (typeof (id) != 'undefined' && id > 0)
                request.id = id;

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PricechangeService/SearchItems",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("Item not found.");
                }
                if ($scope.ItemList.length == 0) {
                    $.each(result, function (k, v) {
                        if (v.discount > 0)
                            v.pdiscount = (v.discount / v.price * 100).round(0);
                        else
                            v.pdiscount = 0;
                        v.stockqty = v.stock;


                        if ($scope.IsDealerPrice && v.dealerprice > 0) {
                            v.pdealerprice = (v.buyingprice / v.dealerprice * 100).round(0);
                        }
                    });
                    $scope.ItemList = result;
                }
                else {
                    $.each(result, function (k, v) {
                        v.stockqty = v.stock;
                        if ($scope.IsDealerPrice && v.dealerprice > 0) {
                            v.pdealerprice = (v.buyingprice / v.dealerprice * 100).round(0);
                        }
                        var exist = $scope.ItemList.find(function (itm) {
                            return (itm.stockid == v.stockid && itm.salablegoodsid == v.salablegoodsid);
                        })
                        if (exist == null) {
                            if (v.discount > 0)
                                v.pdiscount = (v.discount / v.price * 100).round(0);
                            else
                                v.pdiscount = 0;
                            $scope.ItemList.push(v);
                        }
                    });
                }
                $scope.ConfigGrid($scope.ItemList);

            }).error(function (result) {
            });
        }

        $scope.Delete = function () {
            var rows = $scope.igrid.grid.getData().getItems();
            var srows = [];
            $.each(rows, function (k, v) {
                if (v.edit)
                    srows.push(v);
            });

            $.each(srows, function (k, v) {
                rows.splice(rows.indexOf(v), 1);
            })
            $scope.igrid.UpdateData(rows, '_id');
        }

        $scope.IsDealerPrice = false;
        $scope.ConfigGrid = function (result) {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }


            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, false, true, 50);

            igrid.AddButtonColumn('Remove', '', 35, "<button type='button' style='width:25px;height:25px;margin-right:5px;line-height: 0;' value='Remove' title='Remove' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 12px/1 FontAwesome'></i></button>");
            igrid.AddSimpleColumn('printingname', 'Detail', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            if ($scope.System_pricetag)
                igrid.AddSimpleColumn('pricetagname', 'PriceTag', 80);
            var uanexist = result.find(function (item) {
                return item.serialno != null && item.serialno != '';
            });
            if (typeof (uanexist) != 'undefined' && uanexist != null) {
                igrid.AddSimpleColumn('serialno', 'UAN', 100);
                igrid.AddFormatedColumn('mrp', 'MRP', 60, 'N2', 2);
            }
            igrid.AddFormatedColumn('taxpercentage', 'Tax %', 60, 'N2', 2);
            igrid.AddFormatedColumn('stockqty', 'Stock', 60, 'N2', 2);
            groupColumns = [];
            groupColumns.push(
                {
                    id: 'Details',
                    name: 'Details',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];

            igrid.AddFormatedColumn('ibuyingprice', 'Cost', 60, 'N2', 2);

            if ($scope.IsDealerPrice) {
                var column = igrid.AddCustomColumn('pdealerprice', 'Margin %', 70, function (item, col) {
                    return $filter('IndianNumber')(item.pdealerprice, 2) + '%';
                });
                igrid.AddFormatedColumn('dealerprice', 'WS Price', 100, 'N2', 2);
            }
            else {
                igrid.AddFormatedColumn('price', 'Price', 60, 'N2', 2);
                var column = igrid.AddCustomColumn('marginpercentage', 'Margin%', 70, function (item, col) {
                    return $filter('IndianNumber')(item.marginpercentage, 2) + '%';
                });
                column.cssClass = 'rightalign';
                igrid.AddFormatedColumn('discount', 'Discount', 60, 'N2', 2);
                var column = igrid.AddCustomColumn('pdiscount', 'Discount%', 70, function (item, col) {
                    return $filter('IndianNumber')(item.pdiscount, 2) + '%';
                });
                column.cssClass = 'rightalign';
            }

            groupColumns.push(
                {
                    id: 'Current',
                    name: 'Current',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];
            igrid.AddCustomColumn('edit', '<input type="checkbox" style="z-index: auto;margin-left:5px">', 40, function (item, col) {
                if (item.edit)
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px" checked>';
                else
                    return '<input type="checkbox" style="z-index: auto;margin-left:10px">';
            });

            if ($scope.IsDealerPrice) {
                igrid.AddColumn(
                    {
                        id: "npdealerprice", name: "WS Price %", field: "npdealerprice",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                igrid.AddColumn(
                    {
                        id: "ndealerprice", name: "WS Price", field: "ndealerprice",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
            }
            else {
                igrid.AddColumn(
                    {
                        id: "nprice", name: "Price", field: "nprice",
                        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );

                igrid.AddColumn(
                    {
                        id: "ndiscount", name: "Discount", field: "ndiscount",
                        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );

                igrid.AddColumn(
                    {
                        id: "npdiscount", name: "Discount %", field: "npdiscount",
                        width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );

                igrid.AddColumn(
                    {
                        id: "newsaleprice", name: "New Price", field: "newsaleprice",
                        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );

                var column = igrid.AddCustomColumn('finalprice', 'Final', 70, function (item, col) {
                    return $filter('IndianNumber')(item.nprice - item.ndiscount);
                });
                column.cssClass = 'rightalign';

            }


            igrid.AddColumn(
                {
                    id: "printcopy", name: "Print", field: "printcopy",
                    width: 50, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );

            groupColumns.push(
                {
                    id: 'New',
                    name: 'New',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            igrid.columns = groupColumns;
            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onHeaderClick.subscribe(function (e, args) {

                if (args.column.id == 'edit' && $(e.target).is(":checkbox")) {
                    if (igrid.grid.getEditorLock().isActive() && !igrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }

                    if ($(e.target).is(":checked")) {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.edit = true;
                            item.oprice = item.price;
                            item.nprice = item.price;
                            item.ndiscount = item.discount;
                            item.npdiscount = item.pdiscount;
                            item.ndealerprice = item.dealerprice;
                            item.npdealerprice = item.pdealerprice;
                        }

                    } else {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            item.edit = false;
                            item.nprice = 0;
                            item.ndiscount = 0;
                            item.npdiscount = 0;
                            item.ndealerprice = 0;
                            item.npdealerprice = 0;
                            item.newsaleprice = 0;
                        }
                    }
                    igrid.grid.invalidate();
                    igrid.grid.render();
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                }

            });

            igrid.grid.onClick.subscribe(function (e, args) {

                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Remove') {
                    var rows = $scope.igrid.grid.getData().getItems();
                    rows.splice(rows.indexOf(item), 1);
                    $scope.igrid.referesh();
                }

                if (args.grid.getColumns()[args.cell].id == 'edit') {
                    item = args.grid.getDataItem(args.row);
                    item.edit = !item.edit && $(e.target).val() == 'on';

                    if (item.edit) {
                        item.oprice = item.price;
                        item.nprice = item.price;
                        item.ndiscount = item.discount;
                        item.npdiscount = item.pdiscount;
                        item.ndealerprice = item.dealerprice;
                        item.npdealerprice = item.pdealerprice;
                    }
                    else {
                        item.nprice = 0;
                        item.ndiscount = 0;
                        item.npdiscount = 0;
                        item.ndealerprice = 0;
                        item.npdealerprice = 0;
                    }

                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);

                }
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'nprice') {
                    $scope.compute(item, 'price');
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'ndiscount') {
                    $scope.compute(item, 'discount');
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'npdiscount') {
                    $scope.compute(item, 'discountpercent');
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'npdealerprice') {
                    $scope.compute(item, 'pdealerprice');
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'ndealerprice') {
                    $scope.compute(item, 'dealerprice');
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id == 'newsaleprice') {
                    $scope.compute(item, 'newsaleprice');
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });
            igrid.UpdateData(result);
            $scope.igrid = igrid;
            $scope.setFocusfirst();
        }

        $scope.Apply = function (action) {

            var npIncrease = $rootScope.GetNumber($scope.allitem.npIncrease);
            var npDecrease = $rootScope.GetNumber($scope.allitem.npDecrease);
            var nprice = $rootScope.GetNumber($scope.allitem.nprice);
            var ndiscount = $rootScope.GetNumber($scope.allitem.ndiscount);
            var npdiscount = $rootScope.GetNumber($scope.allitem.npdiscount);
            var ndealerprice = $rootScope.GetNumber($scope.allitem.ndealerprice);

            $.each($scope.ItemList, function (k, v) {
                if (typeof (v.edit) != 'undefined' && v.edit) {
                    if (action == "1") {
                        if ($scope.allitem.nprice.indexOf('+') == 0) {
                            v.nprice = v.price + nprice;
                            $scope.compute(v, 'price');
                        }
                        else if ($scope.allitem.nprice.indexOf('-') == 0) {
                            v.nprice = v.price - nprice;
                            $scope.compute(v, 'price');
                        }
                        else if ($scope.allitem.nprice.indexOf('%') > 0) {
                            v.nprice = v.price + (v.price * nprice / 100);
                            $scope.compute(v, 'price');
                        }
                        else {
                            v.nprice = $scope.allitem.nprice;
                            $scope.compute(v, 'price');
                        }
                    } else if (action == "2") {
                        v.ndiscount = $scope.allitem.ndiscount;
                        $scope.compute(v, 'discount');
                    }
                    else if (action == "3") {
                        v.npdiscount = $scope.allitem.npdiscount;
                        $scope.compute(v, 'discountpercent');
                    }
                    else if (action == "4") {
                        v.nprofit = $scope.allitem.nprofit;
                        $scope.compute(v, 'profit');
                    } else if (action == "5") {
                        v.npdealerprice = parseFloat($scope.allitem.npdealerprice);
                        if (isNaN(v.npdealerprice)) v.npdealerprice = 0;
                        $scope.compute(v, 'pdealerprice');
                    } else if (action == "6") {
                        v.ndealerprice = $scope.allitem.ndealerprice;
                        $scope.compute(v, 'dealerprice');
                    } else if (action == "7") {
                        if ($scope.IsDealerPrice) {
                            v.ndealerprice = $scope.RoundPrice(v.dealerprice / (100 + v.taxpercentage) * (100 + npIncrease + v.taxpercentage));
                            $scope.compute(v, 'dealerprice');
                        }
                        else {
                            v.nprice = $scope.RoundPrice(v.price / (100 + v.taxpercentage) * (100 + npIncrease + v.taxpercentage));
                            $scope.compute(v, 'price');
                        }
                    } else if (action == "8") {
                        if ($scope.IsDealerPrice) {
                            v.ndealerprice = $scope.RoundPrice(v.dealerprice / (100 + v.taxpercentage) * (100 - npDecrease + v.taxpercentage));
                            $scope.compute(v, 'dealerprice');
                        }
                        else {
                            v.nprice = $scope.RoundPrice(v.price / (100 + v.taxpercentage) * (100 - npDecrease + v.taxpercentage));
                            $scope.compute(v, 'price');
                        }
                    }
                }
            });
            $scope.igrid.grid.invalidate();
            $scope.igrid.grid.render();
        };

        $scope.ApplyFinalValue = function (action) {

            var nprice = $rootScope.GetNumber($scope.allitem.nprice);
            var ndiscount = $rootScope.GetNumber($scope.allitem.ndiscount);
            var npdiscount = $rootScope.GetNumber($scope.allitem.npdiscount);
            var ndealerprice = $rootScope.GetNumber($scope.allitem.ndealerprice);
            var finalprice = $rootScope.GetNumber($scope.allitem.finalprice);

            $.each($scope.ItemList, function (k, v) {
                if (typeof (v.edit) != 'undefined' && v.edit) {
                    if (v.nprice > 0)
                        v.ndiscount = (v.nprice - finalprice);
                    else
                        v.ndiscount = (v.price - finalprice);
                    $scope.compute(v, 'discount');
                }
            });
            $scope.igrid.grid.invalidate();
            $scope.igrid.grid.render();
        };

        $scope.SaveConformation = function () {
            $.confirm({
                title: 'SAVE',
                content: 'Do you want to change the item price/discount? The changes will be applied immediately!!',
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
                            $scope.Save();
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
        $scope.Save = function () {
            $scope.Price.iswarehouse = $scope.Price.mode == 1;
            if ($scope.Price.mode == 2) {
                if (($scope.Price.locationid == 0 || $scope.Price.locationid == 'undefined')) {
                    $rootScope.Toaster.Info("Please select location");
                    return;
                }
                $scope.Price.iswarehouse = false;
            }
            else if ($scope.Price.mode == 3) {
                if ($scope.Price.pricemode == 2)
                    $scope.Price.iswarehouse = true;
                else
                    $scope.Price.iswarehouse = ($scope.Price.locationid == null || $scope.Price.locationid == 0);
            }
            else {
                $scope.Price.iswarehouse = $scope.Price.mode == 1;
            }
            $scope.Price.itemlist = [];

            $.each($scope.ItemList, function (k, v) {
                if (typeof (v.edit) != 'undefined' && v.edit) {
                    $scope.Price.itemlist.push({
                        id: v.id,
                        printingname: v.printingname,
                        salablegoodsid: $scope.Price.iswarehouse ? 0 : v.salablegoodsid,
                        stockid: $scope.Price.iswarehouse ? v.stockid : 0,
                        locationid: $scope.Price.locationid,
                        barcode: v.barcode,
                        price: v.price,
                        discount: v.discount,
                        nprice: v.nprice,
                        ndiscount: v.ndiscount,
                        ndealerprice: v.ndealerprice
                    });
                }
            });

            $scope.Price.changedealerprice = $scope.IsDealerPrice;

            var request = {
                "Item": $scope.Price,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PricechangeService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Saved Successfully.!");
                    //$scope.Price = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $rootScope.Toaster.Info("Error - " + result.ErrorMessage);
                }
            }).error(function (result) {
            });
        }
        $scope.Clear = function () {
            $scope.ItemList = [];
            $scope.Price = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.allitem = {};
            $scope.ConfigGrid($scope.ItemList);
        }
        $scope.ManageSearchItem = function (action, item) {
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
                                    url: "PricechangeService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
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
                $scope.ShowSearch = false;
                $scope.Price = item;
                $scope.ItemList = [];
                $scope.SearchItems(item.id);
            }
        }

        $scope.fileuploadedcomplete = function (value, directbarcode) {
            $scope.uploadfile = null;
            var condition = {
                mode: $scope.Price.mode,
                barcodes: [],
                dosort: true,
                locationid: $scope.Price.locationid
            };

            var priceList = [];
            if (value != null) {
                if (value.rows[0].values.length > 1) {
                    var priceindex = -1;
                    var barcodeindex = -1;
                    var discountindex = -1;
                    var qtyindex = -1;
                    $.each(value.rows[0].values, function (k, v) {
                        if (v.toLowerCase() == 'barcode')
                            barcodeindex = k;
                        if (v.toLowerCase() == 'price')
                            priceindex = k;
                        if (v.toLowerCase() == 'discount')
                            discountindex = k;
                        if (v.toLowerCase() == 'print' || v.toLowerCase() == 'qty')
                            qtyindex = k;
                    });
                    if (barcodeindex >= 0) {
                        $.each(value.rows, function (k, v) {
                            var vitem = { 'barcode': v.values[barcodeindex] };
                            if (priceindex >= 0)
                                vitem.price = v.values[priceindex];
                            if (discountindex >= 0)
                                vitem.discount = v.values[discountindex];
                            if (qtyindex >= 0)
                                vitem.printcopy = v.values[qtyindex];

                            priceList.push(vitem);
                        });
                    }
                }

                $.each(value.rows, function (k, v) {
                    var barcode = v.values[0];
                    if (barcode != null && barcode != '' && barcode.length > 0) {
                        condition.barcodes.push(barcode)
                    }
                });
            }
            else {
                condition.barcodes.push(directbarcode);
            }

            if (condition.barcodes.length > 0) {
                $scope.SearchStock(condition, function (result, condition) {
                    if (typeof ($scope.ItemList) == 'undefined' || $scope.ItemList == null)
                        $scope.ItemList = [];
                    $.each(result, function (k, v) {
                        var stock = $scope.ItemList.find(function (item) {
                            return item.barcode == v.barcode;
                        });
                        if (typeof (stock) == 'undefinied' || stock == null) {

                            v.pdealerprice = 0;
                            if ($scope.IsDealerPrice && v.dealerprice > 0) {
                                v.pdealerprice = (v.dealerprice / v.ibuyingprice * 100 - 100).round(0);
                            }
                            if (v.discount > 0)
                                v.pdiscount = (v.discount / v.price * 100).round(0);
                            else
                                v.pdiscount = 0;
                            v.stockqty = v.stock;

                            var price = priceList.find(function (pitem) {
                                return pitem.barcode == v.barcode;
                            });
                            if (price && price != null) {
                                v.edit = true;
                                if (price.price)
                                    v.nprice = price.price;
                                if (price.discount)
                                    v.ndiscount = price.discount;
                                if (price.printcopy)
                                    v.printcopy = price.printcopy;
                            }

                            $scope.ItemList.push(v);

                            //if ($scope.Price.mode == 1) {
                            //$scope.ItemList.push({
                            //    'printingname': v.itemname, 'sellingmode': v.sellingmode,
                            //    'barcode': v.barcode, 'oprice': v.price, 'price': v.price, 'discount': v.discount,
                            //    'buyingprice': v.buyingprice, 'ibuyingprice': v.ibuyingprice, 'marginpercentage': v.marginpercentage,
                            //    'nprice': 0, 'odiscount': v.discount, 'pdiscount': (v.discount / v.price * 100).round(0),
                            //    'ndiscount': 0, 'stockqty': v.qty, 'mrp': v.mrp, 'serialno': v.serialno,
                            //    'stockid': v.stockid, 'edit': false, 'npdiscount': 0, 'dealerprice': v.dealerprice,
                            //    'pdealerprice': v.pdealerprice
                            //});
                            //}
                            //else {
                            //$scope.ItemList.push({
                            //    'printingname': v.itemname, 'sellingmode': v.sellingmode, 'ibuyingprice': v.ibuyingprice,
                            //    'barcode': v.barcode, 'oprice': v.price, 'price': v.price, 'discount': v.discount, 'buyingprice': v.buyingprice, 'marginpercentage': v.marginpercentage,
                            //    'nprice': 0, 'odiscount': v.discount, 'pdiscount': (v.discount / v.price * 100).round(0),
                            //    'ndiscount': 0, 'stockqty': v.qty, 'mrp': v.mrp, 'serialno': v.serialno,
                            //    'salablegoodsid': v.salablegoodsid, 'edit': false, 'npdiscount': 0, 'dealerprice': v.dealerprice,
                            //    'pdealerprice': v.pdealerprice
                            //});
                            //}
                        }
                        else {
                            var price = priceList.find(function (pitem) {
                                return pitem.barcode == v.barcode;
                            });
                            if (price && price != null) {
                                v.nprice = price.price;
                                v.ndiscount = price.discount;
                            }
                        }
                    });
                    $scope.UpdatePrintCount(condition.barcodes, value == null);
                    $scope.ConfigGrid($scope.ItemList);
                });
            }
        }

        $scope.UpdatePrintCount = function (barcodes, singlebarcode) {
            $.each(barcodes, function (k, barcode) {
                var existing = $scope.ItemList.find(function (item) {
                    return item.barcode == barcode;
                });
                if (existing && existing != null) {
                    if (singlebarcode) {
                        var printcopy = parseInt(existing.printcopy);
                        if (isNaN(printcopy))
                            printcopy = existing.stockqty;
                        else
                            printcopy++;
                        if (existing.stockqty < printcopy) {
                            $rootScope.Toaster.Info(barcode + " - Stock lesser than the scanned qty");
                        }
                        else {
                            existing.printcopy = printcopy;
                        }
                    }
                    else {
                        var printcopy = parseInt(existing.printcopy);
                        if ($rootScope.GetNumber(printcopy) == 0)
                            existing.printcopy = existing.stockqty;
                    }
                }
            });
        }

        $scope.SearchStock = function (condition, callback) {

            if (UserDetails.Info.p_SHOWVALUE.view) {
                $rootScope.Toaster.Info("You don't have permission to view the cost");
                return;
            }


            if (UserDetails.Info.p_SHOWSELLINGVALUE.view) {
                $rootScope.Toaster.Info("You don't have permission to view the selling price");
                return;
            }

            if (typeof (condition) == 'undefined')
                condition = $scope.SearchCondition;

            var serviceurl = "PricechangeService/SearchItems"
            condition.iswarehouse = true;
            if (condition.mode == 2) {
                if ((condition.locationid == 0 || condition.locationid == 'undefined')) {
                    $rootScope.Toaster.Info("Please select location");
                    return;
                }
                condition.iswarehouse = false;
            }
            else if (condition.mode == 3) {
                if (condition.pricemode == 2)
                    condition.iswarehouse = false;
                else
                    condition.iswarehouse = (condition.locationid == null || condition.locationid == 0);
            }
            else {
                condition.iswarehouse = condition.mode == 1;
            }

            //var serviceurl = "WarehouseReportService/SearchStock";
            //if (condition.mode == 2) {
            //    if ((condition.locationid == 0 || condition.locationid == 'undefined')) {
            //        $rootScope.Toaster.Info("Please select location");
            //        return;
            //    }
            //    serviceurl = "RetailReportService/SearchStock";
            //}
            //if (condition.mode == 3) {
            //    if (!(condition.locationid == 0 || condition.locationid == 'undefined')) {
            //        serviceurl = "RetailReportService/SearchStock";    
            //    }
            //}
            request = {
                "item": condition,
                "securitytoken": "String content"
            }

            $scope.IsDealerPrice = $scope.SupportDealerPrice &&
                ($scope.Price.mode == 1 || $scope.Price.mode == 3) &&
                $scope.Price.pricemode == 2;

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {
                if (typeof (callback) != 'undefined') {
                    callback(result, condition);
                }
                else {
                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                    }
                }
            }).error(function (result) {
            });
        }

        $scope.PrintBarcode = function () {
            var ids = [];
            var _selectindex = 1;
            $.each($scope.ItemList, function (k, v) {
                if (v.edit) {
                    if (typeof (v.printcopy) == 'undefined' || v.printcopy == null || v.printcopy == 0) {
                        ids.push({
                            'barcode': v.barcode,
                            'noofcopy': 1,
                            'selectindex': _selectindex++
                        });
                    }
                    else {
                        ids.push({
                            'barcode': v.barcode,
                            'noofcopy': v.printcopy,
                            'selectindex': _selectindex++
                        });
                    }

                    //    if (typeof (v.sellingmode) == 'undefined' || v.sellingmode == null || v.sellingmode == 3) {
                    //        if (typeof (v.printcopy) == 'undefined' || v.printcopy == null || v.printcopy == 0) {
                    //            ids.push({
                    //                'barcode': v.barcode,
                    //                'noofcopy': 1
                    //            });
                    //        }
                    //        else {
                    //            ids.push({
                    //                'barcode': v.barcode,
                    //                'noofcopy': v.printcopy
                    //            });
                    //        }
                    //    }
                    //    else {
                    //        if (typeof (v.printcopy) == 'undefined' || v.printcopy == null || v.printcopy == 0) {
                    //            ids.push({
                    //                'barcode': v.barcode,
                    //                'noofcopy': v.stockqty
                    //            });
                    //        }
                    //        else {
                    //            ids.push({
                    //                'barcode': v.barcode,
                    //                'noofcopy': v.printcopy
                    //            });
                    //        }
                    //    }
                }
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Info("Please select barcode to be print.");
            }
            $scope.ChoseReport({ 'iswarehouse': ($scope.Price.mode == 1), 'locationid': $scope.Price.locationid, 'barcodes': ids, 'reportid': 0 });
        };

        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PricechangeService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.RoundPrice = function (amount) {

            if (!$scope.SupportRounding) return Math.round(amount, 0).toFixed(0);

            if (amount < 100) {
                amount = Math.round(amount, 0);
            }

            if (amount > 100 && amount < 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    if (diff - 5 >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 10;
                    } else {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    }
                } else {
                    if (diff >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    } else {
                        amount = (Math.floor(amount / 10) * 10);
                    }
                }

            } else if (amount > 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    amount = (Math.floor(amount / 10) * 10) + 10;
                } else {
                    amount = (Math.floor(amount / 10) * 10);
                }
            }
            return amount;
        }

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            $.each(printdocument, function (k, v) {

                $rootScope.Sent2Printer(v, printerData);
            })
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'PRICE CHANGER - EXPORT'
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