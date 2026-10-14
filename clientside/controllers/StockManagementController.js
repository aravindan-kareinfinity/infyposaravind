var app = angular.module('InfinityLab');
app.controller('StockManagementController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', '$filter', 'restService', 'ngDialog', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, $filter, restService, ngDialog, $timeout) {
        $scope.SearchRef = restService.SearchReference;
        $scope.status = '';

        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });

        $scope.Type = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.Type = results;
        });

        $scope.Color = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Color = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });


        $scope.Location = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
            var exist = false;
            $.each($scope.Sizes, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.Sizes.push({ id: -2, name: 'Jump' });
                $scope.Sizes.push({ id: -3, name: 'Cut' });
            }
        });
        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Pattern = results;
        });
        $scope.Style = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Style = results;
        });

        $scope.Modes = [{ 'id': '1', 'name': 'Warehouse' },
        { 'id': '2', 'name': 'Retail' },
        { 'id': '3', 'name': 'Stock Marker' }];

        $scope.ReportType = [{ 'code': 'ADJUSTMENT', 'name': 'Stock Adjustment' },
        { 'code': 'DAMAGE', 'name': 'Damaged Stock' },
        { 'code': 'OFFER', 'name': 'Offer Stock' },
        { 'code': 'STAR', 'name': 'Mark as Star' }
        ];

        $scope.MarkerType = restService.Get('ReferencelistService/FULL-MARKER', true).then(function (results) {
            $scope.MarkerType = results;
        });

        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            markerid: null,
            locationid: "",
            barcode: "",
            stockid: "",
            salablegoodsid: "",
            qty: ""
        };

        $scope.StockMarker = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.selectall = function () {
            $.each($scope.StockResult, function (k, v) {
                v.selected = $scope.isselectall;
            });
        }
        $scope.Clear = function () {
            $scope.StockResult = [];
            $scope.igrid.UpdateData($scope.StockResult, '_id');
        }
        //$scope.SearchforSync = function () {


        //    if ($scope.SearchCondition.productid == null || $scope.SearchCondition.productid == 0) {
        //        $rootScope.Toaster.Info("Please select Atleast Product Or Enter Barcode");
        //        return;
        //    }

        //    if (typeof ($scope.SearchCondition.barcode) != 'undefined' && $scope.SearchCondition.barcode != null && $scope.SearchCondition.barcode.length > 0) {
        //        $scope.SearchBarcode();
        //        return;
        //    }
        //    $scope.StockResult = [];
        //    $scope.status = 'Searching stock for synchronizing.'
        //    $scope.SearchStock({
        //        'mode': $scope.SearchCondition.mode, 'locationid': $scope.SearchCondition.locationid, 'productid': $scope.SearchCondition.productid, 'brandid': $scope.SearchCondition.brandid, 'patternid': $scope.SearchCondition.patternid,
        //        'sizeid': $scope.SearchCondition.sizeid, 'styleid': $scope.SearchCondition.styleid, 'materialid': $scope.SearchCondition.materialid, 'supplierid': $scope.SearchCondition.supplierid, 'typeid': $scope.SearchCondition.typeid,
        //        'markerid': $scope.SearchCondition.markerid
        //    }, function (result) {
        //        if (typeof ($scope.StockResult) == 'undefined' || $scope.StockResult == null) {
        //            $scope.StockResult = result;
        //        }
        //        else {
        //            $.each(result, function (k, v) {
        //                $scope.StockResult.push(v);
        //            })
        //        }
        //        $scope.status = 'Please upload excel file to sync';
        //        $scope.AvailableStock = 0;
        //        $scope.SyncStock = 0;
        //        $scope.UploadedStock = 0;
        //        $.each($scope.StockResult, function (k, v) {
        //            $scope.AvailableStock += v.qty;
        //        })
        //        $scope.igrid.UpdateData($scope.StockResult);
        //    });
        //}

        $scope.SearchBarcode = function () {
            if ($scope.SearchCondition.barcode == 'undefined' || $scope.SearchCondition.barcode == null ||
                $scope.SearchCondition.barcode == '') {
                $rootScope.Toaster.Info("Please enter barcode");
                return;
            }
            $scope.SearchStock({
                'mode': $scope.SearchCondition.mode,
                'locationid': $scope.SearchCondition.locationid,
                'barcode': $scope.SearchCondition.barcode
            }, function (result) {
                if (typeof ($scope.StockResult) == 'undefined' || $scope.StockResult == null) {
                    $.each(result, function (k, v) {
                        v.stockqty = v.qty;
                        if (typeof ($scope.SearchCondition.barcode) != 'undefined' &&
                            $scope.SearchCondition.barcode != null &&
                            $scope.SearchCondition.barcode != '') {
                            v.qty = 1;
                        }
                    })
                    $scope.StockResult = result;
                }
                else {
                    $.each(result, function (k, v) {
                        var eitem = $scope.StockResult.find(function (item) {
                            return item.barcode == v.barcode;
                        });

                        if (typeof (eitem) != 'undefined' && eitem != null) {
                            if (typeof ($scope.SearchCondition.barcode) != 'undefined' &&
                                $scope.SearchCondition.barcode != null &&
                                $scope.SearchCondition.barcode != '') {
                                eitem.qty++;
                            }
                            else {
                                eitem.qty = v.qty;
                                v.stockqty = v.qty;
                            }
                        }
                        else {
                            v.stockqty = v.qty;
                            if (typeof ($scope.SearchCondition.barcode) != 'undefined' &&
                                $scope.SearchCondition.barcode != null &&
                                $scope.SearchCondition.barcode != '') {
                                v.qty = 1;
                            }
                            $scope.StockResult.push(v);
                        }
                    });
                }
                $scope.ConfigGrid(false);
                $scope.igrid.UpdateData($scope.StockResult, '_id');
                $scope.SearchCondition.barcode = '';
            });
        }

        $scope.SearchCondition = {
            mode: 1,
            fromdate: null,
            todate: null,
            locationid: 0,
            supplierid: 0,
            productid: 0,
            brandid: 0,
            materialid: 0,
            patternid: 0,
            styleid: 0,
            sizeid: 0,
            designid: "",
            searchmode: '2',
            typeid: "",
            markerid: ""
        };
        $scope.StockResult = [];
        $scope.AvailableStock = 0;
        $scope.UploadedStock = 0;
        $scope.SyncStock = 0;
        $scope.MissingBarcode4Sync = 0;
        $scope.markerid = 0;
        $scope.ShowChangeDialog = function () {
            var seletedRows = $scope.igrid.GetSelectedRows();
            if (seletedRows == 'undefined' || seletedRows == null || seletedRows.length == 0) {
                $rootScope.Toaster.Info("Please select the items to be added/removed.");
                return;
            }
            $scope.ExclusiveBarcode = $scope.analyser_uniquemarker;
            var dialog = ngDialog.open({
                template: 'ChangeAttributeDialog',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.AddFromList = function () {
                        $scope.closeThisDialog('Add');
                    };
                    $scope.RemoveFromList = function () {
                        $scope.closeThisDialog('Delete');
                    };
                    $scope.DeleteAll = function () {
                        $scope.closeThisDialog('DeleteAll');
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var request = { "items": [], "delete": data.value == "Delete", "deleteall": data.value == "DeleteAll" };

                        var seletedRows = $scope.igrid.GetSelectedRows();

                        $.each(seletedRows, function (k, v) {
                            request.items.push({
                                'isactive': true,
                                'isdamaged': v.isdamaged,
                                'stockid': v.stockid,
                                'qty': v.qty,
                                'id':v.id,
                                'stockqty': v.stockqty,
                                'locationid': v.locationid,
                                'salablegoodsid': v.salablegoodsid,
                                'barcode': v.barcode,
                                'glncode': v.glncode
                            });
                        });

                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        if ($scope.markerid == null || $scope.markerid == 'undefined' || $scope.markerid == 0) {
                            $rootScope.Toaster.Info("Please select the marker name");
                            return;
                        }

                        var marker = $scope.MarkerType.find(function (item) {
                            return item.id == $scope.markerid;
                        });

                        request.markerid = $scope.markerid;
                        request.isdamaged = marker.additionalinfo.disabled;
                        request.isonline = marker.additionalinfo.isonline;
                        request.isexclusive = $scope.ExclusiveBarcode;
                        //$.each(request, function (k, v) {
                        //    request.markerid = $scope.markerid;
                        //})
                        
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "StockmarkerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Saved Successfully.");
                        }).error(function (result) {
                        });

                    }

                }
            });
        }
        $scope.SearchStock = function (condition, callback) {
            if (typeof (condition) == 'undefined')
                condition = $scope.SearchCondition;
            $scope.totalqty = 0;
            $scope.amount = 0;

            if (condition.mode == 3) {
                $rootScope.Toaster.Info("Please select Marker name and Use Go option.");
                return;
            }

            var serviceurl = "WarehouseReportService/SearchStock";
            if (condition.mode == 2) {
                if ((condition.locationid == 0 || condition.locationid == 'undefined')) {
                    $rootScope.Toaster.Info("Please select location");
                    return;
                }
                serviceurl = "RetailReportService/SearchStock";
            }

            condition.removezerostock = true;
            request = {
                "item": condition,
                "securitytoken": "String content"
            }

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
                    $scope.StockResult = result;

                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                        return;
                    }

                    $.each(result, function (k, v) {
                        v.stockqty = v.qty;
                    })
                    $scope.ConfigGrid(false);
                    $scope.igrid.UpdateData($scope.StockResult, '_id');
                }
            }).error(function (result) {
            });
        }

        $scope.exporttoExcel = function (infygrid, filename) {
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, filename);
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
        $scope.RemoveList = [];

        $scope.fileuploadedcomplete = function (value) {

            var condition = {
                mode: $scope.SearchCondition.mode,
                barcodes: [],
                locationid: $scope.SearchCondition.locationid
            };

            var uploadedstock = [];

            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    var stock = $scope.StockResult.find(function (item) {
                        return item.barcode == barcode;
                    });

                    var qty = 1;
                    if (v.values.length > 1) {
                        qty = parseFloat(v.values[1]);
                        if (typeof (qty) == 'undefined' || qty <= 0 || isNaN(qty))
                            qty = 1;
                    }

                    var found = uploadedstock.find(function (item) {
                        return item.barcode == barcode;
                    });
                    if (typeof (found) == 'undefined' || found == null) {
                        uploadedstock.push({ 'barcode': barcode, 'qty': qty });
                    }
                    else
                        found.qty = found.qty + qty;

                    if (typeof (stock) != 'undefined' && stock != null) {
                        stock.stock = stock.stock + qty;
                        $scope.SyncStock = $scope.SyncStock + qty;
                    }
                    else {
                        condition.barcodes.push(barcode)
                    }
                }
            });
            if (condition.barcodes.length > 0) {
                $scope.SearchStock(condition, function (result) {
                    if (typeof ($scope.StockResult) == 'undefined' || $scope.StockResult == null) {
                        $scope.StockResult = result;
                    }
                    else {
                        $.each(result, function (k, v) {
                            v.stockqty = v.qty;
                            var eitem = $scope.StockResult.find(function (item) {
                                return item.barcode == v.barcode;
                            });

                            v.qty = 0;
                            if (typeof (eitem) != 'undefined' && eitem != null) {
                                $scope.StockResult.splice($scope.StockResult.indexOf(eitem), 1);
                                v.qty = eitem.qty;
                            }

                            if (typeof (v.qty) == 'undefined' || v.qty == 'undefined' || v.qty == null || v.qty == 0)
                                v.qty += v.qty;

                            $scope.StockResult.push(v);
                        });
                    }
                    var mismatch = false;
                    $.each(uploadedstock, function (k, v) {
                        var eitem = $scope.StockResult.find(function (item) {
                            return item.barcode == v.barcode;
                        });
                        if (typeof (eitem) != 'undefined' && eitem != null) {
                            if (eitem.stockqty < v.qty) mismatch = true;
                            eitem.qty = v.qty;
                        }
                    });
                    if (mismatch)
                        $rootScope.Toaster.Error("Marked qty is more than the current stock qty in few of the items.");
                    $scope.igrid.UpdateData($scope.StockResult, '_id');
                    $scope.SearchCondition.barcode = '';
                });
            }
            $scope.uploadfile = null;
        }


        $scope.SaveOnline = function () {
            var seletedRows = $scope.igrid.GetSelectedRows();
            var lst = [];
            $.each(seletedRows, function (k, v) {
                lst.push({ 'id': v.id, 'salablegoodsid': v.salablegoodsid, 'olqty': v.olqty });
            });
            var webrequest = {
                "items": lst,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(webrequest);
            $http({
                method: 'POST',
                url: "StockmarkerService/SaveOLQTY",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Saved Successfully.");
            }).error(function (result) {
            });
        }


        $scope.RemoveItem = function (item) {
            $scope.StockResult.splice($scope.StockResult.indexOf(item), 1);
            item.isactive = false;
            $scope.RemoveList.push(item);
            $scope.igrid.UpdateData($scope.StockResult, '_id');
        }

        $scope.igrid = null;
        $scope.ConfigGrid = function (existinglist) {

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '430px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            var column = igrid.AddCustomColumn('barcode', 'Barcode', 150, function (item, col) {
                if ($scope.ismfgapplication && item.glncode &&
                    item.glncode != null && item.glncode != '') {
                    return item.glncode;
                }
                return item.barcode;
            });
            igrid.AddSimpleColumn('suppliername', 'Supplier', 120);
            igrid.AddSimpleColumn('productname', 'Product', 120);
            igrid.AddSimpleColumn('brandname', 'Brand', 100);
            igrid.AddSimpleColumn('sizename', 'Size', 80);

            if (existinglist) {
                if ($scope.IsOnline) {
                    var column = igrid.AddColumn(
                        {
                            id: "olqty", name: "Online", field: "olqty",
                            width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true
                        }
                    );
                    column.cssClass = 'rightalign';
                    column.total = true;
                }
                igrid.AddFormatedColumn('qty', 'Marked Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('stockqtyatmark', 'Stock At Mark', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('stockqty', 'Stock Current', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('addedon', 'Added On', 80, 'dd-MM-yyyy');
            }
            else {
                igrid.AddColumn(
                    {
                        id: "qty", name: "Marked Qt", field: "qty",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                igrid.AddFormatedColumn('stockqty', 'Stock Current', 80, 'N2', 2).total = true;
            }
            igrid.AddFormatedColumn('price', 'Sale Price', 80, 'N2', 2)
            igrid.AddFormatedColumn('discount', 'Sale Discount', 80, 'N2', 2)
            igrid.AddSimpleColumn('materialname', 'Material', 100);
            igrid.AddSimpleColumn('patternname', 'Pattern', 100);
            igrid.AddSimpleColumn('stylename', 'Style', 100);
            igrid.AddSimpleColumn('colourname', 'Colour', 100);
            igrid.AddSimpleColumn('typename', 'Type', 100);
            igrid.AddSimpleColumn('hsncode', 'HSN Code', 100);
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                if ($scope.SearchCondition.mode !=3 && item.stockqty < item.qty) {
                    return { cssClasses: 'igriddebitnoterow' };
                }
            };

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'qty') {
                    $timeout(function () {

                        if (parseFloat(item.stockqty) < item.qty) {
                            item.qty = 0;
                            $rootScope.Toaster.Info("Can't mark more than " + item.qty);
                        }
                        else {
                            var seletedRows = args.grid.getSelectedRows();
                            seletedRows.push(args.row);
                            args.grid.setSelectedRows(seletedRows);
                        }
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            $scope.igrid = igrid;
        }

        $scope.SearchResult = [];
        $scope.ConfigGrid(false);
        $scope.Search = function () {
            $scope.SearchStockMark({ 'markerid': $scope.SearchCondition.markerid, 'mode': $scope.SearchCondition.mode, 'locationid': $scope.SearchCondition.locationid, }, function (result) {
                $scope.StockResult = result;
                $scope.ConfigGrid(true);
                $scope.igrid.UpdateData($scope.StockResult, '_id');
            });
        }

        $scope.IsOnline = false;
        $scope.SearchStockMark = function (condition, callback) {

            var marker = $scope.MarkerType.find(function (item) {
                return item.id == $scope.SearchCondition.markerid;
            });

            $scope.IsOnline = marker.additionalinfo &&
                marker.additionalinfo.isonline;

            if (typeof (condition) == 'undefined')
                condition = $scope.SearchCondition;

            var serviceurl = "StockmarkerService/Search";

            request = {
                "item": condition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: serviceurl,
                data: srequest
            }).success(function (result) {

                $scope.StockResult = result;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                else {
                    callback($scope.StockResult);
                }
            }).error(function (result) {
            });
        }


        $scope.LoadDefault = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.LoadDefault();
                }, 50);
                return;
            }
            $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
            $scope.analyser_uniquemarker = $rootScope.UserInfo.appsettings.analyser_uniquemarker;
        }
        $scope.LoadDefault();

    }]);

