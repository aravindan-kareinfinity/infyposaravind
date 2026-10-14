
var app = angular.module('InfinityLab');

app.controller('SupplierProductGroupsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.selectedcount = 0;
        $scope.EnableProductGroup = false;
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.ProductGroups = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductGroups = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.Sellingnametemplates = restService.Get('ReferencelistService/SELLINGNAMES', true).then(function (results) {
            $scope.Sellingnametemplates = results;
        });

        $scope.CurrentAttribute = 'Size';

        $scope.MarginModes = [{ 'id': '1', 'name': 'Fixed' }, { 'id': '2', 'name': 'Markup - Cost' }, { 'id': '3', 'name': 'Mark down - MRP' }];
        $scope.BarcodeModes = [{ 'id': '1', 'name': 'GRN' }, { 'id': '2', 'name': 'VENDOR UAN' }, { 'id': '3', 'name': 'AUTO UAN' }];
        $scope.AttributeTypes = [{ 'id': 1, 'name': 'Size' }, { 'id': 2, 'name': 'Colour' },
        { 'id': 3, 'name': 'Style' }, { 'id': 4, 'name': 'Pattern' }, { 'id': 5, 'name': 'Sleeve' }, { 'id': 6, 'name': 'Fit' },
        { 'id': 7, 'name': 'Material' }, { 'id': 8, 'name': 'Type' }, { 'id': 9, 'name': 'Brand' }];

        $scope.PatternList = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.PatternList = results;
        });
        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.SleeveList = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.SleeveList = results;
        });
        $scope.FitList = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.FitList = results;
        });
        $scope.MaterialList = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.MaterialList = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });
        $scope.StyleList = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.StyleList = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });
        $scope.BrandList = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandList = results;
        });

        $scope.ChangeAttribute = function () {
            $scope.AttributeList = [];
            $scope.CurrentAttribute = '';
            switch (parseInt($scope.AttributeTypeID)) {
                case 1:
                    $scope.AttributeList = $scope.SizeList;
                    $scope.CurrentAttribute = 'Size';
                    break;
                case 2:
                    $scope.AttributeList = $scope.ColorList;
                    $scope.CurrentAttribute = 'Colour';
                    break;
                case 3:
                    $scope.AttributeList = $scope.StyleList;
                    $scope.CurrentAttribute = 'Style';
                    break;
                case 4:
                    $scope.AttributeList = $scope.PatternList;
                    $scope.CurrentAttribute = 'Pattern';
                    break;
                case 5:
                    $scope.AttributeList = $scope.SleeveList;
                    $scope.CurrentAttribute = 'Sleeve';
                    break;
                case 6:
                    $scope.AttributeList = $scope.FitList;
                    $scope.CurrentAttribute = 'Fit';
                    break;
                case 7:
                    $scope.AttributeList = $scope.MaterialList;
                    $scope.CurrentAttribute = 'Material';
                    break;
                case 8:
                    $scope.AttributeList = $scope.TypeList;
                    $scope.CurrentAttribute = 'Type';
                    break;
                case 8:
                    $scope.AttributeList = $scope.BrandList;
                    $scope.CurrentAttribute = 'Brand';
                    break;
            }
            $timeout(function () {
                $scope['setFocusattributeid']();
            }, 10);
        }

        $scope.SizeGroup = [{ 'id': 1, 'name': 'Kids' }, { 'id': 2, 'name': 'Shirts' }, { 'id': 3, 'name': 'Pants' }, { 'id': 4, 'name': 'Inners' }, { 'id': 5, 'name': 'Material' }, { 'id': 6, 'name': 'none' }];

        $scope.CoreBrand = [{ 'id': 1, 'name': 'Pure Brand' }, { 'id': 2, 'name': 'Local Brand' }, { 'id': 3, 'name': 'Trial Brand' }, { 'id': 4, 'name': 'Un Brand' }, { 'id': 5, 'name': 'none' }];


        $scope.Entity = {
            "code": "",
            "name": "",
            "printingname": "",
            "barcodeprefix": "",
            "barcodesuffix": "",
            "discountmode": 0,
            "discountvalue": 0,
            "preferredsupplierid": 0,
            "corebrand": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00"
        };

        $scope.SupplierTradeAgreement = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.ErrorMessage = "";

        $scope.Search = function () {

            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SuppliertradeagreementService/Search",
                data: srequest
            }).success(function (results) {
                $scope.ShowSearchResult(results);
            }).error(function (result) {
            });
        }


        $scope.searchgrid = null;
        $scope.ShowSearchResult = function (results) {
            if ($scope.searchgrid != null)
                $scope.searchgrid.destroy();

            var igrid = new InfyGrid('searchresult', '450px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('supplierid', 'Supplier', 100, $scope.Suppliers);
            igrid.AddReferenceColumn('productgroupid', 'Product', 100, $scope.ProductGroups);

            igrid.AddSimpleColumn('itemcode', 'Code', 80);
            igrid.AddSimpleColumn('agreementname', 'Name', 80);
            igrid.AddFormatedColumn('agreementon', 'Date', 80, 'dd-MM-yyyy');

            if ($scope.SearchCondition.showdetail) {
                igrid.AddSimpleColumn('itemname', 'Item Name', 80);
                igrid.AddSimpleColumn('itemgln', 'UAN', 80);
                igrid.AddFormatedColumn('itemcost', 'Item Cost', 80, 'N2', 2);
                igrid.AddFormatedColumn('itemprice', 'Item Price', 60, 'N2', 2);
                igrid.AddFormatedColumn('itemmrp', 'Item MRP', 60, 'N2', 2);
            }
            else {
                igrid.AddFormatedColumn('cost', 'Cost', 80, 'N2', 2);
                igrid.AddFormatedColumn('price', 'Price', 60, 'N2', 2);
                igrid.AddFormatedColumn('mrp', 'MRP', 60, 'N2', 2);
            }
            igrid.AddReferenceColumn('approvedby', 'Approved', 100, $scope.Employees);
            igrid.AddFormatedColumn('createdon', 'Created', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('createdby', 'Created By', 100, $scope.Employees);
            igrid.AddFormatedColumn('modifiedon', 'Last Modified', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('modifiedby', 'Modifiedby By', 100, $scope.Employees);
            igrid.AddCustomColumn('disable', 'Disabled', 60, function (item, col) {
                if (item.disable)
                    return '<input type="checkbox" style="z-index: auto;margin-left:25px" checked disbaled>';
                else
                    return '<input type="checkbox" style="z-index: auto;margin-left:25px" disbaled>';
            });
            igrid.AddFormatedColumn('disabledon', 'Disabled On', 80, 'dd-MM-yyyy');

            igrid.AddButtonColumn('edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-primary btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'edit') {
                    $timeout(function () {
                        $scope.EditAgreement(item);
                    }, 10);
                }
            });

            $scope.searchgrid = igrid;
            $scope.searchgrid.UpdateData(results, '_id');
        }

        $scope.EditAgreement = function (item) {
            $scope.ShowSearch = false;
            $scope.Navigation.SetMode('EDIT');
            $scope.SupplierTradeAgreement.supplierid = item.supplierid;
            $scope.LoadData(function () {

                var prdgrp = $scope.ProductGroupList.find(function (sitem) {
                    return sitem.productgroupid == item.productgroupid;
                })

                if (prdgrp && prdgrp != null)
                    $scope.ShowDetail(prdgrp, function () {
                        $scope.TradeAgreement = $scope.TradeAgreements.find(function (sitem) {
                            return sitem.id == item.id;
                        })
                        if ($scope.TradeAgreement && $scope.TradeAgreement != null) {
                            if ($scope.TradeAgreement.items == undefined || $scope.TradeAgreement.items == undefined || $scope.TradeAgreement.items.length == 0) {
                                $scope.ShowMatrixConfig($scope.TradeAgreement);
                            }
                            else {
                                $timeout(function () {
                                    $scope.ShowItemGrid($scope.TradeAgreement);
                                }, 10);
                            }
                        }
                    });
            })
        }

        $scope.SearchCondition = {
            code: "",
            name: ""
        }

        $scope.New = function () {
            $scope.Brand = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }


        $scope.Save = function () {

            if ($scope.SupplierTradeAgreement == undefined ||
                $scope.SupplierTradeAgreement == null ||
                $scope.SupplierTradeAgreement.id == undefined ||
                $scope.SupplierTradeAgreement.id == null ||
                $scope.SupplierTradeAgreement.id == 0) {
                $rootScope.Toaster.Error("Please select supplier/product group");
                return;
            }

            request = {
                "Item": $scope.SupplierTradeAgreement,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SupplierproductgroupsService/Save",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Saved Successfully.!");
                $scope.ShowDetail(result);
            }).error(function (result) {
            });
        }

        $scope.findGST = function (supplierid) {
            var supplier = $scope.Suppliers.find(function (item) {
                return item.id == supplierid;
            });

            if (supplier && supplier != null) {
                return supplier.gstno;
            }
            return '';
        }

        $scope.LoadData = function (callback) {
            $scope.SalesAgreements = null;
            $scope.PurchaseAgreements = null;
            $scope.PGAttributeList = null;
            $scope.SupplierTradeAgreement.gstno = '';
            if ($scope.SupplierTradeAgreement.supplierid &&
                $scope.SupplierTradeAgreement.supplierid > 0) {

                var supplier = $scope.Suppliers.find(function (item) {
                    return item.id == $scope.SupplierTradeAgreement.supplierid;
                });

                if (supplier && supplier != null) {
                    $scope.SupplierTradeAgreement.gstno = supplier.gstno;
                }

                request = {
                    "Item": { 'supplierid': $scope.SupplierTradeAgreement.supplierid },
                    "securitytoken": "String content"
                }
                $http({
                    method: 'POST',
                    url: "SupplierproductgroupsService/Search",
                    data: request
                }).success(function (result) {
                    $scope.ProductGroupList = result;
                    $scope.ProductGroupGridConfig();
                    if ($scope.itemsgrid)
                        $scope.itemsgrid.UpdateData([]);
                    if ($scope.tradeagreementgrid)
                        $scope.tradeagreementgrid.UpdateData([]);
                    if ($scope.pgattributegrid)
                        $scope.pgattributegrid.UpdateData([]);

                    if (callback && callback != null)
                        callback();

                }).error(function (result) {
                });
            }
        }

        $scope.TradeAgreement = null;
        $scope.TradeAgreements = null;
        $scope.AddTradeAgreement = function () {
            if ($scope.TradeAgreements == null) {
                $scope.TradeAgreements = [];
                $scope.TradeAgreementsGridConfig();
            }

            if ($scope.mrp == undefined) {
                $rootScope.Toaster.Info("Please enter mrp");
                return;
            }

            if ($scope.price == undefined) {
                $rootScope.Toaster.Info("Please enter price");
                return;
            }

            if ($scope.cost == undefined) {
                $rootScope.Toaster.Info("Please enter cost");
                return;
            }

            if ($scope.GetNumber($scope.price) < $scope.GetNumber($scope.cost)) {
                $rootScope.Toaster.Info("Price can't be higher than Cost");
                return;
            }

            if ($scope.GetNumber($scope.mrp) < $scope.GetNumber($scope.price)) {
                $rootScope.Toaster.Info("Price can't be higher than MRP");
                return;
            }

            var productattribute = {
                'agreementname': $scope.agreementname, 'approvedby': $scope.approvedby,
                'agreementon': $scope.agreementon, 'marginmode': $scope.marginmode,
                'cost': $scope.cost, 'price': $scope.price, 'mrp': $scope.mrp,
                'productgroupid': $scope.SupplierTradeAgreement.productgroupid, 'Supplierproductgroupsid': $scope.SupplierTradeAgreement.id,
                'disable': false, 'brandid': $scope.brandid, 'supplierid': $scope.SupplierTradeAgreement.supplierid,
                'isactive': true, 'version': 1
            };

            request = {
                "Item": productattribute,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SuppliertradeagreementService/Save",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Saved Successfully.!");
                $scope.TradeAgreements.push(result);
                $scope.TradeAgreement = result;
                $scope.tradeagreementgrid.UpdateData($scope.TradeAgreements, '_id');
            }).error(function (result) {
            });
        }

        $scope.TradeAgreementsGridConfig = function () {
            if ($scope.tradeagreementgrid != null)
                $scope.tradeagreementgrid.destroy();

            var igrid = new InfyGrid('tradeagreementgrid', '450px', '100%', true, true, true, 400);
            igrid.options.showHeaderRow = true;
            igrid.AddButtonColumn('Config', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-success btn-xs'><i class='fa fa-gears' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddSimpleColumn('itemcode', 'Code', 80);
            igrid.AddSimpleColumn('agreementname', 'Name', 80);
            igrid.AddFormatedColumn('agreementon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('marginmode', 'Margin', 80, $scope.MarginModes);
            igrid.AddFormatedColumn('cost', 'Cost', 80, 'N2', 2);
            igrid.AddFormatedColumn('price', 'Price', 60, 'N2', 2);
            igrid.AddFormatedColumn('mrp', 'MRP', 60, 'N2', 2);
            igrid.AddCustomColumn('disable', 'Disable', 60, function (item, col) {
                if (item.disable)
                    return '<input type="checkbox" style="z-index: auto;margin-left:25px" checked>';
                else
                    return '<input type="checkbox" style="z-index: auto;margin-left:25px">';
            });
            igrid.AddFormatedColumn('disabledon', 'Until', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('approvedby', 'Approved', 100, $scope.Employees);
            igrid.AddButtonColumn('Clone', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-info btn-xs'><i class='fa fa-clone' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete') {
                    $scope.DeleteAggreement(item);

                }
                if (command == 'Clone') {
                    $timeout(function () {
                        $scope.CloneAggreement(item);
                    }, 10);
                }
                else if (command == 'Config') {
                    $scope.TradeAgreement = item;
                    if (item.items == undefined || item.items == undefined || item.items.length == 0) {
                        $scope.ShowMatrixConfig(item);
                    }
                    else {
                        $timeout(function () {
                            $scope.ShowItemGrid(item);
                        }, 10);
                    }
                }
                else if (igrid.grid.getColumns()[cell.cell].id == 'disable') {
                    item = igrid.grid.getDataItem(cell.row);
                    item.disable = !item.disable && $(e.target).val() == 'on';
                    $scope.DisableAggreement(item);
                    $timeout(function () {
                        igrid.grid.invalidateRow(cell.row);
                        igrid.grid.render();
                    }, 10);
                }
            });

            $scope.tradeagreementgrid = igrid;
            if ($scope.TradeAgreements && $scope.TradeAgreements != null)
                $scope.tradeagreementgrid.UpdateData($scope.TradeAgreements, '_id');
        }

        $scope.DisableAggreement = function (item) {
            var request = {
                "Item": { 'id': item.id, 'disable': item.disable },
                "securitytoken": "String content"
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SuppliertradeagreementService/UpdateDisable",
                data: srequest
            }).success(function (result) {
                if (result) {
                    if (item.disable)
                        $rootScope.Toaster.Error("Disabled");
                    else
                        $rootScope.Toaster.Error("Recovered");
                }
            });
        }

        $scope.CloneAggreement = function (source) {

            var productattribute = {
                'agreementname': source.agreementname, 'approvedby': source.approvedby,
                'agreementon': source.agreementon, 'marginmode': source.marginmode,
                'cost': source.cost, 'price': source.price, 'mrp': source.mrp, 'namingmode': source.namingmode,
                'productgroupid': source.productgroupid, 'Supplierproductgroupsid': source.Supplierproductgroupsid,
                'disable': false, 'brandid': source.brandid, 'supplierid': source.supplierid,
                'isactive': true, 'version': 1, 'matrixlist': source.matrixlist, 'barcodemode': source.barcodemode
            };

            var dialog = ngDialog.open({
                template: 'CloneAgreement',
                data: {
                    'scope': $scope, 'agreement': productattribute
                },
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData.scope;
                    $scope.agreement = $scope.ngDialogData.agreement;
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'agreement': $scope.agreement,
                            'copymatrix': $scope.copymatrix,
                            'copyitems': $scope.copyitems
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var request = {
                            "Item": data.value.agreement,
                            "securitytoken": "String content"
                        };

                        if (!data.value.copymatrix) {
                            request.Item.matrixlist = null;
                        }
                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "SuppliertradeagreementService/Save",
                            data: srequest
                        }).success(function (result) {

                            $rootScope.Toaster.Info("Saved Successfully.!");
                            $scope.TradeAgreements.push(result);
                            $scope.TradeAgreement = result;
                            $scope.tradeagreementgrid.UpdateData($scope.TradeAgreements, '_id');

                            if (data.value.copyitems) {

                                if (source.items && source.items != null) {
                                    $scope.TradeAgreement.items = JSON.parse(JSON.stringify(source.items));
                                    $.each($scope.TradeAgreement.items, function (k, v) {
                                        v.itemid = 0;
                                        v.gln = '';
                                    })
                                }

                                request = {
                                    "Item": $scope.TradeAgreement,
                                    "securitytoken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "SuppliertradeagreementService/UpdateMatrixItems",
                                    data: srequest
                                }).success(function (result) {
                                    $scope.AttributeChanged = false;
                                    $scope.ItemGLNChanged = false;
                                    $scope.TradeAgreement.items = result.items;
                                    $scope.ShowItemGrid($scope.TradeAgreement);
                                }).error(function (result) {
                                });

                            }
                        }).error(function (result) {

                        });

                    }
                }
            });
        }

        $scope.PGAttributeList = null;
        $scope.AttributeChanged = false;
        $scope.AddAttribute = function () {
            if ($scope.PGAttributeList == null) {
                $scope.PGAttributeList = [];
                $scope.ProductAttributeGridConfig();
            }
            var attribute = $scope.AttributeList.find(function (item) {
                return item.id == $scope.attributeid
            });
            if (attribute && attribute != null) {

                var exist = $scope.PGAttributeList.find(function (item) {
                    return item.attributeid == attribute.id &&
                        item.attributetype == $scope.CurrentAttribute;
                });

                if (exist == undefined || exist == null) {
                    var productattribute = {
                        'attributetype': $scope.CurrentAttribute, 'attributetypeid': $scope.AttributeTypeID,
                        'attributeid': attribute.id, 'attributename': attribute.name, 'disable': false
                    };
                    $scope.PGAttributeList.push(productattribute);
                    $scope.pgattributegrid.UpdateData($scope.PGAttributeList, '_id');
                    $scope.AttributeChanged = true;
                }
                else {
                    $rootScope.Toaster.Error("Already Exist in the list");
                }
            }
        }

        $scope.ProductAttributeGridConfig = function () {
            if ($scope.pgattributegrid != null)
                $scope.pgattributegrid.destroy();

            var igrid = new InfyGrid('attributeslist', '450px', '100%', true, true, true, 90);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('attributetype', 'Attribute', 100);
            igrid.AddSimpleColumn('attributename', 'Name', 100);
            igrid.AddCustomColumn('disable', 'Disable', 60, function (item, col) {
                if (item.disable)
                    return '<input type="checkbox" style="z-index: auto;margin-left:25px" checked>';
                else
                    return '<input type="checkbox" style="z-index: auto;margin-left:25px">';
            });
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e, args) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete') {
                    $scope.DeleteAttribute(item);
                }
                if (command == 'disable') {
                    item.disable = !item.disabled && $(e.target).val() == 'on';
                    $timeout(function () {
                        igrid.grid.invalidateRow(cell.row);
                        igrid.grid.render();
                    }, 10);
                }
            });

            $scope.pgattributegrid = igrid;
            if ($scope.PGAttributeList && $scope.PGAttributeList != null)
                $scope.pgattributegrid.UpdateData($scope.PGAttributeList, '_id');
        }

        $scope.ProductGroupList = null;
        $scope.AddProductGroup = function () {
            if ($scope.ProductGroupList == null) {
                $scope.ProductGroupList = [];
                $scope.SaveEmpty(function (response) {
                    $scope.ProductGroupList.push(response);
                    $scope.ProductGroupGridConfig();
                });
            }
            else {
                var exist = $scope.ProductGroupList.find(function (item) {
                    return item.productgroupid == $scope.SupplierTradeAgreement.productgroupid;
                });

                if (exist && exist != null) {
                    $rootScope.Toaster.Error("Already Exist in the list");
                    return;
                }
                $scope.SaveEmpty(function (response) {
                    $scope.ProductGroupList.push(response);
                    $scope.pggrid.UpdateData($scope.ProductGroupList, '_id');
                });
            }
        }

        $scope.SaveEmpty = function (callback) {

            var supplier = $scope.Suppliers.find(function (item) {
                return item.id == $scope.SupplierTradeAgreement.supplierid;
            });

            var prdgrp = $scope.ProductGroups.find(function (item) {
                return item.id == $scope.SupplierTradeAgreement.productgroupid;
            });

            request = {
                "Item": {
                    'isactive': true,
                    'itemcode': supplier.code + prdgrp.code,
                    'cur_seq': 1,
                    'supplierid': $scope.SupplierTradeAgreement.supplierid,
                    'productgroupid': $scope.SupplierTradeAgreement.productgroupid,
                    'version': 1
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SupplierproductgroupsService/Save",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Added Successfully.!");
                    $scope.ShowDetail(result);
                    callback(result);
                }
            }).error(function (result) {
            });
        }

        $scope.ProductGroupGridConfig = function () {
            if ($scope.pggrid != null)
                $scope.pggrid.destroy();

            var igrid = new InfyGrid('productgrouplist', '450px', '100%', true, true, true, 100);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('productgroupid', 'Product Group', 180, $scope.ProductGroups);
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Show', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-primary btn-xs'><i class='fa  fa-arrow-circle-right' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddSimpleColumn('itemcode', 'Code', 60);
            igrid.AddSimpleColumn('cur_seq', 'Name', 60);
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Show')
                    $scope.ShowDetail(item);
                if (command == 'Delete')
                    $scope.DeleteProductGroup(item);
            });
            $scope.pggrid = igrid;
            $scope.pggrid.UpdateData($scope.ProductGroupList, '_id');
        }

        $scope.DeleteAttribute = function (item) {

            $timeout(function () {

                $rootScope.Confirm('Delete', 'Do you want to delete ' + item.attributename + '?', function () {
                    $scope.PGAttributeList.splice($scope.PGAttributeList.indexOf(item), 1);
                    $scope.pgattributegrid.UpdateData($scope.PGAttributeList, '_id');
                });

            }, 10);
        }

        $scope.DeleteAggreement = function (item) {
            $rootScope.Confirm('Delete', 'Do you want to delete ' + item.agreementname + "?", function () {

                $http({
                    method: 'POST',
                    url: "SuppliertradeagreementService/Delete",
                    data: { 'item': item }
                }).success(function (result) {
                    if (result) {
                        $scope.TradeAgreements.splice($scope.TradeAgreements.indexOf(item), 1);
                        $rootScope.Toaster.Info("Deleted Successfully.!");
                        if ($scope.itemsgrid != null)
                            $scope.itemsgrid.destroy();
                        $scope.tradeagreementgrid.UpdateData($scope.TradeAgreements, '_id');
                    }
                }).error(function (result) {
                });

            });
        }

        $scope.DeleteProductGroup = function (item) {

            var pgrd = $scope.ProductGroups.find(function (fitem) {
                return item.productgroupid == fitem.id;
            });

            $rootScope.Confirm('Delete', 'Do you want to delete ' + pgrd.name + "?", function () {

                request = {
                    "Item": item,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SupplierproductgroupsService/Delete",
                    data: srequest
                }).success(function (result) {
                    $scope.ProductGroupList.splice($scope.ProductGroupList.indexOf(item), 1);
                    $scope.pggrid.UpdateData($scope.ProductGroupList, '_id');
                }).error(function (result) {
                });

            })

        }

        $scope.ManageAttribute = function () {
            var dialog = ngDialog.open({
                template: 'ManageAttribute',
                data: {
                    'scope': $scope
                },
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'code': $scope.code,
                            'name': $scope.name,
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            "Item": {
                                'type': $scope.CurrentAttribute.toUpperCase(),
                                'name': data.value.name,
                                'code': data.value.code,
                                'isactive': true
                            },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "ReferencelistService/Save",
                            data: srequest
                        }).success(function (result) {
                            if (result != null && result.id == 0) {
                                $rootScope.Toaster.Error(result.ErrorMessage);
                                return;
                            }
                            var currentattributelist = $scope.AttributeList;
                            currentattributelist.push(result);
                            $scope.AttributeList = null;
                            $timeout(function () {
                                $scope.AttributeList = currentattributelist;
                            }, 20);
                        }).error(function (result) {

                        });
                    }
                }
            });
        }

        $scope.TradeAgreement = null;
        $scope.ShowMatrixConfig = function (agreement) {

            var masters = [];
            if (agreement.matrixlist == undefined || agreement.matrixlist == null)
                agreement.matrixlist = [];

            $.each($scope.PGAttributeList, function (k, v) {
                var matattconfig = agreement.matrixlist.find(function (item) {
                    return item.attributetype == v.attributetype;
                });

                if (matattconfig == null || matattconfig == undefined) {
                    matattconfig = { 'attributetype': v.attributetype, 'items': [] };
                    agreement.matrixlist.push(matattconfig);
                }

                var existmaster = masters.find(function (item) {
                    return item.attributetype == v.attributetype;
                });
                if (existmaster == undefined || existmaster == null) {
                    existmaster = { 'attributetype': v.attributetype, 'items': [] };
                    masters.push(existmaster);
                }
                var existconfig = matattconfig.items.find(function (item) {
                    return item.id == v.attributeid;
                });
                v.selected = existconfig && existconfig != null;
                existmaster.items.push(v);
            });

            var dialog = ngDialog.open({
                template: 'MatrixConfiguration',
                data: [$scope, masters, agreement],
                width: '650px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.masters = $scope.ngDialogData[1];
                    $scope.Agreement = $scope.ngDialogData[2];
                    $scope.barcodemode = $scope.Agreement.barcodemode;
                    $scope.namingmode = $scope.Agreement.namingmode;
                    $scope.SelectAll = function (value, list) {
                        $.each(list, function (k, v) {
                            v.selected = value;
                        });
                    };
                    $scope.Selected = function () {
                        $scope.Agreement.barcodemode = $scope.barcodemode;
                        $scope.Agreement.namingmode = $scope.namingmode;
                        $.each($scope.masters, function (k, v) {
                            var matrix = $scope.Agreement.matrixlist.find(function (item) {
                                return item.attributetype == v.attributetype;
                            });
                            matrix.items = [];
                            $.each(v.items, function (kk, vv) {
                                if (vv.selected)
                                    matrix.items.push({ 'id': vv.attributeid });
                            })
                        })
                        $scope.closeThisDialog(true);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        if (agreement.items == null || agreement.items == undefined) {
                            agreement.items = [];
                        }
                        var rows = [];
                        var hasbrand = false;
                        var hasstyle = false;
                        var haspattern = false;
                        var hassize = false;
                        var hasfit = false;
                        var hassleeve = false;
                        var hascolour = false;
                        var hasmaterial = false;
                        var hastype = false;

                        $.each(agreement.matrixlist, function (k, v) {
                            var field = '';
                            switch (v.attributetype.toLowerCase()) {
                                case 'size':
                                    field = 'sizeid';
                                    hassize = true;
                                    break;
                                case 'colour':
                                    field = 'colourid';
                                    hascolour = true;
                                    break;
                                case 'style':
                                    field = 'styleid';
                                    hasstyle = true;
                                    break;
                                case 'pattern':
                                    field = 'patternid';
                                    haspattern = true;
                                    break;
                                case 'sleeve':
                                    field = 'sleeveid';
                                    hassleeve = true;
                                    break;
                                case 'fit':
                                    field = 'fitid';
                                    hasfit = true;
                                    break;
                                case 'material':
                                    field = 'materialid';
                                    hasmaterial = true;
                                    break;
                                case 'type':
                                    field = 'typeid';
                                    hastype = true;
                                    break;
                                case 'brand':
                                    field = 'brandid';
                                    hasbrand = true;
                                    break;
                            }
                            if (field != '')
                                rows = $scope.BuildInnerRows(rows, v.items, field);
                        });

                        var removeexist = [];
                        $.each(agreement.items, function (k, v) {
                            var row = rows.find(function (item) {
                                return (item.fit == v.fitid || !hasfit) &&
                                    (item.brandid == v.brandid || !hasbrand) &&
                                    (item.sleeveid == v.sleeveid || !hassleeve) &&
                                    (item.styleid == v.styleid || !hasstyle) &&
                                    (item.patternid == v.patternid || !haspattern) &&
                                    (item.sizeid == v.sizeid || !hassize) &&
                                    (item.colourid == v.colourid || !hascolour) &&
                                    (item.materialid == v.materialid || !hasmaterial) &&
                                    (item.typeid == v.typeid || !hastype);
                            });

                            var hasgln = v.gln && v.gln != null && v.gln != '';
                            if (!hasgln) {
                                if (row == undefined || row == null) {
                                    removeexist.push(v);
                                }
                                else {
                                    rows.splice(rows.indexOf(row), 1);
                                }
                            }
                        })

                        $.each(removeexist, function (k, v) {
                            agreement.items.splice(agreement.items.indexOf(v), 1);
                        });

                        $.each(rows, function (k, v) {
                            agreement.items.push(v);
                        });

                        if ($scope.AttributeChanged)
                            agreement.attributelist = $scope.PGAttributeList;

                        if (agreement.namingmode &&
                            agreement.namingmode != null &&
                            agreement.namingmode > 0) {

                            var template = $scope.Sellingnametemplates.find(function (item) {
                                return item.id == agreement.namingmode;
                            });
                            if (template && template != null) {


                                var hasproduct = template.name.indexOf('[PRODUCT]') >= 0;
                                var hasbrand = template.name.indexOf('[BRAND]') >= 0;
                                var hascolour = template.name.indexOf('[COLOUR]') >= 0;
                                var hasstyle = template.name.indexOf('[STYLE]') >= 0;
                                var hassize = template.name.indexOf('[SIZE]') >= 0;
                                var hasmaterial = template.name.indexOf('[MATERIAL]') >= 0;
                                var hassleeve = template.name.indexOf('[SLEEVE]') >= 0;
                                var hasfit = template.name.indexOf('[FIT]') >= 0;
                                var haspattern = template.name.indexOf('[PATTERN]') >= 0;
                                var hastype = template.name.indexOf('[TYPE]') >= 0;

                                var template = template.name.replace('[AGREEMENT]', agreement.agreementname);

                                if (template.indexOf('[PRODUCT]') >= 0) {
                                    var product = $scope.ProductGroups.find(function (item) {
                                        return agreement.productgroupid == item.id;
                                    });
                                    if (product && product != null)
                                        template = template.replace('[PRODUCT]', product.name);
                                }
                                if (template.indexOf('[BRAND]') >= 0) {
                                    var brand = $scope.BrandList.find(function (item) {
                                        return agreement.brandid == item.id;
                                    });
                                    if (brand && brand != null)
                                        template = template.replace('[BRAND]', brand.name);
                                }
                                $.each(agreement.items, function (k, v) {
                                    var newtemplate = template.slice();
                                    if (hascolour) {
                                        var attrib = $scope.ColorList.find(function (item) {
                                            return v.colourid == item.id;
                                        });
                                        if (attrib && attrib != null)
                                            newtemplate = newtemplate.replace('[COLOUR]', attrib.name);
                                    }
                                    if (hasstyle) {
                                        var attrib = $scope.StyleList.find(function (item) {
                                            return v.styleid == item.id;
                                        });
                                        if (attrib && attrib != null)
                                            newtemplate = newtemplate.replace('[STYLE]', attrib.name);
                                    }
                                    if (hassize) {
                                        var attrib = $scope.SizeList.find(function (item) {
                                            return v.sizeid == item.id;
                                        });
                                        if (attrib && attrib != null)
                                            newtemplate = newtemplate.replace('[SIZE]', attrib.name);
                                    }
                                    if (hastype) {
                                        var attrib = $scope.TypeList.find(function (item) {
                                            return v.typeid == item.id;
                                        });
                                        if (attrib && attrib != null)
                                            newtemplate = newtemplate.replace('[TYPE]', attrib.name);
                                    }
                                    if (haspattern) {
                                        var attrib = $scope.PatternList.find(function (item) {
                                            return v.patternid == item.id;
                                        });
                                        if (attrib && attrib != null)
                                            newtemplate = newtemplate.replace('[PATTERN]', attrib.name);
                                    }
                                    if (hasfit) {
                                        var attrib = $scope.FitList.find(function (item) {
                                            return v.fitid == item.id;
                                        });
                                        if (attrib && attrib != null)
                                            newtemplate = newtemplate.replace('[FIT]', attrib.name);
                                    }
                                    if (hassleeve) {
                                        var attrib = $scope.SleeveList.find(function (item) {
                                            return v.sleeveid == item.id;
                                        });
                                        if (attrib && attrib != null)
                                            newtemplate = newtemplate.replace('[SLEEVE]', attrib.name);
                                    }
                                    if (hasmaterial) {
                                        var attrib = $scope.MaterialList.find(function (item) {
                                            return v.materialid == item.id;
                                        });
                                        if (attrib && attrib != null)
                                            newtemplate = newtemplate.replace('[MATERIAL]', attrib.name);
                                    }
                                    v.cost = agreement.cost;
                                    v.price = agreement.price;
                                    v.mrp = agreement.mrp;
                                    v.printingname = newtemplate;
                                });
                            }
                        }
                        request = {
                            "Item": agreement,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "SuppliertradeagreementService/UpdateMatrixItems",
                            data: srequest
                        }).success(function (result) {
                            $scope.AttributeChanged = false;
                            $scope.ItemGLNChanged = false;
                            agreement.items = result.items;
                            $scope.ShowItemGrid(agreement);
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.ShowItemGrid = function (agreement) {
            if ($scope.itemsgrid != null)
                $scope.itemsgrid.destroy();
            var igrid = new InfyGrid('itemsgrid', '450px', '100%', true, true, true, 330);
            igrid.options.showHeaderRow = true;
            igrid.AddColumn(
                {
                    id: "gln", name: "UAN", field: "gln",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            igrid.AddColumn(
                {
                    id: "printingname", name: "Printing", field: "printingname",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            igrid.AddColumn(
                {
                    id: "cost", name: "Cost", field: "cost",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            igrid.AddColumn(
                {
                    id: "price", name: "SRP", field: "price",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            igrid.AddColumn(
                {
                    id: "mrp", name: "MRP", field: "mrp",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            if (agreement.matrixlist && agreement.matrixlist != null) {
                $.each(agreement.matrixlist, function (k, v) {
                    var datalist = [];
                    var field = '';
                    switch (v.attributetype.toLowerCase()) {
                        case 'size':
                            field = 'sizeid';
                            datalist = $scope.SizeList;
                            break;
                        case 'colour':
                            field = 'colourid';
                            datalist = $scope.ColorList;
                            break;
                        case 'style':
                            field = 'styleid';
                            datalist = $scope.StyleList;
                            break;
                        case 'pattern':
                            field = 'patternid';
                            datalist = $scope.PatternList;
                            break;
                        case 'sleeve':
                            field = 'sleeveid';
                            datalist = $scope.SleeveList;
                            break;
                        case 'fit':
                            field = 'fitid';
                            datalist = $scope.FitList;
                            break;
                        case 'material':
                            field = 'materialid';
                            datalist = $scope.MaterialList;
                            break;
                        case 'type':
                            field = 'typeid';
                            datalist = $scope.TypeList;
                            break;
                        case 'brand':
                            field = 'brandid';
                            datalist = $scope.BrandList;
                            break;
                    }
                    igrid.AddReferenceColumn(field, v.attributetype, 120, datalist);

                });
            }
            igrid.AddButtonColumn('Clone', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-info btn-xs'><i class='fa fa-clone' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onCellChange.subscribe(function (e, args) {
                $timeout(function () {
                    $scope.ItemGLNChanged = true;
                }, 20);
            });

            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData().getItem(row);
                if (rowdata.nonuan)
                    rowdata.style = 'igridbigrowswithred';
                else
                    rowdata.style = '';
                return { 'cssClasses': rowdata.style };
            }

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Clone') {
                    $timeout(function () {
                        $scope.CloneItem(item);
                    }, 10);
                }
            });
            $scope.selectedcount = 0;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            $scope.itemsgrid = igrid;
            if (agreement.items && agreement.items != null)
                $scope.itemsgrid.UpdateData(agreement.items, '_id');
        }
        $scope.selectedcount = 0;
        $scope.CloneItem = function (item) {
            var row = JSON.parse(JSON.stringify(item));
            request = {
                "Item": row,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SuppliertradeagreementService/CloneMatrixItems",
                data: srequest
            }).success(function (result) {
                $scope.ItemGLNChanged = true;
                row.gln = '';
                row.itemid = result.itemid;
                $scope.TradeAgreement.items.splice($scope.TradeAgreement.items.indexOf(item) + 1, 0, row);
                $scope.itemsgrid.UpdateData($scope.TradeAgreement.items, '_id');
            }).error(function (result) {
            });
        }

        $scope.ExportItems = function () {
            $scope.exportHref = $scope.itemsgrid.ExportExcel($scope.itemsgrid.grid);
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

        $scope.Export = function () {
            if (typeof ($scope.searchgrid) == 'undefined') return;
            $scope.exportHref = $scope.searchgrid.ExportExcel($scope.searchgrid.grid, 'VENDOR AGREEMENT');
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

        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            var barcodeindex = -1;
            var priceindex = -1;
            var costindex = -1;
            var qtyindex = -1;
            var mrpindex = -1;
            var descipindex = -1;
            $.each(value.columns, function (k, v) {
                var colunmname = v.toString().trim().toLowerCase();
                if (colunmname == 'barcode' || colunmname == 'uan' || colunmname == 'gln')
                    barcodeindex = k;
                if (colunmname == 'srp' || colunmname == 'price')
                    priceindex = k;
                if (colunmname == 'cost' || colunmname == 'rate')
                    costindex = k;
                if (colunmname == 'mrp')
                    mrpindex = k;
                if (colunmname == 'name' || colunmname == 'description' || colunmname == 'printing')
                    descipindex = k;
            });

            if (barcodeindex == -1) {
                $rootScope.Toaster.Error("Barcode column is missing");
                return;
            }

            $.each(value.rows, function (k, v) {
                if (k == 0) return;
                var glnexist = $scope.TradeAgreement.items.find(function (item) {
                    return item.gln == v.values[barcodeindex];
                })
                if (priceindex >= 0) {
                    glnexist.price = parseFloat(v.values[priceindex]);
                }
                if (descipindex >= 0 && v.values[descipindex] != '') {
                    glnexist.printingname = v.values[descipindex].trim();
                }
                if (costindex >= 0) {
                    glnexist.cost = parseFloat(v.values[costindex]);
                }
                if (mrpindex >= 0) {
                    glnexist.mrp = parseFloat(v.values[mrpindex]);
                }
            });
            $scope.ItemGLNChanged = true;
            $scope.itemsgrid.UpdateData($scope.TradeAgreement.items, '_id');
        }

        $scope.DeleteMatrixItems = function () {
            $rootScope.Confirm('Delete', 'Do you want to delete ?', function () {
                var rows = $scope.itemsgrid.GetSelectedRows();
                $.each(rows, function (k, v) {
                    $scope.TradeAgreement.items.splice($scope.TradeAgreement.items.indexOf(v), 1);
                });
                $scope.itemsgrid.UpdateData($scope.TradeAgreement.items, '_id');
                var request = {
                    "Item": {
                        'id': $scope.TradeAgreement.id,
                        'productgroupid': $scope.TradeAgreement.productgroupid,
                        'items': $scope.TradeAgreement.items,
                        'itemcode': $scope.TradeAgreement.itemcode,
                        'updateitemonly': true
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "SuppliertradeagreementService/UpdateMatrixItems",
                    data: srequest
                }).success(function (result) {

                }).error(function (result) {
                    $.each(rows, function (k, v) {
                        $scope.TradeAgreement.items.push(v);
                        $scope.itemsgrid.UpdateData($scope.TradeAgreement.items, '_id');
                    });
                });

            });
        }

        $scope.UpdateMatrixItems = function () {
            if ($scope.AttributeChanged)
                agreement.attributelist = $scope.PGAttributeList;
            var request = {
                "Item": {
                    'itemcode': $scope.TradeAgreement.itemcode,
                    'id': $scope.TradeAgreement.id,
                    'productgroupid': $scope.TradeAgreement.productgroupid,
                    'attributelist': $scope.PGAttributeList,
                    'items': $scope.TradeAgreement.items,
                    'updateglnonly': true
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SuppliertradeagreementService/UpdateMatrixItems",
                data: srequest
            }).success(function (result) {
                $scope.AttributeChanged = false;
                $scope.ItemGLNChanged = false;
            }).error(function (result) {
            });
        }

        $scope.ItemGLNChanged = false;

        $scope.BuildInnerRows = function (rows, values, fieldid) {
            var newrows = [];

            var rowEntity = {
                "productid": 0,
                "brandid": 0,
                "sizeid": 0,
                "patternid": 0,
                "styleid": 0,
                "colourid": 0,
                "typeid": 0,
                "materialid": 0,
                "key": ''
            };

            if (rows.length == 0) {
                $.each(values, function (k, v) {
                    var row = JSON.parse(JSON.stringify(rowEntity));
                    row[fieldid] = v.id;
                    newrows.push(row);
                });
            }
            else {
                $.each(rows, function (k, v) {
                    $.each(values, function (kk, vv) {
                        var row = JSON.parse(JSON.stringify(v));
                        row[fieldid] = vv.id;
                        newrows.push(row);
                    });
                });
            }

            if (newrows.length == 0) return rows;
            rows = null;
            return newrows;
        }

        $scope.ImportAttribute = function () {
            var dialog = ngDialog.open({
                template: 'ImportAttributes',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $timeout(function () {
                        var igrid = new InfyGrid('attributegrid', '250px', '100%', true, true, true);
                        igrid.options.showHeaderRow = true;
                        igrid.AddSimpleColumn('code', 'Code', 100);
                        igrid.AddSimpleColumn('name', 'Name', 200);
                        igrid.Initialize();
                        $scope.igrid = igrid;
                        $scope.igrid.UpdateData($scope.Parent.AttributeList, '_id');
                    }, 50);

                    $scope.Selected = function () {
                        $scope.closeThisDialog($scope.igrid.GetSelectedRows());
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        if ($scope.PGAttributeList == null) {
                            $scope.PGAttributeList = [];
                            $scope.ProductAttributeGridConfig();
                        }
                        $.each(data.value, function (k, attribute) {
                            var productattribute = {
                                'attributetype': $scope.CurrentAttribute, 'attributetypeid': $scope.AttributeTypeID,
                                'attributeid': attribute.id, 'attributename': attribute.name, 'disable': false
                            };
                            $scope.PGAttributeList.push(productattribute);
                        })
                        $scope.pgattributegrid.UpdateData($scope.PGAttributeList, '_id');
                    }
                }
            });
        }

        $scope.UpdateNonUANItems = function () {
            var rows = $scope.itemsgrid.GetSelectedRows();
            $.each(rows, function (k, v) {
                if (typeof (v.nonuan) == 'undefined') {
                    v.nonuan = false;
                }
                v.nonuan = !v.nonuan;
            });
            $scope.ItemGLNChanged = true;
            $scope.itemsgrid.UpdateData($scope.TradeAgreement.items, '_id');
        }

        $scope.AddMissingAttribute = function (list, missingattribes, name) {
            if (missingattribes && missingattribes != null && missingattribes.length > 0) {
                $.each(missingattribes, function (k, v) {
                    list.push({ 'key': name, 'value': v });
                })
            }
        }

        $scope.ImportAggreement = function (source) {
            $scope.ImportAggreementmodel = null;
            source.missingattribes = [];
            source.missingproducts = source.Status.missing_product && source.Status.missing_product != null && source.Status.missing_product.length > 0;
            source.noofrows = source.Status.noofrows;
            $scope.AddMissingAttribute(source.missingattribes, source.Status.missing_brand, 'BRAND');
            $scope.AddMissingAttribute(source.missingattribes, source.Status.missing_size, 'SIZE');
            $scope.AddMissingAttribute(source.missingattribes, source.Status.missing_pattern, 'PATTERN');
            $scope.AddMissingAttribute(source.missingattribes, source.Status.missing_material, 'MATERIAL');
            $scope.AddMissingAttribute(source.missingattribes, source.Status.missing_style, 'STYLE');
            $scope.AddMissingAttribute(source.missingattribes, source.Status.missing_colour, 'COLOUR');
            $scope.AddMissingAttribute(source.missingattribes, source.Status.missing_fit, 'FIT');
            $scope.AddMissingAttribute(source.missingattribes, source.Status.missing_sleeve, 'SLEEVE');
            $scope.AddMissingAttribute(source.missingattribes, source.Status.missing_type, 'TYPE');
            var dialog = ngDialog.open({
                template: 'ImportAgreement',
                data: {
                    'scope': $scope, 'agreement': source
                },
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData.scope;
                    $scope.agreement = $scope.ngDialogData.agreement;
                    $scope.Selected = function () {
                        var itemtemplate = $scope.Parent.Sellingnametemplates.find(function (item) {
                            return item.id == $scope.agreement.namingmode
                        });
                        if (itemtemplate && itemtemplate != null) {
                            $scope.agreement.ItemNameTemplateID = itemtemplate.id;
                            $scope.agreement.ItemNameTemplate = itemtemplate.name;
                        }
                        $scope.closeThisDialog({
                            'agreement': {
                                'ItemNameTemplateID': $scope.agreement.ItemNameTemplateID,
                                'ItemNameTemplate': $scope.agreement.ItemNameTemplate,
                                'barcodemode': $scope.agreement.barcodemode,
                                'uploadfileid': $scope.agreement.fileid,
                                'approvedby': $scope.agreement.approvedby
                            },
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        data.value.agreement.supplierid = $scope.SupplierTradeAgreement.supplierid;
                        var request = {
                            "Item": { 'MigrationData': data.value.agreement },
                            "securitytoken": "String content"
                        };
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "File/UpdateProductAgreement",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Saved Successfully.!");
                        }).error(function (result) {

                        });
                    }
                }
            });
        }

        $scope.ShowDetail = function (item, callback) {
            if (item.priceagreements == null)
                item.priceagreements = [];
            if (item.attributelist == null)
                item.attributelist = [];
            $scope.AttributeChanged = false;
            $scope.ItemGLNChanged = false;
            restService.Get('SuppliertradeagreementService/AllAgreement/' + item.id, true).then(function (results) {
                item.priceagreements = results;
                $scope.SupplierTradeAgreement = item;
                $scope.SupplierProduct = $scope.ProductGroups.find(function (item) {
                    return item.id == $scope.SupplierTradeAgreement.productgroupid;
                });
                $scope.PGAttributeList = item.attributelist;
                $scope.TradeAgreements = item.priceagreements;
                $scope.TradeAgreementsGridConfig();
                $scope.ProductAttributeGridConfig();
                if ($scope.itemsgrid != null)
                    $scope.itemsgrid.destroy();

                if (callback && callback != null)
                    callback();
            });

        }
    }]);



