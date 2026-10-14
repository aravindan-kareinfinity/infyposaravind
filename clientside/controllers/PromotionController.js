
var app = angular.module('InfinityLab');

app.controller('PromotionController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.SetMode('EDIT');
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.DiscountModes = [{ 'id': 1, 'name': 'On Bill Value' }, { 'id': 2, 'name': 'On Bill Item' }];
        $scope.BarcodeModes = [{ 'id': 1, 'name': 'Add Barcode' }, { 'id': 2, 'name': 'Remove Barcode' }];
        $scope.ApplicableModes = [{ 'id': 1, 'name': 'On Same Bill' }, { 'id': 2, 'name': 'On Next Bill' }, { 'id': 3, 'name': 'On Next Day Bill' },
            { 'id': 4, 'name': 'On Settlement' }, { 'id': 5, 'name': 'On Bill (Addon)' }, { 'id': 9, 'name': 'On Simulation' }];

        $scope.ConditionModes = [{ 'id': 1, 'name': 'Product Attribute' }, { 'id': 2, 'name': 'Invoice' }, { 'id': 3, 'name': 'Barcode' }, { 'id': 4, 'name': 'Marked Item' },
        { 'id': 5, 'name': 'Excluded Barcode' }, { 'id': 6, 'name': 'Excluded Marker' }, { 'id': 7, 'name': 'Excluded Discount' },
        { 'id': 9, 'name': 'Excluded Product Attribute' }, { 'id': 10, 'name': 'Bill/Bill Item Value' },
        { 'id': 11, 'name': 'Agreement Code' }, { 'id': 12, 'name': 'Barcode Prefix' }];

        $scope.BenefitModes = [{ 'id': 1, 'name': 'Discount In Percentage' }, { 'id': 2, 'name': 'Discount In Fixed Value' },
        { 'id': 3, 'name': 'Reward Points' },
        { 'id': 4, 'name': 'Discount In Qty' }, { 'id': 5, 'name': 'Combo Offer' },
        { 'id': 6, 'name': 'Discounted Item' }, { 'id': 7, 'name': 'Coupen' },
        { 'id': 11, 'name': 'Coupen By Range' },
        { 'id': 8, 'name': 'Free Goods' }, { 'id': 9, 'name': 'Flat Price' },
        { 'id': 10, 'name': 'Free Gift Marker' }];

        $scope.StockMarkers = restService.Get('ReferencelistService/MARKER', true).then(function (results) {
            $scope.StockMarkers = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
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
        $scope.Fits = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.Fits = results;
        });
        $scope.Sleeves = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.Sleeves = results;
        });
        $scope.Types = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.Types = results;
        });
        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.Location = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.Location = results;
            results.push({ 'id': -1, 'name': 'ALL LOCATION' });
        });
        $scope.Promotions = restService.Get('ReferencelistService/PROMOTIONS', true).then(function (results) {
            $scope.Promotions = results;
        });
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Entity = {
            "id": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": new Date(),
            "modifiedon": new Date(),
            "locationid": 0,
            "name": null,
            "validfrom": new Date(),
            "validto": new Date(),
            "mode": 0,
            "applicablemode": 0,
            "enabled": false,
            "barcode_add": [],
            "barcode_remove": [],
            "barcode_exclude_add": [],
            "barcode_exclude_remove": [],
            "rules": [],
            "benefit": [],
        };

        $scope.ruleEntity = {
            "mode": 0,
            "ValueFrom": 0,
            "ValueTo": 0,
            "item": {},
            "barcode": "",
            "barcodecount": 0
        };

        $scope.benefitEntity = { "mode": 0, "value": 0, "qtyfrom": 0, "qtyto": 0, "unit": 0, "applicableqty": 0, "max": 0.0, "pricefrom": 0, "priceto": 0, "priority": 0, "includediscount": true, item: {} };

        $scope.DiscountScheme = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.rule = (JSON.parse(JSON.stringify($scope.ruleEntity)));
        $scope.benefit = (JSON.parse(JSON.stringify($scope.benefitEntity)));

        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.DiscountScheme = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.AddBarcode = function () {
            if ($scope.rule.mode == 3) {
                if (typeof ($scope.rule.item.barcode) != 'undefined' && $scope.rule.item.barcode != '') {
                    if ($scope.rule.barcodemode == 2) {
                        $scope.DiscountScheme.barcode_remove.push($scope.rule.item.barcode)
                    }
                    else {
                        $scope.DiscountScheme.barcode_add.push($scope.rule.item.barcode);
                    }
                }
            }
            if ($scope.rule.mode == 5) {
                if (typeof ($scope.rule.item.barcode) != 'undefined' && $scope.rule.item.barcode != '') {
                    if ($scope.rule.barcodemode == 2) {
                        $scope.DiscountScheme.barcode_exclude_remove.push($scope.rule.item.barcode)
                    }
                    else {
                        $scope.DiscountScheme.barcode_exclude_add.push($scope.rule.item.barcode);
                    }
                }
            }
            $scope.rule.item.barcode = '';
        }

        $scope.ClonePromotion = function () {
            $rootScope.Prompt('Promotion Clone', 'Name', function (code) {
                code = code.toString();
                if (typeof (code) != 'undefined' && code.length > 0) {
                    var item = $scope.igrid.GetSelectedRows()[0];
                    item.name = code;
                    item.code = code;

                    request = {
                        "Item": item,
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "PromotionsService/Clone",
                        data: srequest
                    }).success(function (result) {

                    }).error(function (result) {
                    });
                }
            })
        }

        $scope.ShowSchemeLocation = function () {
            var item = $scope.igrid.GetSelectedRows()[0];
            item.ids = [];
            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                item.ids.push(v.id);
            })

            if (item.schemelocations == undefined || item.schemelocations == null)
                item.schemelocations = [];
            var list = [];
            $.each($scope.Location, function (k, v) {
                if (v.id == -1) return;
                list.push({ 'id': v.id, 'name': v.name, 'selected': item.schemelocations.indexOf(v.id) >= 0 });
            });

            var dialog = ngDialog.open({
                template: 'SchemeLocation',
                data: [list, item],
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.List = $scope.ngDialogData[0];
                    $scope.Item = $scope.ngDialogData[1];
                    $scope.Selected = function () {
                        $scope.closeThisDialog(
                            {
                                'List': $scope.List,
                                'Item': $scope.Item
                            }
                        );
                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                    var item = data.value.Item;
                    item.schemelocations = [];
                    $.each(data.value.List, function (k, v) {
                        if (v.selected)
                            item.schemelocations.push(v.id);
                    });

                    request = {
                        "Item": item,
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "PromotionsService/UpdateSchemeLocation",
                        data: srequest
                    }).success(function (result) {

                    }).error(function (result) {
                    });

                }
            });
        }


        $scope.schemefileuploaded = function (value) {
            $scope.agmtcodes = null;
            var itemcodeindex = -1;
            var fromindex = -1;
            var toindex = -1;
            var percentageindex = -1;
            var valueindex = -1;
            $.each(value.columns, function (k, v) {
                if (v.toLowerCase().indexOf('item') >= 0)
                    itemcodeindex = k;
                if (v.toLowerCase().indexOf('from') >= 0)
                    fromindex = k;
                if (v.toLowerCase().indexOf('to') >= 0)
                    toindex = k;
                if (v.toLowerCase().indexOf('percentage') >= 0)
                    percentageindex = k;
                if (v.toLowerCase().indexOf('value') >= 0)
                    valueindex = k;
            })

            if (itemcodeindex == -1) {
                $rootScope.Toaster.Info("ITEM CODE Column missing.");
                return;
            }

            if (fromindex == -1) {
                $rootScope.Toaster.Info("FROM DATE Column missing.");
                return;
            }

            if (toindex == -1) {
                $rootScope.Toaster.Info("TO DATE Column missing.");
                return;
            }

            if (percentageindex == -1) {
                $rootScope.Toaster.Info("DISCOUNT PERCENTAGE Column missing.");
                return;
            }

            if (valueindex == -1) {
                $rootScope.Toaster.Info("DISCOUNT VALUE Column missing.");
                return;
            }
            var list = [];
            $.each(value.rows, function (k, v) {
                if (k == 0) return;

                var item = {
                    'itemcode': v.values[itemcodeindex],
                    'fromdate': v.values[fromindex],
                    'todate': v.values[toindex],
                    'discountpercentage': parseFloat(v.values[percentageindex]),
                    'discountvalue': parseFloat(v.values[valueindex])
                };
                if (isNaN(item.discountpercentage)) item.discountpercentage = 0;
                if (isNaN(item.discountvalue)) item.discountvalue = 0;
                list.push(item);
            });

            var dialog = ngDialog.open({
                template: 'ImportPO',
                data: [list,$scope],
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.List = $scope.ngDialogData[0];
                    $scope.ParentScope = $scope.ngDialogData[1];
                    $scope.Delete = function (item) {
                        $scope.List.splice($scope.List.indexOf(item), 1);
                        if ($scope.List.length == 0)
                            $scope.closeThisDialog();
                    }
                    $scope.Selected = function () {
                        $scope.closeThisDialog(
                            {
                                'items': $scope.List,
                                'name': $scope.promotionname,
                                'companyid': $scope.companyid,
                                'locationid': $scope.locationid
                            }
                        );
                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                    request = {
                        "Item": data.value,
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "PromotionsService/UploadHyperPromotion",
                        data: srequest
                    }).success(function (result) {

                    }).error(function (result) {
                    });

                }   
            });
        }


        $scope.fileuploadedbarcodeprefix = function (value) {
            $scope.rule.barcodeprefix = null;
            $scope.barcodeprefix = null;

            if ($scope.rule.item == undefined || $scope.rule.item == null)
                $scope.rule.item = { 'barcodeprefixes': [] };

            if ($scope.rule.item.barcodeprefixes == undefined || $scope.rule.item.barcodeprefixes == null)
                $scope.rule.item.barcodeprefixes = [];

            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    if (!barcode.startsWith('BARCODE')) {

                        if ($scope.rule.item.barcodeprefixes.indexOf(barcode) < 0)
                            $scope.rule.item.barcodeprefixes.push(barcode);
                    }
                }
            });
        }

        $scope.AddBarcodePrefixcode = function () {

            if ($scope.rule.item.barcodeprefix == undefined || $scope.rule.item.barcodeprefix == null || $scope.rule.item.barcodeprefix == '') return;

            if ($scope.rule.item == undefined || $scope.rule.item == null)
                $scope.rule.item = { 'barcodeprefixes': [] };

            if ($scope.rule.item.barcodeprefixes == undefined || $scope.rule.item.barcodeprefixes == null)
                $scope.rule.item.barcodeprefixes = [];

            if ($scope.rule.item.barcodeprefixes.indexOf($scope.rule.item.barcodeprefix) < 0)
                $scope.rule.item.barcodeprefixes.push($scope.rule.item.barcodeprefix);
            $scope.rule.item.barcodeprefix = '';
        }

        $scope.RemoveBarcodePrefixcode = function (item) {
            $scope.rule.item.barcodeprefixes.splice($scope.rule.item.barcodeprefixes.indexOf(item), 1);
        }

        $scope.fileuploadedagmtcodes = function (value) {
            $scope.agmtcodes = null;
            $scope.rule.agmtcodes = null;
            if ($scope.rule.item == undefined || $scope.rule.item == null)
                $scope.rule.item = { 'agmtcodes': [] };

            if ($scope.rule.item.agmtcodes == undefined || $scope.rule.item.agmtcodes == null)
                $scope.rule.item.agmtcodes = [];

            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    if (!barcode.startsWith('BARCODE')) {

                        if ($scope.rule.item.agmtcodes.indexOf(barcode) < 0)
                            $scope.rule.item.agmtcodes.push(barcode);

                    }
                }
            });
        }

        $scope.AddAgmtcode = function () {

            if ($scope.rule.item.agmtcode == undefined || $scope.rule.item.agmtcode == null || $scope.rule.item.agmtcode == '') return;

            if ($scope.rule.item == undefined || $scope.rule.item == null)
                $scope.rule.item = { 'agmtcodes': [] };

            if ($scope.rule.item.agmtcodes == undefined || $scope.rule.item.agmtcodes == null)
                $scope.rule.item.agmtcodes = [];

            if($scope.rule.item.agmtcodes.indexOf($scope.rule.item.agmtcode)<0)
                $scope.rule.item.agmtcodes.push($scope.rule.item.agmtcode);
            $scope.rule.item.agmtcode = '';
        }

        $scope.RemoveAgmtcode = function (item) {
            $scope.rule.item.agmtcodes.splice($scope.rule.item.agmtcodes.indexOf(item), 1);
        }

        $scope.Addcondition = function (fixedmode) {
            if (typeof (fixedmode) != 'undefined')
                $scope.rule.mode = fixedmode;

            if ($scope.rule.mode == 8) {
                $rootScope.Toaster.Info("Barcodes updated.");
                $scope.rule = (JSON.parse(JSON.stringify($scope.ruleEntity)));
                return;
            }

            if ($scope.DiscountScheme.rules.indexOf($scope.rule) == -1)
                $scope.DiscountScheme.rules.push($scope.rule);
            $scope.rule = (JSON.parse(JSON.stringify($scope.ruleEntity)));
        }

        $scope.Addbenefit = function () {
            if ($scope.DiscountScheme.benefit.indexOf($scope.benefit) == -1)
                $scope.DiscountScheme.benefit.push($scope.benefit);
            $scope.benefit = (JSON.parse(JSON.stringify($scope.benefitEntity)));
        }

        $scope.GetAttributeInfo = function (item) {
            var str = '';
            if (item.productid > 0)
                str += $scope.SearchRef(item.productid, $scope.Products) + ' - ';
            if (item.brandid > 0)
                str += $scope.SearchRef(item.brandid, $scope.BrandName) + ' - ';
            if (item.materialid > 0)
                str += $scope.SearchRef(item.materialid, $scope.Material) + ' - ';
            if (item.patternid > 0)
                str += $scope.SearchRef(item.patternid, $scope.Pattern) + ' - ';
            if (item.colourid > 0)
                str += $scope.SearchRef(item.colourid, $scope.ColorList) + ' - ';
            if (item.sizeid > 0)
                str += $scope.SearchRef(item.sizeid, $scope.Sizes);
            if (item.typeid > 0)
                str += $scope.SearchRef(item.typeid, $scope.Types);
            return str;
        }

        $scope.GetBenefitDetail = function (rule, item) {
            var str = '';
            if (rule.mode == 6 || rule.mode == 8) {
                str += $scope.GetAttributeInfo(item) + ' ';
            }
            if (rule.mode == 7) {
                str += $scope.SearchRef(rule.locationid, $scope.Location);
            }
            if (rule.mode == 11) {
                if (rule.min && rule.min>0)
                    str += ' Min-' + rule.min;
                if (rule.validity && rule.validity != '')
                    str += ' Validity-' + $filter('date')(rule.validity, 'dd-MM-yyyy');
            }
            return str;
        }

        $scope.GetRuleDetail = function (rule, item) {
            var str = $scope.SearchRef(rule.mode, $scope.ConditionModes) + ' ';
            if ((rule.mode == 1 || rule.mode == 9)) {
                str += $scope.GetAttributeInfo(item);
            }
            if (rule.mode == 10 || rule.mode == 0) {
                str += rule.ValueFrom + ' - ' + rule.ValueTo + ' ';
            }
            if (rule.mode == 7) {
                str += ' Discount % : ' + rule.excludediscount;
            }
            if (rule.mode == 11 && rule.item && rule.item.agmtcodes) {
                str += ' Codes : ' + (rule.item.agmtcodes.length == 1 ? rule.item.agmtcodes[0] : rule.item.agmtcodes.length);
            }
            return str;
        }

        $scope.RemoveBenefit = function (item) {
            $scope.DiscountScheme.benefit.splice($scope.DiscountScheme.benefit.indexOf(item), 1);
        }

        $scope.RemoveCondition = function (item) {
            $scope.DiscountScheme.rules.splice($scope.DiscountScheme.rules.indexOf(item), 1);
        }

        $scope.EditBenefit = function (item) {
            $scope.benefit = item;
        }

        $scope.EditCondition = function (item) {
            $scope.rule = item;
        }

        $scope.Clear = function () {
            $scope.DiscountScheme.rules = [];
            $scope.rule = (JSON.parse(JSON.stringify($scope.ruleEntity)));
        }

        $scope.SearchCondition = {
            "name": null,
            "validfrom": null,
            "validto": null,
            "mode": 0,
            "locationid": 0
        }

        $scope.Search = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PromotionsService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ConfigSearchGrid(result);
                $scope.SearchResult = result;
            }).error(function (result) {
            });
        }

        $scope.selectedcount = 0;
        $scope.ConfigSearchGrid = function (results) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();
            var igrid = new InfyGrid('sourcegrid', '400px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', 'Name', 150);
            igrid.AddReferenceColumn('mode', 'Mode', 150, $scope.DiscountModes);
            igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.Location);
            igrid.AddFormatedColumn('validfrom', 'From', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('validto', 'To', 80, 'dd-MM-yyyy');
            var column = igrid.AddCustomColumn('enable', 'Status', 100, function (item, col) {
                return item.enabled ? 'Active' : 'Deactivated';
            });

            if ($scope.UserInfo.p_SALEPROMOTION.modify) {
                igrid.AddButtonColumn('Disable', 'Disable', 50, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Disable' class='btn btn-danger btn-xs'><i class='fa fa-circle-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                igrid.AddButtonColumn('Enable', 'Enable', 50, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Enable' class='btn btn-primary btn-xs'><i class='fa  fa-circle' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                igrid.AddButtonColumn('Edit', 'Edit', 50, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-primary btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }
            if ($scope.UserInfo.p_SALEPROMOTION.delete)
                igrid.AddButtonColumn('Delete', 'Delete', 50, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa  fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            //            <button type="button" class="btn btn-default btn-xs" ng-show="UserInfo.p_SALEPROMOTION.modify && item.enabled==true" ng-click="ManageStatus(false,item)">Disable</button>
            //<button type="button" class="btn btn-info btn-xs" ng-show="item.enabled==false" ng-click="ManageStatus(true,item)">Enable</button>
            //<button type="button" class="btn btn-danger btn-xs" ng-show="UserInfo.p_SALEPROMOTION.delete" ng-click="ManageSearchItem('Delete',item)">Delete</button>
            //<button type="button" class="btn btn-primary btn-xs" ng-show="UserInfo.p_SALEPROMOTION.modify" ng-click="ManageSearchItem('Edit',item)">Edit</button>


            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;

                $timeout(function () {
                    switch (command) {
                        case 'Disable':
                            $scope.ManageStatus(false, item);
                            break;
                        case 'Enable':
                            $scope.ManageStatus(true, item);
                            break;
                        case 'Edit':
                            $scope.ManageSearchItem('Edit', item);
                            break;
                        case 'Delete':
                            $scope.ManageSearchItem('Delete', item);
                            break;
                    }
                }, 10);
            });

            $scope.selectedcount = 0;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(results);
        }


        $scope.New = function () {
            $scope.DiscountScheme = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.rule = (JSON.parse(JSON.stringify($scope.ruleEntity)));
            $scope.benefit = (JSON.parse(JSON.stringify($scope.benefitEntity)));
        }

        $scope.fileuploadedexcludecomplete = function (value) {
            $scope.rule.barcodes = null;
            if ($scope.DiscountScheme.barcode_exclude_remove == null)
                $scope.DiscountScheme.barcode_exclude_remove = [];
            if ($scope.DiscountScheme.barcode_exclude_add == null)
                $scope.DiscountScheme.barcode_exclude_add = [];

            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    if (!barcode.startsWith('BARCODE')) {
                        //if ($scope.rule.barcodemode == 2) {
                        //    $scope.DiscountScheme.barcode_exclude_remove.push(barcode)
                        //}
                        //else {
                        $scope.DiscountScheme.barcode_exclude_add.push(barcode);
                        //}
                    }
                }
            });
        }



        $scope.fileuploaded = function (value) {
            if ($scope.rule.mode == 11) {
                $scope.fileuploadedagmtcodes(value);
            }
            else if ($scope.rule.mode == 5)
                $scope.fileuploadedexcludecomplete(value);
            else
                $scope.fileuploadedincludecomplete(value);
        }

        $scope.fileuploadedincludecomplete = function (value) {
            $scope.rule.barcodes = null;
            if ($scope.DiscountScheme.barcode_add == null)
                $scope.DiscountScheme.barcode_add = [];
            if ($scope.DiscountScheme.barcode_remove == null)
                $scope.DiscountScheme.barcode_remove = [];
            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {

                    if (!barcode.startsWith('BARCODE')) {
                        //if ($scope.rule.barcodemode == 2) {
                        //    $scope.DiscountScheme.barcode_remove.push(barcode)
                        //}
                        //else {
                        $scope.DiscountScheme.barcode_add.push(barcode);
                        //}
                    }
                }
            });
        }


        $scope.fileuploadedcouponexcludecomplete = function (value) {

            if ($scope.DiscountScheme.coupon_remove == null)
                $scope.DiscountScheme.coupon_remove = [];
            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    $scope.DiscountScheme.coupon_remove.push(barcode);
                }
            });
        }

        $scope.fileuploadedcouponincludecomplete = function (value) {

            if ($scope.DiscountScheme.coupon_add == null)
                $scope.DiscountScheme.coupon_add = [];

            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    $scope.DiscountScheme.coupon_add.push(barcode);
                }
            });
        }



        $scope.Save = function () {
            if ($scope.DiscountScheme.applicablemode == null || $scope.DiscountScheme.applicablemode == 0) {
                $rootScope.Toaster.Info("Please select Applicablemode.");
                return;
            }
            if ($scope.DiscountScheme.rules == null || $scope.DiscountScheme.rules.length == 0) {
                $rootScope.Toaster.Info("Please create scheme rules.");
                return;
            }
            if ($scope.DiscountScheme.benefit == null || $scope.DiscountScheme.benefit.length == 0) {
                $rootScope.Toaster.Info("Please create scheme benefits.");
                return;
            }

            //$scope.DiscountScheme.rules = $scope.DiscountScheme.rules;

            //$.each($scope.DiscountScheme.rules, function (k, v) {
            //    if (v.benefit.percentage != null && v.benefit.percentage > 0) {
            //        v.benefit.mode = 1;
            //        v.benefit.value = v.benefit.percentage;
            //    }
            //    else if (v.benefit.discountamount != null && v.benefit.discountamount > 0) {
            //        v.benefit.mode = 2;
            //        v.benefit.value = v.benefit.discountamount;
            //    }
            //    else if (v.benefit.pointsperamount != null && v.benefit.pointsperamount > 0) {
            //        v.benefit.mode = 3;
            //        v.benefit.value = v.benefit.points;
            //        v.benefit.unit = v.benefit.pointsperamount;
            //    }
            //    else if (v.benefit.qty != null && v.benefit.qty > 0) {
            //        v.benefit.mode = 4;
            //        v.benefit.value = v.benefit.qty;
            //    }
            //});

            request = {
                "Item": $scope.DiscountScheme,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PromotionsService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Promotions Scheme Saved Successfully.!");
                    $scope.DiscountScheme.id = result.id;
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });
        }

        $scope.ManageStatus = function (action, item) {
            item.enabled = action;
            request = {
                "Item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PromotionsService/Deactivate",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $rootScope.Toaster.Info("Promotions Scheme updated successfully.!");
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
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
                                    url: "PromotionsService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.ConfigSearchGrid($scope.SearchResult);
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

                var request = {
                    "Item": { 'id': item.id },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PromotionsService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.Navigation.SetMode('EDIT');
                    $scope.DiscountScheme = result[0];
                    $scope.DiscountScheme.barcode_exclude_remove = [];
                    $scope.DiscountScheme.barcode_exclude_add = [];
                    $scope.DiscountScheme.barcode_add = [];
                    $scope.DiscountScheme.barcode_remove = [];
                    $scope.rule = (JSON.parse(JSON.stringify($scope.ruleEntity)));
                    $scope.benefit = (JSON.parse(JSON.stringify($scope.benefitEntity)));

                }).error(function (result) {
                });
            }
        }

        $scope.RemoveBarcode = function (include) {
            var request = {
                "Item": { 'id': $scope.DiscountScheme.id, 'include': include },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PromotionsService/RemoveBarcode",
                data: srequest
            }).success(function (result) {

                if (include)
                    $scope.DiscountScheme.barcode_count = 0;
                else
                    $scope.DiscountScheme.excludebarcode_count = 0;

            }).error(function (result) {
            });
        }

        $scope.applicablemodechanged = function () {
            if ($scope.DiscountScheme.applicablemode == 4)
                $scope.DiscountScheme.mode = 1;
        }

        $scope.SaveExcluePromotionList = function (empty) {
            var list = [];
            if (!empty) {
                $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                    list.push(v.id);
                });
            }
            var request = {
                "Item": { 'promotionids': list },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ConfigurationService/SaveExcludeReward",
                data: srequest
            }).success(function (result) {

                if (include)
                    $scope.DiscountScheme.barcode_count = 0;
                else
                    $scope.DiscountScheme.excludebarcode_count = 0;

            }).error(function (result) {
            });
        }

        $scope.ShowExludePromotionList = false;
        $timeout(function () {
            if ($rootScope.UserInfo &&
                $rootScope.UserInfo.appsettings &&
                $rootScope.UserInfo.appsettings.gft_excludepromotionlist &&
                $rootScope.UserInfo.appsettings.gft_excludepromotionlist != null &&
                $rootScope.UserInfo.appsettings.gft_excludepromotionlist.length > 0) {
                $scope.ShowExludePromotionList = true;
            }
            $scope.setFocusfirst();
        }, 10);
    }]);
