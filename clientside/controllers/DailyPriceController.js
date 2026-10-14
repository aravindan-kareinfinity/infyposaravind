var app = angular.module('InfinityLab');
app.controller('DailyPriceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, $filter) {

        $rootScope.AccordionName = $location.path();
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


        $scope.UOM = [{ 'id': 0, 'name': 'Piece' }, { 'id': 1, 'name': 'Meter' },
        { 'id': 2, 'name': 'Gram' },
        { 'id': 3, 'name': 'Kg' },
        { 'id': 4, 'name': 'Ml' },
        { 'id': 5, 'name': 'Litter' },
        { 'id': 6, 'name': 'Qty' }];

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Locations = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.DailyPriceGroup = restService.Get('ReferencelistService/DAILYPRICEGROUP', true).then(function (results) {
            $scope.DailyPriceGroup = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
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


        $scope.CanSave = typeof ($rootScope.UserInfo.p_DAILYPRICE.modify) == 'undefined' ? false : $rootScope.UserInfo.p_DAILYPRICE.modify;


        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.DailyPrice = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.disableclear = false;
        $scope.ClearCurrent = function () {
            if ($scope.disableclear) return;

            $scope.DailyPrice.id = 0;
            $scope.DailyPrice.items = [];
            $scope.CopyApplyList = [];
            $scope.CopyCount = 0;
            $scope.ApplyCount = 0;
            if ($scope.lgrid != null) {
                $scope.lgrid.grid.destroy();
                $('#locationgrid').empty();
            }
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }
        }

        $scope.EnableLocationCopy = function () {
            $scope.CopyCount = 0;
            $scope.ApplyCount = 0;
            if ($scope.lgrid != null) {
                $scope.lgrid.grid.destroy();
                $('#locationgrid').empty();
            }

            if ($scope.DailyPrice && $scope.DailyPrice.id && $scope.DailyPrice.id > 0) {
                var newlocations = [];
                $.each($scope.Locations, function (k, v) {
                    var available = null;

                    if ($scope.DailyPrice.priceavailablelocations &&
                        $scope.DailyPrice.priceavailablelocations != null) {
                        available = $scope.DailyPrice.priceavailablelocations.find(function (item) {
                            return item.locationid = v.id;
                        })
                    }
                    newlocations.push({
                        'locationid': v.id, 'available': (available && available != null) ? true : false,
                        'applied': (available && available != null) ? available.applied : false,
                        'appliedrows': (available && available != null) ? available.rows.toString() : '',
                        'existpriceid': (available && available != null) ? available.existpriceid : 0
                    });
                })

                var igrid = new InfyGrid('locationgrid', '425px', '100%', true, true, true, 275);
                igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.Locations);
                igrid.AddCustomColumn('available', 'Available', 50, function (item, col) {
                    if (item.available)
                        return '<input type="checkbox" readonly  disabled style="z-index: auto;margin-left:25px" checked>';
                    else
                        return '';
                });

                igrid.AddCustomColumn('applied', 'Applied', 50, function (item, col) {
                    if (item.applied)
                        return '<input type="checkbox" readonly  disabled style="z-index: auto;margin-left:25px" checked>';
                    else
                        return '';
                });

                igrid.AddCustomColumn('copy', 'Copy', 50, function (item, col) {
                    if (item.copy)
                        return '<input type="checkbox" style="z-index: auto;margin-left:25px" checked>';
                    else
                        return '<input type="checkbox" style="z-index: auto;margin-left:25px">';
                });
                igrid.AddCustomColumn('apply', 'Apply', 50, function (item, col) {
                    if (item.available) {
                        if (item.apply)
                            return '<input type="checkbox" style="z-index: auto;margin-left:25px" checked>';
                        else
                            return '<input type="checkbox" style="z-index: auto;margin-left:25px">';
                    }
                    else {
                        return '';
                    }
                });

                igrid.options.autoEdit = true;
                igrid.Initialize();

                igrid.grid.onClick.subscribe(function (e) {
                    var cell = igrid.grid.getCellFromEvent(e);
                    var item = igrid.grid.getDataItem(cell.row);
                    var command = igrid.grid.getColumns()[cell.cell].id;
                    if (command == 'apply') {
                        item.apply = !item.apply && $(e.target).val() == 'on';
                        $timeout(function () {
                            $scope.UpdateCopyApply();
                            igrid.grid.invalidateRow(cell.row);
                            igrid.grid.render();
                        }, 10);
                    }
                    if (command == 'copy') {
                        item.copy = !item.copy && $(e.target).val() == 'on';
                        $timeout(function () {
                            $scope.UpdateCopyApply();
                            igrid.grid.invalidateRow(cell.row);
                            igrid.grid.render();
                        }, 10);
                    }
                });
                $scope.CopyApplyList = newlocations;
                igrid.UpdateData(newlocations);
                $scope.lgrid = igrid;
            }
        }

        $scope.UpdateCopyApply = function () {
            $scope.CopyCount = 0;
            $scope.ApplyCount = 0;
            $.each($scope.CopyApplyList, function (k, v) {
                $scope.CopyCount += (v.copy ? 1 : 0);
                $scope.ApplyCount += (v.apply ? 1 : 0);
            });
        }

        $scope.ShowDailPriceItems = function () {
            if ($scope.DailyPrice.priceon == undefined ||
                $scope.DailyPrice.priceon == null ||
                $scope.DailyPrice.priceon == '') {
                $rootScope.Toaster.Info("Please select the date before search");
                return;
            }

            var request = {
                "Item": {
                    'pricegroupid': $scope.DailyPrice.pricegroupid,
                    'locationid': $scope.DailyPrice.locationid,
                    'companyid': $scope.DailyPrice.companyid,
                    'priceon': $scope.DailyPrice.priceon
                }
            };
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DailypriceService/PriceList",
                data: srequest
            }).success(function (result) {
                $scope.disableclear = true;
                $scope.DailyPrice = result;
                $scope.ConfigGrid(result);
                $timeout(function () {
                    $scope.disableclear = false;
                    $scope.EnableLocationCopy();
                }, 40);
            }).error(function (result) {
            });
        }


        $scope.ApplyPrice = function () {
            $scope.DailyPrice.applylocations = [];
            $.each($scope.CopyApplyList, function (k, v) {
                if (v.apply) {
                    $scope.DailyPrice.applylocations.push(v);
                }
            });
            var request = {
                "Item": $scope.DailyPrice
            };
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DailypriceService/Apply",
                data: srequest
            }).success(function (result) {
                if (result) {
                    $rootScope.Toaster.Info("Applied Successfully.!");
                }
            }).error(function (result) {
            });
        }


        $scope.CopyPrice = function () {
            $scope.DailyPrice.copylocations = [];
            $.each($scope.CopyApplyList, function (k, v) {
                if (v.copy) {
                    $scope.DailyPrice.copylocations.push(v);
                }
            });
            var request = {
                "Item": $scope.DailyPrice
            };
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DailypriceService/Copy",
                data: srequest
            }).success(function (result) {
                if (result) {
                    $rootScope.Toaster.Info("Copied Successfully.!");
                }
            }).error(function (result) {
            });
        }

        $scope.IsDealerPrice = false;
        $scope.ConfigGrid = function (result) {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(result.items);
            igrid.AddReferenceColumn('productid', 'Product', 180, $scope.Products);
            igrid.AddSimpleColumn('itemcode', 'Item Code', 100);
            igrid.AddSimpleColumn('agreementname', 'Agreement', 100);
            igrid.AddSimpleColumn('description', 'Description', 100);
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddFormatedColumn('lastprice', 'Current', 80, 'N2', 2);
            var column = igrid.AddColumn(
                {
                    id: "newprice", name: "New Price", field: "newprice",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                item.style = '';
                if (item.uploadedprice)
                    item.style = 'igridbigrowswithdarkgreen';
                else if (!item.stockexist)
                    item.style = 'igridbigrowswithred';
                return { cssClasses: '' };
            };

            igrid.grid.onCellChange.subscribe(function (e, args) {
                $timeout(function () {
                    $scope.SaveAgain = true;
                }, 10);
            });
            igrid.UpdateData(result.items);
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

        $scope.SearchCondition = {};
        $scope.Search = function () {
            $scope.SearchCondition.searchonly = true;
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DailypriceService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SaveAgain = false;
                $scope.ConfigSearchGrid(result);
            }).error(function (result) {
            });
        }

        $scope.ConfigSearchGrid = function (result) {
            if ($scope.sgrid != null) {
                $scope.sgrid.grid.destroy();
                $('#searchgridContainer').empty();
            }
            var igrid = new InfyGrid('searchgridContainer', '425px', '100%', true, true, true, 50);
            igrid.EnableSerial(result);

            if ($scope.SearchCondition.companyid == undefined || $scope.SearchCondition.companyid == null)
                igrid.AddReferenceColumn('companyid', 'Company', 180, $scope.Company);

            if ($scope.SearchCondition.locationid == undefined || $scope.SearchCondition.locationid == null)
                igrid.AddReferenceColumn('locationid', 'Location', 180, $scope.Locations);

            igrid.AddReferenceColumn('employeeid', 'Prepared By', 180, $scope.Employees);
            igrid.AddFormatedColumn('priceon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('appliedby', 'Applied By', 180, $scope.Employees);
            igrid.AddFormatedColumn('appliedon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('appliedrows', 'Affected', 80, 'N0', 0);
            igrid.AddReferenceColumn('createdby', 'Created By', 180, $scope.Employees);
            igrid.AddReferenceColumn('modifiedby', 'Modified By', 180, $scope.Employees);

            if ($scope.SearchCondition.productid && $scope.SearchCondition.productid != null &&
                $scope.SearchCondition.productid > 0) {
                igrid.AddSimpleColumn('description', 'Product', 100);
                igrid.AddSimpleColumn('itemcode', 'Item Code', 100);
                igrid.AddSimpleColumn('barcode', 'Barcode', 100);
                igrid.AddFormatedColumn('lastprice', 'Current', 80, 'N2', 2);
                igrid.AddFormatedColumn('newprice', 'Current', 80, 'N2', 2);
            }

            igrid.AddButtonColumn('edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-primary btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'edit') {
                    $timeout(function () {
                        $scope.EditDailyPrice(item);
                    }, 10);
                }
            });
            igrid.UpdateData(result, '_id');
            $scope.igrid = igrid;
        }

        $scope.EditDailyPrice = function (item) {
            $scope.ShowSearch = false;
            var request = {
                "Item": { 'id': item.id },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "DailypriceService/Search",
                data: srequest
            }).success(function (result) {
                $scope.DailyPrice = result[0];
                $scope.ConfigGrid($scope.DailyPrice);
                $timeout(function () {
                    $scope.EnableLocationCopy();
                }, 40);
            }).error(function (result) {
            });
        }

        $scope.fileuploaded = function (value) {
            $scope.barcodes = null;
            var barcodeindex = -1;
            var priceindex = -1;

            $.each(value.rows[0].values, function (kk, vv) {
                if (vv.toUpperCase() == 'BARCODE') {
                    barcodeindex = kk;
                }
                if (vv.toUpperCase() == 'PRICE') {
                    priceindex = kk;
                }
            });


            if (barcodeindex == -1) {
                $rootScope.Toaster.Info("Barcode column missing");
                return;
            }

            if (priceindex == -1) {
                $rootScope.Toaster.Info("Price column missing");
                return;
            }

            if ($scope.DailyPrice == undefined ||
                $scope.DailyPrice == null ||
                $scope.DailyPrice.items == undefined ||
                $scope.DailyPrice.items == null) {
                $rootScope.Toaster.Info("Please create items list before upload the price");
                return;
            }

            var missingbarcodes = [];

            $.each(value.rows, function (k, v) {
                if (k == 0) return;
                var barcode = v.values[barcodeindex];
                var price = v.values[priceindex];

                var dpriceitem = $scope.DailyPrice.items.find(function (item) {
                    return item.barcode == barcode;
                });

                if (dpriceitem == undefined ||
                    dpriceitem == null) {
                    missingbarcodes.push(barcode);
                }
                else {
                    dpriceitem.uploadedprice = true;
                    dpriceitem.newprice = parseFloat(price);
                }
            });

            if (missingbarcodes.length > 0) {
                $rootScope.Toaster.Info("Missing product - " + missingbarcodes.join(','));
            }
            $scope.igrid.UpdateData($scope.DailyPrice.items);
        }

        $scope.Save = function () {

            var request = {
                "Item": $scope.DailyPrice,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DailypriceService/Save",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $scope.SaveAgain = false;
                    $scope.disableclear = true;
                    $scope.DailyPrice = result;
                    $scope.ConfigGrid(result);
                    $timeout(function () {
                        $scope.disableclear = false;
                        $scope.EnableLocationCopy();
                    }, 40);
                    $rootScope.Toaster.Info("Saved Successfully.!");
                }
                else {
                    $rootScope.Toaster.Info("Error - " + result.ErrorMessage);
                }
            }).error(function (result) {
            });
        }

        $scope.SaveAgain = false;
        $scope.ApplyCurrent = function () {

            $scope.DailyPrice.applylocations = [{ 'existpriceid': $scope.DailyPrice.id }];
            var request = {
                "Item": $scope.DailyPrice
            };
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DailypriceService/Apply",
                data: srequest
            }).success(function (result) {
                if (result) {
                    $rootScope.Toaster.Info("Applied Successfully.!");
                    $scope.ShowDailPriceItems();
                }
            }).error(function (result) {
            });
        }

        $scope.Export2CSV = function () {

            var rows = [];
            var header = [];

            $.each($scope.DailyPrice.items, function (k, v) {

                if (k == 0) {
                    header.push('code,barcode,product,unit,rate');
                    rows.push(header);
                }
                var cells = [];

                cells.push(v.barcode.replace(/[^0-9]/g, ''));
                cells.push(v.barcode);
                var product = $scope.Products.find(function (item) {
                    return item.id == v.productid;
                });

                var uom = $scope.UOM.find(function (item) {
                    return item.id == product.uomid;
                });

                //cells.push(product.name);
                cells.push(v.description);
                cells.push(uom.name);
                if (v.newprice && !isNaN(parseFloat(v.newprice)) &&
                    parseFloat(v.newprice) > 0) {
                    cells.push(v.newprice);
                }
                else {
                    cells.push(v.lastprice);
                }
                rows.push(cells.join(','));
            });

            var blob = new Blob([rows.join('\r\n')],
                { type: "text/plain;charset=utf-8" });
            saveAs(blob, "DailyPrice.csv");
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