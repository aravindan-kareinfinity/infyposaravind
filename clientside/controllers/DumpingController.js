var app = angular.module('InfinityLab');
app.controller('DumpingController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {

        $rootScope.AccordionName = $location.path();
        $scope.PriceModes = [
            { 'id': 1, 'name': 'Retail Price' },
            { 'id': 2, 'name': 'Dealer Price' }];
        $scope.SearchModes = [
            { 'id': 1, 'name': 'Warehouse' },
            { 'id': 2, 'name': 'Retail' }];
        $scope.editall = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.ItemList = [];
        $scope.SupportDealerPrice = false;
        $scope.SupportRounding = false;
        $scope.System_b2brounding = 0;
        $scope.System_pricetag = false;
        $scope.HideCostingValue = false;
        $scope.LoadingConfiguration = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadingConfiguration, 100);
            }
            else {
                $scope.SupportDealerPrice = $rootScope.UserInfo.appsettings.system_dealerprice || false;
                $scope.System_pricetag = $rootScope.UserInfo.appsettings.system_invcol_pricetag || false;
                $scope.SupportRounding = $rootScope.UserInfo.appsettings.system_supportrounding || false;
                $scope.HideCostingValue = $rootScope.UserInfo.p_SHOWVALUE && $rootScope.UserInfo.p_SHOWVALUE.view;
                if (false || $scope.UserInfo.appsettings.system_b2brounding)
                    $scope.System_b2brounding = 4;
            }
        }

        $scope.LoadingConfiguration();

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.DumpingGroups = restService.Get('ReferencelistService/DUMPINGGROUP', true).then(function (results) {
            $scope.DumpingGroups = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
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
        $scope.Dump = (JSON.parse(JSON.stringify($scope.Entity)));


        $scope.CanSave = typeof ($rootScope.UserInfo.p_PRICECHANGER.modify) == 'undefined' ? false : $rootScope.UserInfo.p_PRICECHANGER.modify;


        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Dump = (JSON.parse(JSON.stringify($scope.Entity)));
        }


        $scope.SearchByBarcode = function (barcode) {
            if (($scope.Dump.locationid == null || $scope.Dump.locationid == 0)) {
                $rootScope.Toaster.Info("Please select retail location.");
                return;
            }

            //barcode
            if ($scope.ItemList && $scope.ItemList.length > 0) {
                var existing = $scope.ItemList.find(function (item) {
                    return item.barcode == barcode;
                });
                if (existing && existing != null) {
                    var dqty = parseInt(existing.dqty);
                    if (isNaN(dqty))
                        dqty = 1;
                    else
                        dqty++;

                    if (existing.qty < dqty) {
                        $rootScope.Toaster.Error(barcode + " (" + dqty + ") - Stock lesser than the scanned qty (" + existing.stockqty + ")");
                    }
                    else {
                        existing.dqty = dqty;
                        $scope.igrid.UpdateData($scope.ItemList);
                        $scope.Dump.barcode = '';
                    }
                    return;
                }
            }
            var request = {
                "Item": { 'locationid': $scope.Dump.locationid, 'barcode': $scope.Dump.barcode }
            };
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockdumpingService/SearchItems",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("Item not found.");
                    return;
                }
                $scope.Dump.barcode = '';
                if ($scope.ItemList == undefined || $scope.ItemList == null)
                    $scope.ItemList = [];
                $.each(result, function (k, v) {
                    $scope.ItemList.push(v);
                });
                $scope.ConfigGrid($scope.ItemList);
            }).error(function (result) {
            });
        };


        $scope.SearchItems = function (id) {

            if ($scope.Dump.locationid == null || $scope.Dump.locationid == 0) {
                $rootScope.Toaster.Info("Please select retail location.");
                return;
            }
            $scope.ItemList = [];
            var request = {
                "Item": $scope.Dump,
                "securitytoken": "String content"
            }

            if (typeof (id) != 'undefined' && id > 0)
                request.id = id;

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockdumpingService/SearchItems",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("Item not found.");
                }
                $scope.ItemList = result;
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



        $scope.ConfigSearchGrid = function (results) {
            if ($scope.sgrid != null) {
                $scope.sgrid.grid.destroy();
                $('#gridSearchContainer').empty();
            }


            var igrid = new InfyGrid('gridSearchContainer', '425px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(results);
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:25px;height:25px;margin-right:5px;line-height: 0;' value='Remove' title='Remove' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 12px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:25px;height:25px;margin-right:5px;line-height: 0;' value='Edit' title='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 12px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Print', '', 35, "<button type='button' style='width:25px;height:25px;margin-right:5px;line-height: 0;' value='Print' title='Print' class='btn btn-success btn-xs'><i class='fa fa-print' style='font:normal normal normal 12px/1 FontAwesome'></i></button>");
            igrid.AddReferenceColumn('companyid', 'Company', 120, $scope.Company);
            igrid.AddReferenceColumn('locationid', 'Location', 120, $scope.Locations);
            igrid.AddFormatedColumn('dumpon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('employeeid', 'Created By', 120, $scope.Employees);

            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.UpdateData(results);
            $scope.sgrid = igrid;

            igrid.grid.onClick.subscribe(function (e, args) {

                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete') {

                    $.confirm({
                        title: 'Delete',
                        content: 'Do you want to delete this dump record?',
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
                                        url: "StockdumpingService/Delete",
                                        data: srequest
                                    }).success(function (result) {
                                        if (result) {
                                            var rows = $scope.sgrid.grid.getData().getItems();
                                            rows.splice(rows.indexOf(item), 1);
                                            $scope.sgrid.referesh();
                                        }
                                    });
                                    return true;
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
                if (command == 'Edit') {
                    $timeout(function () {
                        $scope.ShowSearch = false;
                        $scope.Edit(item);
                    }, 10);
                }
                if (command == 'Print') {
                    $timeout(function () {
                        $scope.Print(item);
                    }, 10);
                }
            });
        }

        $scope.Edit = function (item) {
            var request = {
                "Item": {'id':item.id},
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockdumpingService/Search",
                data: srequest
            }).success(function (result) {
                $scope.Dump = result[0];
                $scope.ItemList = $scope.Dump.itemlist;
                $scope.ConfigGrid($scope.Dump.itemlist);
            }).error(function (result) {
            });
        }

        $scope.ConfigGrid = function (result) {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }


            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, false, true, 50);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(result);
            igrid.AddSimpleColumn('productname', 'Detail', 150);
            igrid.AddSimpleColumn('itemcode', 'Item Code', 150);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            if (!$scope.HideCostingValue)
                igrid.AddFormatedColumn('costprice', 'Cost', 80, 'N2', 2);
            igrid.AddFormatedColumn('price', 'MRP', 80, 'N2', 2);
            igrid.AddFormatedColumn('qty', 'Stock', 80, 'N2', 2);

            igrid.AddColumn(
                {
                    id: "dqty", name: "Dump", field: "dqty",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:25px;height:25px;margin-right:5px;line-height: 0;' value='Remove' title='Remove' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 12px/1 FontAwesome'></i></button>");
            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e, args) {

                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete') {
                    var rows = $scope.igrid.grid.getData().getItems();
                    rows.splice(rows.indexOf(item), 1);
                    $scope.igrid.referesh();
                }
            });

            igrid.UpdateData(result);
            $scope.igrid = igrid;
        }

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
            if (($scope.Dump.locationid == 0 || $scope.Dump.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please select location");
                return;
            }
            $scope.Dump.itemlist = [];

            $.each($scope.ItemList, function (k, v) {
                $scope.Dump.itemlist.push(v);
            });

            var request = {
                "Item": $scope.Dump,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockdumpingService/Save",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Saved Successfully.!");
                    $scope.Dump = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.igrid.UpdateData([]);
                }
                else {
                    $rootScope.Toaster.Info("Error - " + result.ErrorMessage);
                }
            }).error(function (result) {
            });
        }

        $scope.SearchCondition = {};
        $scope.Search = function () {
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockdumpingService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ConfigSearchGrid(result);
            }).error(function (result) {
            });
        }

        $scope.Clear = function () {
            $scope.ItemList = [];
            $scope.Dump = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.allitem = {};
            $scope.ConfigGrid($scope.ItemList);
        }

        $scope.fileuploaded = function (value) {
            $scope.barcodes = null;
            var barcodeindex = -1;
            var qtyindex = -1;

            $.each(value.rows[0].values, function (kk, vv) {
                if (vv.toUpperCase() == 'BARCODE') {
                    barcodeindex = kk;
                }
                if (vv.toUpperCase() == 'QTY') {
                    qtyindex = kk;
                }
            });


            if (barcodeindex == -1) {
                $rootScope.Toaster.Info("Barcode column missing");
                return;
            }

            if (qtyindex == -1) {
                $rootScope.Toaster.Info("Qty column missing");
                return;
            }

            if ($scope.ItemList ||
                $scope.ItemList == null ) {
                $rootScope.Toaster.Info("Please create items list before upload the qty");
                return;
            }

            var missingbarcodes = [];

            $.each(value.rows, function (k, v) {
                if (k == 0) return;
                var barcode = v.values[barcodeindex];
                var qty = v.values[qtyindex];

                var dumpqty = $scope.ItemList.find(function (item) {
                    return item.barcode == barcode;
                });

                if (dumpqty == undefined ||
                    dumpqty == null) {
                    missingbarcodes.push(barcode);
                }
                else {
                    dumpqty.dqty = parseFloat(qty);
                }
            });
            if (missingbarcodes.length > 0) {
                $rootScope.Toaster.Info("Missing product - " + missingbarcodes.join(','));
            }
            $scope.igrid.UpdateData($scope.ItemList);
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
                $scope.Dump = item;
                $scope.ItemList = [];
                $scope.SearchItems(item.id);
            }
        }

        $scope.Print = function (item) {
            $rootScope.Download('StockdumpingService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + item.id, 'dumpingadvice.pdf', 'application/pdf');
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