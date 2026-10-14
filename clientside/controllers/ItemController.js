
var app = angular.module('InfinityLab');

app.controller('ItemController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', '$compile',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $compile) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.UploadingItems = false;
        $scope.IsDialog = typeof ($scope.ngDialogData) != 'undefined' && $scope.ngDialogData.length > 0;
        $scope.SalesMode = [{ 'id': 1, 'name': 'Single' }, { 'id': 2, 'name': 'Part' }, { 'id': 3, 'name': 'Multiple' }, { 'id': 4, 'name': 'Mode Four' }];
        $scope.DiscountSchemes = [{ 'id': 1, 'name': 'Free Qty' }, { 'id': 2, 'name': 'Combo Price' }, { 'id': 3, 'name': 'Discount Item' }, { 'id': 4, 'name': 'Discount %' }];
        $scope.ExpiryModes = [{ 'id': 1, 'name': 'Days' }, { 'id': 2, 'name': 'Months' }, { 'id': 3, 'name': 'Years' }];
        $scope.PricingMechanisms = [{ 'id': 1, 'name': 'MRP Price' }, { 'id': 2, 'name': 'Margin By Percentage' }, { 'id': 3, 'name': 'Margin By Rupees' }, { 'id': 4, 'name': 'OLD Price' }, { 'id': 5, 'name': 'Fixed Price' }, { 'id': 6, 'name': 'Dynamic Price' }];
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });
        $scope.GetItem = function (items, id) {
            var result = items.find(function (item) {
                return item.id == id;
            });
            if (typeof (result) == 'undefined')
                return { 'id': 0, 'name': '' };
            return result;
        }

        $scope.preparesellingname = function () {

            $scope.produtchanged();

            if (!$scope.IsMobileAppplication) return;

            var product = $scope.GetItem($scope.Products, $scope.Item.productid);
            if (product.id == 0) return;
            if (product.producttype == 0) {
                $scope.Item.sellingname = product.name;
                $scope.Item.printingname = $scope.Item.sellingname;
                return;
            }
            if (product.producttype == 6) {
                var nameparts = [];
                var item = $scope.GetItem($scope.Styles, $scope.Item.styleid);
                if (item.id > 0) nameparts.push(item.name);
                var item = $scope.GetItem($scope.Patterns, $scope.Item.patternid);
                if (item.id > 0) nameparts.push(item.name);
                var item = $scope.GetItem($scope.BrandName, $scope.Item.brandid);
                if (item.id > 0) nameparts.push(item.name);
                var item = $scope.GetItem($scope.Colors, $scope.Item.colourid);
                if (item.id > 0) nameparts.push(item.name);
                $scope.Item.sellingname = nameparts.join('-');
                $scope.Item.printingname = $scope.Item.sellingname;
                return;
            }
            if (product.producttype == 7) {
                var nameparts = [];
                var item = $scope.GetItem($scope.Styles, $scope.Item.styleid);
                if (item.id > 0) nameparts.push(item.name);
                var item = $scope.GetItem($scope.BrandName, $scope.Item.brandid);
                if (item.id > 0) nameparts.push(item.name);
                nameparts.push(product.code);
                $scope.Item.sellingname = nameparts.join('-');
                $scope.Item.printingname = $scope.Item.sellingname;
                return;
            }
        }

        $scope.IsMobileAppplication = false;
        $scope.LadingApplicationConfiguration = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LadingApplicationConfiguration, 100);
            }
            else {
                $scope.IsMobileAppplication = $rootScope.UserInfo.appsettings.system_ismobileapplication || false;
            }
        }

        $scope.LadingApplicationConfiguration();

        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
        });
        $scope.Patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Patterns = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
            $scope.Sizes = results;
        });
        $scope.VSizes = restService.Get('ReferencelistService/VSIZE', true).then(function (results) {
            $scope.VSizes = results;
        });
        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Styles = results;
        });
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });
        $scope.FitList = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.FitList = results;
        });
        $scope.SleeveList = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.SleeveList = results;
        });
        $scope.Entity = {
            "code": "",
            "sellingname": "",
            "brandid": 0,
            "brandname": "",
            "allowsearch": true,
            "productid": 0,
            "productname": "",
            "colourname": "",
            "colourid": 0,
            "patternid": 0,
            "patternname": "",
            "materialid": 0,
            "materialname": "",
            "sizeid": 0,
            "sizename": "",
            "styleid": 0,
            "stylename": "",
            "salemode": 0,
            "printingname": "",
            "pricingmode": 0,
            "typeid": 0,
            "typename": "",
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "createdby": 0,
            "modifiedby": 0,
            "isactive": true,
            "version": 1,
            podays: "",
            stockagedays: "",
            minrate: "",
            ratetobebilled: "",
            mrp: "",
            salerate: "",
            purchaserate: "",
            dealerprice: ""
        };

        $scope.Item = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Item = (JSON.parse(JSON.stringify($scope.Entity)));
        }
        $scope.ErrorMessage = "";
        $scope.SearchCondition = {
            sellingname: "",
            printingname: "",
            code: "",
            brandid: "",
            productid: "",
            patternid: "",
            styleid: "",
            sizeid: "",
            materialid: 0,
            materialname: ""
        }

        $scope.New = function () {
            $scope.Item = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.IsEdit = false;
        $scope.Save = function () {

            $scope.IsEdit = typeof ($scope.Item.id) != 'undefined' && $scope.Item.id > 0;
            if ($scope.Item.sellingname == null || $scope.Item.sellingname == '') {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Select Selling Name");
                return;
            }

            if ($scope.Item.printingname == null || $scope.Item.printingname == '') {
                $scope['setFocusprintingname']();
                $rootScope.Toaster.Error("Please Select Printing Name");
                return;
            }

            if ($scope.Item.productid == null || $scope.Item.productid == 0) {
                $scope['setFocusproductid']();
                $rootScope.Toaster.Error("Please Select Product");
                return;
            }

            if ($scope.Item.brandid == null || $scope.Item.brandid == 0) {
                $scope['setFocusbrandid']();
                $rootScope.Toaster.Error("Please Select Brand");
                return;
            }

            $scope.Item.brandname = $scope.SearchRef($scope.Item.brandid, $scope.BrandName, '');
            $scope.Item.productname = $scope.SearchRef($scope.Item.productid, $scope.Products, '');
            $scope.Item.patternname = $scope.SearchRef($scope.Item.patternid, $scope.Patterns, '');
            $scope.Item.materialname = $scope.SearchRef($scope.Item.materialid, $scope.Material, '');
            $scope.Item.colourname = $scope.SearchRef($scope.Item.colourid, $scope.Colors, '');
            $scope.Item.sizename = $scope.SearchRef($scope.Item.sizeid, $scope.Sizes, '');
            $scope.Item.stylename = $scope.SearchRef($scope.Item.styleid, $scope.Styles, '');
            $scope.Item.typename = $scope.SearchRef($scope.Item.typeid, $scope.TypeList, '');


            var request = {
                "Item": $scope.Item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ItemsService/Save",
                data: srequest
            }).success(function (result) {

                if ($scope.IsDialog) {
                    result.IsEdit = $scope.IsEdit;
                    $scope.closeThisDialog(result);
                    return;
                }
                if (result != null && result.id > 0) {
                    if (result.version != 1) {
                        $rootScope.Toaster.Error("Item Updated on exising item successfully.!");
                    }
                    else {
                        $rootScope.Toaster.Info("Item Saved Successfully.!");
                    }
                    $scope.Item = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }
            }).error(function (result) {
            });

        }


        $scope.fileuploadedcomplete = function (value) {
            $scope.UploadedExcel = value;
            $scope.uploadfile = null;
            $scope.Upload(value);
        }

        $scope.Upload = function (excel) {
            var dialog = ngDialog.open({
                template: 'ImportItem',
                data: {
                    'Excel': excel,
                    'scope': $scope
                },
                width: '600px',
                height: '370px',
                controller: ['$scope', '$timeout', 'localStorageService',
                    function ($scope, $timeout, localStorageService) {

                        $scope.ExcelColumns = [];
                        $scope.sheetname = $scope.ngDialogData.Excel.sheets[0].name;
                        $.each($scope.ngDialogData.Excel.sheets[0].columns, function (k, v) {
                            $scope.ExcelColumns.push({ 'id': v, 'name': v });
                        });

                        $timeout(function () {
                            var columns = localStorageService.get('MigrationData_Items');
                            if (typeof (columns) != 'undefined' && columns != null && columns.length > 0) {
                                $.each($scope.Columns, function (k, v) {

                                    var eColumn = columns.find(function (item) {
                                        return item.name == v.name;
                                    });

                                    if (typeof (eColumn) != 'undefined' && eColumn != null) {
                                        v.excelcolunmn = eColumn.excelcolunmn;
                                    }

                                });
                            }
                        }, 50);

                        $scope.Columns = [
                            { table: 'Item', display: 'PRODUCT', name: 'product', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'BRAND', name: 'brand', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'STYLE', name: 'style', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'TYPE', name: 'type', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'COLOUR', name: 'colour', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'SIZE', name: 'size', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'MATERIAL', name: 'material', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'PATTERN', name: 'pattern', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'CODE', name: 'code', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'GLN', name: 'gln', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'DESIGN', name: 'glndesignid', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'SHOWLIST', name: 'showlist', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'FIT', name: 'fit', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'SLEEVE', name: 'sleeve', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'PRINTING', name: 'printing', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'SALERATE', name: 'salerate', isnumber: 'Y', isdate: 'N' },
                            { table: 'Item', display: 'MRP', name: 'mrprate', isnumber: 'Y', isdate: 'N' },
                            { table: 'Item', display: 'COST', name: 'purchaserate', isnumber: 'Y', isdate: 'N' },
                            { table: 'Item', display: 'SUPPLIER', name: 'supplier', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'SUPPLIER CODE', name: 'suppliercode', isnumber: 'N', isdate: 'N' },
                            { table: 'Item', display: 'B2B Price', name: 'dealerprice', isnumber: 'Y', isdate: 'N' }];

                        $scope.Selected = function () {

                            localStorageService.set('MigrationData_Items', $scope.Columns);

                            var request = {
                                'columns': $scope.Columns,
                                'tablename': 'Items',
                                'sheetname': $scope.sheetname,
                                'uploadfileid': $scope.ngDialogData.Excel.fileid
                            };

                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "ItemsService/MigrateItemProcess",
                                data: srequest
                            }).success(function (result) {
                                $scope.closeThisDialog(result);
                            }).error(function (result) {
                            });
                        };

                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document'
                        || data.value == '$escape')
                        return;

                    $scope.ShowMigrateData(data.value);
                }
            });
        }

        $scope.ShowMigrateData = function (data) {
            $scope.UploadingItems = true;
            $scope.currentimportdatarows = data.rows;
            data.rows = [];
            $scope.currentimportdata = data;
            $timeout(function () {

                if ($scope.igrid != null) {
                    $scope.igrid.grid.destroy();
                    $('#importgrid').empty();
                }

                var igrid = new InfyGrid('importgrid', '80px', '100%', true, true, true, 20);
                igrid.options.showHeaderRow = true;

                igrid.AddSimpleColumn('suppliercode', 'Supplier Code', 80);
                igrid.AddSimpleColumn('supplier', 'Supplier', 80);
                igrid.AddSimpleColumn('gln', 'UAN/GLN', 100);
                igrid.AddSimpleColumn('product', 'Product', 80);
                igrid.AddSimpleColumn('brand', 'Brand', 80);
                igrid.AddSimpleColumn('glndesignid', 'Design', 80);
                igrid.AddSimpleColumn('stylename', 'Style', 80);
                igrid.AddSimpleColumn('colour', 'Colour', 80);
                igrid.AddSimpleColumn('size', 'Size', 80);
                igrid.AddSimpleColumn('pattern', 'Pattern', 80);
                igrid.AddSimpleColumn('type', 'Type', 80);
                igrid.AddSimpleColumn('fit', 'Fit', 80);
                igrid.AddSimpleColumn('sleeve', 'Sleeve', 80);
                igrid.AddSimpleColumn('purchaserate', 'Cost Price', 80);
                igrid.AddSimpleColumn('salerate', 'Sale Rate', 80);
                igrid.AddSimpleColumn('mrprate', 'MRP', 80);

                igrid.AddSimpleColumn('dealerprice', 'Dealer Price', 80);

                igrid.Initialize();
                $scope.igrid = igrid;
                igrid.dataView.getItemMetadata = function (row) {
                    var item = igrid.dataView.getItem(row);
                    if (item.exist) {
                        item.style = 'igridbigrowswithred';
                        return { cssClasses: 'igridbigrowswithred' };
                    }
                    else {
                        item.style = '';
                        return { cssClasses: '' };
                    }
                };
                $scope.igrid.UpdateData($scope.currentimportdatarows);

            }, 10);
        }

        $scope.AddSupplier = function () {
            if ($scope.Item.supplierlist == undefined || $scope.Item.supplierlist == null) {
                $scope.Item.supplierlist = [];
            }
            $scope.Item.supplierlist.push({ 'id': $scope.dynamicsupplierid });
        }

        $scope.RemoveSupplier = function (item, list) {
            list.splice(list.indexOf(item), 1);
        }

        $scope.Import = function () {

            var request = (JSON.parse(JSON.stringify($scope.currentimportdata)));
            request.rows = this.igrid.GetSelectedRows();

            if (request.rows == 'undefined' || request.rows == null || request.rows.length == 0) {
                $rootScope.Toaster.Info("Please select the rows to be Loaded.");
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ItemsService/MigrateItemImport",
                data: srequest
            }).success(function (result) {

                $scope.StatusID = result.identifier;
                $scope.UploadedingStatus = result;
                $scope.ProcessStarted = true;
                $scope.ProgressValue = 0;
                $scope.migrationtimer = $timeout(function () {
                    $scope.CheckStatus();
                }, 2000);


                //$rootScope.Toaster.Info("Items Uploaded successfully.!");

                //var request = (JSON.parse(JSON.stringify($scope.currentimportdata)));
                //var srequest = JSON.stringify(request);
                //$http({
                //    method: 'POST',
                //    url: "ItemsService/MigrateItemProcess",
                //    data: srequest
                //}).success(function (result) {
                //    $scope.ShowMigrateData(result);
                //}).error(function (result) {
                //});

            }).error(function (result) {
            });
        }

        $scope.CheckStatus = function () {
            restService.Get('File/ProcessStatus/' + $scope.UserInfo.OrganizationID + '/' + $scope.StatusID).then(function (result) {
                $scope.UploadedingStatus = result;
                $scope.ProgressValue = result.percentage;
                if (!result.hascompleted) {
                    $scope.migrationtimer = $timeout(function () {
                        $scope.CheckStatus();
                    }, 2000);
                }
            });
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
                                    url: "ItemsService/Delete",
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
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.Item = item;
                $scope.produtchanged();
                if ($scope.system_customfields) {
                    if ($scope.Item.CustomFields == null)
                        $scope.Item.CustomFields = [];

                    $.each($scope.Entity.CustomFields, function (cf, cv) {
                        var cf = $scope.Item.CustomFields.find(function (item) {
                            return item.key == cv.key;
                        });
                        if (cf == undefined || cf == null) {
                            $scope.Item.CustomFields.push({ 'key': cv.key, 'value': null });
                        }
                    });
                }
            }
        }

        $scope.Search = function () {

            if (typeof ($scope.SearchCondition.showall) != 'undefined' && $scope.SearchCondition.showall) {
                $scope.SearchCondition.onlysearchable = false;
            }
            else {
                $scope.SearchCondition.onlysearchable = true;
            }
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ItemsService/Search",
                data: srequest
            }).success(function (result) {

                $.each(result, function (k, v) {
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

                $scope.SearchResult = result;
                $scope.ItemConfig();
            }).error(function (result) {
            });

        }


        $scope.ItemConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 10);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sellingname', 'Selling Name', 120);
            igrid.AddSimpleColumn('printingname', 'Printing Name', 120);
            igrid.AddSimpleColumn('display', 'Detail', 400);
            igrid.AddReferenceColumn('salemode', 'Sale Mode', 100, $scope.SalesMode);
            igrid.AddReferenceColumn('pricingmode', 'Pricing', 120, $scope.PricingMechanisms);

            var commands = "";
            if ($rootScope.UserInfo.p_ITEMMASTER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_ITEMMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_ITEMMASTER.modify || $rootScope.UserInfo.p_ITEMMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });
            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }

        $scope.MandatoryStyle = {
            'color': 'orangered'
        };

        $scope.Mandatory = {
            "attributefilter": false
        }

        $scope.produtchanged = function () {
            var prditem = $scope.Products.find(function (item) {
                return (item.id == $scope.Item.productid);
            });
            if (prditem || prditem != null) {
                $scope.CurrentProduct = prditem;
                if (prditem.attributes != null) {
                    $scope.Mandatory = {
                        "attributefilter": prditem.attributefilter,
                        "producttype": prditem.producttype,
                        "style": prditem.attributes.mandatorystyle || prditem.attributes.mandatorymodel ? $scope.MandatoryStyle : {},
                        "pattern": prditem.attributes.mandatorypattern || prditem.attributes.mandatoryquality ? $scope.MandatoryStyle : {},
                        "brand": prditem.attributes.mandatorybrand ? $scope.MandatoryStyle : {},
                        "material": prditem.attributes.mandatorymaterial || prditem.attributes.mandatorywarrenty ? $scope.MandatoryStyle : {},
                        "designid": prditem.attributes.mandatorydesignid ? $scope.MandatoryStyle : {},
                        "purchaseorder": prditem.attributes.mandatorypurchaseorder ? $scope.MandatoryStyle : {},
                        "type": prditem.attributes.mandatorytype ? $scope.MandatoryStyle : {},
                        "fit": prditem.attributes.mandatoryfit ? $scope.MandatoryStyle : {},
                        "sleeve": prditem.attributes.mandatorysleeve ? $scope.MandatoryStyle : {},
                        "colour": prditem.attributes.mandatorycolour ? $scope.MandatoryStyle : {},
                        "size": prditem.attributes.mandatorysize ? $scope.MandatoryStyle : {},
                        "item": prditem.attributes.mandatoryitem ? $scope.MandatoryStyle : {},
                        "serialno": prditem.attributes.mandatoryserialno ? $scope.MandatoryStyle : {},
                        "batchandexpiry": prditem.attributes.mandatorybatchandexpiry ? $scope.MandatoryStyle : {},
                        "model": prditem.attributes.mandatorymodel ? $scope.MandatoryStyle : {},
                        "quality": prditem.attributes.mandatoryquality ? $scope.MandatoryStyle : {},
                        "warrenty": prditem.attributes.mandatorywarrenty ? $scope.MandatoryStyle : {},
                        "multipleprice": prditem.attributes.mandatorymultipleprice ? $scope.MandatoryStyle : {},
                        "simtype": prditem.attributes.mandatorysimtype ? $scope.MandatoryStyle : {},
                        "weight": prditem.attributes.mandatoryweight ? $scope.MandatoryStyle : {},
                        "wastage": prditem.attributes.mandatorywastage ? $scope.MandatoryStyle : {},
                        "workingcharge": prditem.attributes.mandatoryworkingcharge ? $scope.MandatoryStyle : {},
                        "attributes": prditem.attributes
                    };

                    if ($scope.system_customfields &&
                        prditem.attributes.customfields) {
                        $.each($scope.ItemTemplate.CustomFields, function (k, v) {
                            var prdconfig = prditem.attributes.customfields.find(function (item) {
                                return item.key == v.key;
                            });
                            if (prdconfig && prdconfig != null) {
                                $scope.Mandatory.attributes[v.key] = prdconfig.show;
                                $scope.Mandatory.style[v.key] = prdconfig.mandatory ? $scope.MandatoryStyle : {};
                            }
                            else {
                                $scope.Mandatory.attributes[v.key] = false;
                                $scope.Mandatory.style[v.key] = {};
                            }
                        });
                    }

                    if (prditem.attributefilter && !prditem.attributes.mandatorysize) {
                        $scope.ItemDetails.Details.sizeid = 0;
                    }

                    if (typeof ($scope.Mandatory.attributes) != 'undefined' &&
                        $scope.Mandatory.attributefilter) {
                        $scope.Mandatory.attributes.sno = true;
                        $scope.Mandatory.attributes.hsncode = true;

                        $scope.Mandatory.style = {
                            "brand": 'col-md-6',
                            "colour": 'col-md-6',
                            "size": 'col-md-6',
                            "pattern": 'col-md-6',
                            "style": 'col-md-6',
                            "type": 'col-md-6',
                            "material": 'col-md-6',
                            "fit": 'col-md-6',
                            "sleeve": 'col-md-6'
                        };

                        if ($scope.Mandatory.attributes.colouroption) {
                            if ($scope.Mandatory.attributes.brand != $scope.Mandatory.attributes.size) {
                                if ($scope.Mandatory.attributes.brand)
                                    $scope.Mandatory.style.brand = 'col-md-12';
                                else
                                    $scope.Mandatory.style.size = 'col-md-12';
                            }
                        } else if ($scope.Mandatory.attributes.brand != $scope.Mandatory.attributes.colour) {
                            if ($scope.Mandatory.attributes.brand)
                                $scope.Mandatory.style.brand = 'col-md-12';
                            else
                                $scope.Mandatory.style.colour = 'col-md-12';
                        }

                        if (($scope.Mandatory.attributes.pattern || $scope.Mandatory.attributes.quality) != ($scope.Mandatory.attributes.style || $scope.Mandatory.attributes.model)) {
                            if ($scope.Mandatory.attributes.pattern || $scope.Mandatory.attributes.quality)
                                $scope.Mandatory.style.pattern = 'col-md-12';
                            else
                                $scope.Mandatory.style.style = 'col-md-12';
                        }

                        if (($scope.Mandatory.attributes.type || $scope.Mandatory.attributes.simtype) != ($scope.Mandatory.attributes.material || $scope.Mandatory.attributes.warrenty)) {
                            if ($scope.Mandatory.attributes.type || $scope.Mandatory.attributes.simtype)
                                $scope.Mandatory.style.type = 'col-md-12';
                            else
                                $scope.Mandatory.style.material = 'col-md-12';
                        }

                        if (($scope.Mandatory.attributes.fit) != ($scope.Mandatory.attributes.sleeve)) {
                            if ($scope.Mandatory.attributes.fit)
                                $scope.Mandatory.style.fit = 'col-md-12';
                            else
                                $scope.Mandatory.style.sleeve = 'col-md-12';
                        }

                    } else {
                        if (typeof ($scope.Mandatory.attributes) != 'undefined')
                            $scope.Mandatory.attributes = {};

                        $scope.Mandatory.attributes.sno = true;
                        $scope.Mandatory.attributes.hsncode = true;
                        $scope.Mandatory.style = {
                            "brand": 'col-md-6',
                            "colour": 'col-md-6',
                            "size": 'col-md-6',
                            "pattern": 'col-md-6',
                            "style": 'col-md-6',
                            "type": 'col-md-6',
                            "material": 'col-md-6',
                            "fit": 'col-md-6',
                            "sleeve": 'col-md-6'
                        };
                    }

                    if (prditem.attributes.item) {
                        $scope.FilterItemList(prditem.id);
                    }
                } else {
                    $scope.Mandatory = {
                        "pattern": {},
                        "brand": {},
                        "material": {},
                        "warrenty": {},
                        "designid": {},
                        "purchaseorder": {},
                        "type": {},
                        "colour": {},
                        "size": {},
                        "multipleprice": {},
                        "attributefilter": false,
                        "producttype": 0,
                        "item": {},
                        "serialno": {},
                        "batchandexpiry": {},
                        "model": {},
                        "quality": {},
                        "simtype": {},
                        "attributes": {
                            'type': true,
                            'pattern': true,
                            'simtype': false,
                            "sno": true,
                            "hsncode": true,
                            'serialno': false,
                            'batchandexpiry': false,
                            'item': false,
                            'colouroption': false,
                            'quality': false,
                            'model': false,
                            'warrenty': false,
                            'multipleprice': false
                        },
                        "style": {
                            "brand": 'col-md-6',
                            "colour": 'col-md-6',
                            "size": 'col-md-6',
                            "pattern": 'col-md-6',
                            "style": 'col-md-6',
                            "type": 'col-md-6',
                            "material": 'col-md-6',
                            "fit": 'col-md-6',
                            "sleeve": 'col-md-6'
                        }
                    };
                }

                if ($scope.SupportAttributeFilter)
                    $scope.ProductAttributeFilter(prditem.attributelist);

                //if (prditem.brands != null && prditem.brands.length > 0) {
                //    $scope.productbrandmap = true;
                //    $scope.BrandName = [];
                //    $.each(prditem.brands, function (k, v) {
                //        $scope.BrandName.push({ id: v.id, name: v.name });
                //    });
                //}
                //else if ($scope.productbrandmap) {
                //    $scope.BrandName = $scope.BrandNameList;
                //    $scope.productbrandmap = false;
                //}

                //$scope.taxChange();
            }

            if ($scope.CurrentProduct && $scope.CurrentProduct != null) {
                if ($scope.CurrentProduct.producttype == 0) {
                    $scope.Sizes = $scope.SizeList;
                }
                else {
                    $scope.Sizes = $scope.VSizes;
                }
            }
        }

        $scope.LoadAppSetting = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadAppSetting, 100);
            } else {
                $scope.system_customfields = $rootScope.UserInfo.appsettings.system_customfields || false;
                if ($scope.system_customfields) {
                    restService.Get('ReferencelistService/FULL-CUSTOMFIELDS', true).then(function (results) {
                        var fields = [];
                        $scope.Entity.CustomFields = [];
                        $scope.Item.CustomFields = [];
                        $.each(results, function (k, v) {

                            if (v.additionalinfo.customfield.cfbuilt) {
                                $scope.Entity.CustomFields.push({
                                    'key': v.additionalinfo.customfield.cfname,
                                    'value': '',
                                });
                                $scope.Item.CustomFields.push({
                                    'key': v.additionalinfo.customfield.cfname,
                                    'value': '',
                                });
                                var fieldname = 'Item.CustomFields[' + k + '].value';
                                var topdiv = '<div class="col-md-12 no-padding" style="margin-bottom: 5px;" ng-show="!Mandatory.attributefilter || Mandatory.attributes.' + v.additionalinfo.customfield.cfname + '">';
                                switch (v.referenceid) {
                                    case 1:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><input type="text" class="input-sm form-control" ng-model="' + fieldname + '"></div></div>');
                                        break;
                                    case 2:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><input type="number" class="input-sm form-control" ng-model="' + fieldname + '"></div></div>');
                                        break;
                                    case 3:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><input type="text" class="form-control input-sm datepicker" datemodule="' + fieldname + '" ng-model="' + fieldname + '"></div></div>');
                                        break;
                                    case 4:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in CustomReference_' + v.id + ' track by item.id"></select></div></div>');
                                        $scope.UpdateCustomReferenceList(v, 'CustomReference_' + v.id);
                                        break;
                                    case 5:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in Colors track by item.id"></select></div></div>');
                                        break;
                                    case 6:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in Sizes track by item.id"></select></div></div>');
                                        break;
                                    case 7:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in FitList track by item.id"></select></div></div>');
                                        break;
                                    case 8:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in TypeList track by item.id"></select></div></div>');
                                        break;
                                    case 9:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in Material track by item.id"></select></div></div>');
                                        break;
                                    case 10:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in Styles track by item.id"></select></div></div>');
                                        break;
                                    case 11:
                                        fields.push(topdiv + '<label class="col-md-4 control-label">' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<div class="col-md-8"><select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in Patterns track by item.id"></select></div></div>');
                                        break;
                                }
                            }
                        });
                        angular.element($("#customefields")).append($compile(fields.join(''))($scope));
                    });
                }
            }
        }

        $scope.UpdateCustomReferenceList = function (referenceitem, scopename) {
            restService.Get('ReferencelistService/' + referenceitem.code, true).then(function (results) {
                $scope[scopename] = results;
            });
        }

        $scope.LoadAppSetting();
    }]);




