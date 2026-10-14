
var app = angular.module('InfinityLab');
app.controller('ProductGalleryController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window',
    'restService', 'ngDialog', '$state', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.showproduct = false;
        $scope.showsupplier = true;
        $scope.system_ismfgapplication = false;

        $scope.LoadAppsettings = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null ||
                typeof ($rootScope.UserInfo.appsettings.system_ismfgapplication) == 'undefined') {
                $timeout(function () {
                    $scope.LoadAppsettings();
                }, 10);
                return;
            }
            $scope.system_ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
            $scope.showsupplier = $scope.system_ismfgapplication == false;
        }

        
        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });

        $scope.StockMarkerList = restService.Get('ReferencelistService/MARKER', true).then(function (results) {
            if (results.length > 0) {
                $timeout(function () {
                    $scope.ProductGallery = { 'markerid': results[0].id };
                }, 20);
            }
            $scope.StockMarkerList = results;
        });

        $scope.ShowDesign = function (designid) {

            if ($scope.ProductGallery.ProductID == undefined ||
                $scope.ProductGallery.ProductID == null || $scope.ProductGallery.ProductID < 0)
                $scope.showproduct = true;
            else
                $scope.showproduct = false;
            var srequest = JSON.stringify({
                'item':
                {
                    'productid': $scope.ProductGallery.ProductID,
                    'supplierid': $scope.ProductGallery.Supplierid,
                    'designid': designid
                }
            });
            $http({
                method: 'POST',
                url: "RetailReportService/SearchDesignwiseStock",
                data: srequest
            }).success(function (result) {
                $scope.CurrentDesignwiseStock = result;
                $scope.ColourList = result.stock;
                $.each($scope.ColourList, function (k, v) {
                    v.oimageid = v.imageid;
                });
                $scope.SizeList = [];
                var stockList = [];
                $.each(result.stock, function (k, v) {
                    $.each(v.stock, function (kk, vv) {
                        var exist = stockList.find(function (item) {
                            return item.colourid == v.colourid && item.locationid == vv.locationid;
                        });
                        if (exist == undefined || exist == null) {
                            exist = {
                                'colourid': v.colourid,
                                'colour': v.colour,
                                'locationid': vv.locationid,
                                'sizeid': vv.sizeid,
                                'size': vv.size,
                                'qty': 0,
                                'oqty':0,
                                'stocklist':[]
                            };
                            stockList.push(exist);
                        }
                        if (exist['size_AVL_' + vv.sizeid] == undefined || exist['size_AVL_' + vv.sizeid] == null)
                            exist['size_AVL_' + vv.sizeid] = 0;

                        if (exist['size_ALL_' + vv.sizeid] == undefined || exist['size_ALL_' + vv.sizeid] == null)
                            exist['size_ALL_' + vv.sizeid] = 0;

                        exist['size_AVL_' + vv.sizeid] += vv.qty;
                        exist['size_ALL_' + vv.sizeid] += vv.oqty;

                        exist.qty += vv.qty;
                        exist.oqty += vv.oqty;
                        exist.stocklist.push(vv);
                        var size = $scope.SizeList.find(function (item) {
                            return item.id == vv.sizeid;
                        });
                        if (size == undefined || size == null) {
                            $scope.SizeList.push({ 'id': vv.sizeid, 'name': vv.size });
                        }
                    })
                });
                $scope.StockList = stockList;
                $scope.ConfigGrid(stockList);
            });
        }

        $scope.Sync2Online = function () {

            if ($scope.ProductGallery.markerid == undefined ||
                $scope.ProductGallery.markerid == null) {
                $rootScope.Toaster.Error("Please select the stock marker");
                return;
            }

            var markerlist = [];
            var colourimage = [];
            var selectedstock = $scope.igrid.GetSelectedRows();
            $.each(selectedstock, function (k, v) {

                $.each($scope.SizeList, function (sk, sv) {

                    var availableqty = v['size_ALL_' + sv.id];

                    $.each(v.stocklist, function (kk, vv) {
                        if (vv.sizeid == sv.id) {

                            if (availableqty > 0) {
                                var allocatedqty = vv.qty;
                                if (allocatedqty > availableqty) {
                                    allocatedqty = availableqty;
                                    availableqty = 0;
                                }
                                else {
                                    availableqty -= allocatedqty;
                                }
                                markerlist.push({
                                    'locationid': v.locationid,
                                    'colourid': v.colourid,
                                    'barcode': vv.barcode,
                                    'qty': allocatedqty,
                                    'bstock':vv.bstock
                                });
                            }
                            else {
                                markerlist.push({
                                    'locationid': v.locationid,
                                    'colourid': v.colourid,
                                    'barcode': vv.barcode,
                                    'bstock': vv.bstock,
                                    'qty': 0
                                });
                            }
                        }
                    });

                });

            });

            $.each($scope.ColourList, function (k, v) {
                colourimage.push({
                    'colourid': v.colourid,
                    'imageid': v.imageid,
                    'oimageid': v.oimageid,
                    'imagelist': v.imagelist,
                    'removelist': v.removelist
                });
            });

            var request = {
                'markerlist': markerlist, 'imagelist': colourimage,
                'productid': $scope.ProductGallery.ProductID,
                'supplierid': $scope.ProductGallery.Supplierid,
                'designid': $scope.CurrentDesignwiseStock.designid,
                'markerid': $scope.ProductGallery.markerid
            };

            var srequest = JSON.stringify({
                'item': request
            });
            $http({
                method: 'POST',
                url: "RetailReportService/SyncOnline",
                data: srequest
            }).success(function (result) {
                if (result) {
                    $rootScope.Toaster.Info("Updated successfully");
                }
            });
        }

        $scope.AllStock2Online = function () {
            if ($scope.CurrentDesignwiseStock == undefined || $scope.CurrentDesignwiseStock == null) return;

            var result = $scope.CurrentDesignwiseStock;
            $scope.SizeList = [];
            var stockList = [];
            $.each(result.stock, function (k, v) {
                $.each(v.stock, function (kk, vv) {
                    var exist = stockList.find(function (item) {
                        return item.colourid == v.colourid && item.locationid == vv.locationid;
                    });
                    if (exist == undefined || exist == null) {
                        exist = {
                            'colourid': v.colourid,
                            'colour': v.colour,
                            'locationid': vv.locationid,
                            'qty': 0,
                            'oqty': 0,
                            'stocklist': []
                        };
                        stockList.push(exist);
                    }
                    if (exist['size_AVL_' + vv.sizeid] == undefined || exist['size_AVL_' + vv.sizeid] == null)
                        exist['size_AVL_' + vv.sizeid] = 0;

                    if (exist['size_ALL_' + vv.sizeid] == undefined || exist['size_ALL_' + vv.sizeid] == null)
                        exist['size_ALL_' + vv.sizeid] = 0;

                    exist['size_AVL_' + vv.sizeid] += vv.qty;
                    exist['size_ALL_' + vv.sizeid] += vv.qty;

                    exist.qty += vv.qty;
                    exist.oqty += vv.qty;
                    exist.stocklist.push(vv);
                    var size = $scope.SizeList.find(function (item) {
                        return item.id == vv.sizeid;
                    });
                    if (size == undefined || size == null) {
                        $scope.SizeList.push({ 'id': vv.sizeid, 'name': vv.size });
                    }
                })
            });
            $scope.StockList = stockList;
            $scope.ConfigGrid(stockList);
        }

        $scope.RemoveImage = function (item, imgitem) {
            item.imagelist.splice(item.imagelist.indexOf(imgitem), 1);
            if (imgitem.id && imgitem.id > 0) {
                if (item.removelist == undefined || item.removelist == null)
                    item.removelist = [];
                item.removelist.push({ 'id': imgitem.id });
            }
        }

        $scope.imageLineaddeded = function (value, scope) {
            if (scope.item.imagelist == undefined || scope.item.imagelist == null)
                scope.item.imagelist = [];

            if (scope.item.imageid == undefined || scope.item.imageid == null || scope.item.imageid == '') {
                scope.item.imageid = value[0];
            }
            else {
                scope.item.imagelist.push({ 'imageid': value[0] });
            }
            scope.item.aimageid = null;
        }
        
        $scope.imageLineuploaded = function (value, scope) {
            scope.item.imageid = value[0];
            scope.item.timageid = null;
        }

        $scope.SearchDesignList = function () {
            var srequest = JSON.stringify({
                'item':
                {
                    'productid': $scope.ProductGallery.ProductID,
                    'supplierid': $scope.ProductGallery.Supplierid,
                    'barcode': $scope.ProductGallery.Barcode
                }
            });
            $http({
                method: 'POST',
                url: "RetailReportService/SearchDesignList",
                data: srequest
            }).success(function (result) {
                var designlist = [];
                $.each(result, function (k, v) {
                    designlist.push({ 'name': v, 'selected': false });
                });
                $scope.DesignList = designlist;
            });
        }

        $scope.SaveImages = function () {
            var srequest = JSON.stringify({
                'item':
                {
                    'productid': $scope.ProductGallery.ProductID,
                    'designid': $scope.CurrentDesignwiseStock.designid,
                    'List': $scope.ColourList
                }
            });
            $http({
                method: 'POST',
                url: "RetailReportService/SaveStockImages",
                data: srequest
            }).success(function (result) {
                
            });
        }

        $scope.ImageList = [];
        $scope.ShowImageMap = false;
        $scope.SearchBarcode = function () {
            var srequest = JSON.stringify({
                'item':
                {
                    'barcode': $scope.ProductGallery.Barcode
                }
            });
            $http({
                method: 'POST',
                url: "RetailReportService/SearchDesignList",
                data: srequest
            }).success(function (result) {
                var designlist = [];
                $.each(result, function (k, v) {
                    designlist.push({ 'name': v, 'selected': false });
                });
                $scope.DesignList = designlist;
            });
        }
        $scope.MapImage = function (imageitem) {
            var colouritem = $scope.ColourList.find(function (item) {
                return item.colourid == imageitem.colourid;
            });
            if (colouritem)
                colouritem.imageid = imageitem.imageid;
        }
        $scope.MultipleImageUpload = function (value) {
            $scope.multipleimage = null;
            $scope.ImageList.push({ 'imageid': value[0] ,'colourid':0 });
        }
        $scope.MultipleImageStart = function () {
            $scope.ImageList = [];
            $scope.ShowImageMap = true;
        }
        $scope.BarcodeList = []
        $scope.ConfigGrid = function (stocklist) {

            $scope.ShowImageMap = false;
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridResultContainer').empty();
            }

            var igrid = new InfyGrid('gridResultContainer', '100px', '100%', false, true, true,-30);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.Locations);
            igrid.AddSimpleColumn('colour', 'Colour', 120);

            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'detail',
                    name: 'Detail',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            
            $.each($scope.SizeList, function (k, v) {
                igrid.columns = [];
                var column = igrid.AddColumn(
                    {
                        id: "size_AVL_" + v.id, name: 'STOCK', field: "size_AVL_" + v.id,
                        width: 50, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, 'enablefilter': true, 'total': true
                    }
                );

                var column = igrid.AddColumn(
                    {
                        id: "size_ALL_" + v.id, name: 'ONLINE', field: "size_ALL_" + v.id,
                        width: 50, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, 'enablefilter': true,'total':true
                    }
                );
                groupColumns.push(
                    {
                        id: v.name,
                        name: v.name,
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            });
            
            igrid.columns = groupColumns;
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(stocklist, '_id');
        }

        $scope.panelheight = { 'height': '500px' };
        $timeout(function () {
            $scope.LoadAppsettings();
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 80) + 'px';
            }
        }, 100);

    }]);
