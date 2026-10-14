
var app = angular.module('InfinityLab');

app.controller('SKUCreationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', '$filter', '$compile',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter, $compile) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowPrint = false;
        $scope.system_isb2bapplication = false;
        $scope.system_sizegrouplayout = false;
        $scope.skucodes = '[YY][MM][SUPPLIER][PRODUCT]';
        $scope.stylecode = '[YY][MM][SUPPLIER][PRODUCT][AUTO]';
        $scope.descriptioncodes = '[YY][MM][SUPPLIER][PRODUCT][AUTO]_[SIZE]';
        $scope.descriptionname = 'PRODUCT/SUPPLIER/SLEEVE/COLOUR/SIZE';
        $scope.mfg_eancode = '';
        $scope.mfg_manualdesigncode = true;
        $scope.system_possizelist = '';
        $scope.mfg_eansequence = true;
        $scope.alloworder = { "background-color": "white", "color": "black" };
        $scope.disalloworder = { "background-color": "black", "color": "white" };
        $scope.ShowConsumption = false;

        $scope.UpdateCustomReferenceList = function (referenceitem, scopename) {
            restService.Get('ReferencelistService/' + referenceitem.code, true).then(function (results) {
                $scope[scopename] = results;
            });
        }

        $scope.panelheight = { 'height': '400px' };

        $scope.LoadItems = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo.appsettings == null ||
                typeof ($rootScope.UserInfo.appsettings.system_supportitemwiseentry) == 'undefined') {
                $timeout(function () {
                    $scope.LoadItems();
                }, 10);
                return;
            }

            $scope.system_sizegrouplayout = $rootScope.UserInfo.appsettings.system_sizegrouplayout;
            $scope.system_isb2bapplication = $rootScope.UserInfo.appsettings.system_isb2bapplication;
            if ($scope.system_isb2bapplication) {
                $scope.ColorList = $scope.Colors;
            }
            $scope.IsUANBased = $rootScope.UserInfo.appsettings.mfg_mode == 5;
            $scope.mfg_eansequence = $rootScope.UserInfo.appsettings.mfg_eansequence;
            if (!$scope.mfg_eansequence && $rootScope.UserInfo.appsettings.mfg_eancode != null)
                $scope.mfg_eancode = $rootScope.UserInfo.appsettings.mfg_eancode;

            if ($rootScope.UserInfo.appsettings.mfg_manualdesigncode) {
                $scope.skucodes = '';
            }
            else {
                $scope.mfg_manualdesigncode = false;
                if ($rootScope.UserInfo.appsettings.mfg_designcode != null)
                    $scope.skucodes = $rootScope.UserInfo.appsettings.mfg_designcode;
            }

            if (typeof ($rootScope.UserInfo.appsettings.mfg_designsizecode) != 'undefined')
                $scope.descriptioncodes = $rootScope.UserInfo.appsettings.mfg_designsizecode;

            if (typeof ($rootScope.UserInfo.appsettings.mfg_stylecode) != 'undefined')
                $scope.stylecode = $rootScope.UserInfo.appsettings.mfg_stylecode;

            if (typeof ($rootScope.UserInfo.appsettings.mfg_designsizedescription) != 'undefined')
                $scope.descriptionname = $rootScope.UserInfo.appsettings.mfg_designsizedescription;

            $scope.mfg_fabricbasedorder = $rootScope.UserInfo.appsettings.mfg_fabricbasedorder;
            $scope.system_possizelist = $rootScope.UserInfo.appsettings.system_possizelist;
            $scope.system_customfields = $rootScope.UserInfo.appsettings.system_customfields || false;

            if ($scope.descriptioncodes == null)
                $scope.descriptioncodes = '';
            if ($scope.descriptionname == null)
                $scope.descriptionname = '';

            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                if ($scope.mfg_fabricbasedorder)
                    $scope.panelheight.height = '' + (available - 230) + 'px';
                else
                    $scope.panelheight.height = '' + (available - 210) + 'px';
            }


            if ($scope.system_customfields) {
                restService.Get('ReferencelistService/FULL-CUSTOMFIELDS', true).then(function (results) {
                    var fields = [];
                    $scope.Entity.CustomFields = [];
                    $scope.SKU.CustomFields = [];
                    $.each(results, function (k, v) {
                        if (v.additionalinfo &&
                            v.additionalinfo.customfield &&
                            v.additionalinfo.customfield.cfbuilt) {
                            $scope.Entity.CustomFields.push({
                                'key': v.additionalinfo.customfield.cfname,
                                'value': '',
                            });
                            $scope.SKU.CustomFields.push({
                                'key': v.additionalinfo.customfield.cfname,
                                'value': '',
                            });
                            var fieldname = 'SKU.CustomFields[' + k + '].value';
                            var topdiv = '<div class="col-md-6 no-padding">';
                            switch (v.referenceid) {
                                case 1:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<input type="text" class="input-sm form-control" ng-model="' + fieldname + '"></div>');
                                    break;
                                case 2:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<input type="number" class="input-sm form-control" ng-model="' + fieldname + '"></div>');
                                    break;
                                case 3:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<input type="text" class="form-control input-sm datepicker" datemodule="' + fieldname + '" ng-model="' + fieldname + '"></div>');
                                    break;
                                case 4:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in CustomReference_' + v.id + ' track by item.id"></select></div>');
                                    $scope.UpdateCustomReferenceList(v, 'CustomReference_' + v.id);
                                    break;
                                case 5:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in ColorList track by item.id"></select></div>');
                                    break;
                                case 6:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in FullSizeList track by item.id"></select></div>');
                                    break;
                                case 7:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in FitList track by item.id"></select></div>');
                                    break;
                                case 8:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in TypeList track by item.id"></select></div>');
                                    break;
                                case 9:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in Material track by item.id"></select></div>');
                                    break;
                                case 10:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in Styles track by item.id"></select></div>');
                                    break;
                                case 11:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in Patterns track by item.id"></select></div>');
                                    break;
                                case 12:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in SeasonList track by item.id"></select></div>');
                                    break;
                                case 13:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in Classification track by item.id"></select></div>');
                                    break;
                                case 14:
                                    fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                    fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in ProductDivision track by item.id"></select></div>');
                                    break;

                                //            { 'name': 'SEASON', 'code': 'SEASON' },
                                //{ 'name': 'PRODUCT DIVISION', 'code': 'PRODUCTDIVISION' },
                                //{ 'name': 'CLASSIFICATION', 'code': 'CLASSIFICATION' }
                            }
                        }
                    });
                    angular.element($("#customefields")).append($compile(fields.join(''))($scope));
                });
            }

            $scope.SKUEditConfig();
        }
        $scope.Locations = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
            $scope.ColorList = results;
            $.each(results, function (k, v) {
                v.displayname = v.name;
            });
        });
        $scope.Patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Patterns = results;
        });
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });
        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Styles = results;
        });
        $scope.SeasonList = restService.Get('ReferencelistService/SEASON', true).then(function (results) {
            $scope.SeasonList = results;
        });
        $scope.Classification = restService.Get('ReferencelistService/CLASSIFICATION', true).then(function (results) {
            $scope.Classification = results;
        });
        $scope.ProductDivision = restService.Get('ReferencelistService/PRODUCTDIVISION', true).then(function (results) {
            $scope.ProductDivision = results;
        });
        $scope.FullSizeNumbers = [];
        $scope.SizeGroupList = [];
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
            $scope.Sizes = results;
            $scope.FullSizeList = results;
            $.each(results, function (k, v) {
                var sizenumber = parseInt(v.code);
                if (!isNaN(sizenumber)) {
                    $scope.FullSizeNumbers.push(sizenumber);
                }
            });
            $scope.FullSizeNumbers.sort();
            $scope.SizeGroupList = restService.Get('ReferencelistService/FULL-SIZEGROUP', true).then(function (results) {
                $scope.SizeGroupList = results;
                $scope.FullSizeGroupList = results;
                $.each(results, function (k, v) {
                    if (v.additionalinfo &&
                        v.additionalinfo.sizegroups &&
                        v.additionalinfo.sizegroups.length > 0) {
                        v.possizelist = [];
                        $.each(v.additionalinfo.sizegroups, function (kk, vv) {
                            var size = $scope.SizeList.find(function (item) {
                                return item.id == vv
                            });
                            if (size && size != null) {
                                v.possizelist.push(size.name);
                            }
                        });
                        v.possizelist = v.possizelist.join(',');
                    }
                })
            });
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            results.push({ 'id': -2, 'name': 'ALL COMBO PRODUCTS' });
            $scope.Products = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });
        $scope.Fabrics = restService.Get('ReferencelistService/FABRIC', true).then(function (results) {
            $scope.Fabrics = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });
        $scope.TaxList = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.TaxList = results;
        });

        $scope.SleeveList = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.SleeveList = results;
        });
        $scope.FitList = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.FitList = results;
        });
        $scope.UOMList = restService.Get('ReferencelistService/UOM', true).then(function (results) {
            $scope.UOMList = results;
        });
        $scope.Workflows = restService.Get('ReferencelistService/MFGWOFLOW', true).then(function (results) {
            $scope.Workflows = results;
        });
        $scope.MfgStages = restService.Get('ReferencelistService/MFGSTAGES', true).then(function (results) {
            $scope.MfgStages = results;
        });
        $scope.BOMCategories = restService.Get('ReferencelistService/FULL-BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategories = results;
        });
        $scope.BOMAllList = restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMAllList = results;
        });
        $scope.BoxList = [];
        restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
            $.each(results, function (k, v) {
                if (v.additionalinfo && v.additionalinfo.paytype == 'BOX') {
                    $scope.BoxList.push({ 'id': v.id, 'name': v.name, 'add': v.additionalinfo.maxlimit, 'remove': v.additionalinfo.billdiscount });
                }
            })
        });
        $scope.JobCharges = restService.Get('ReferencelistService/FULL-JOBCHARGE', true).then(function (results) {
            $scope.JobCharges = results;
        });

        $scope.Entity = {
            id: 0,
            sizemode: 0,
            name: "",
            code: "",
            printingname: "",
            fitid: "",
            seasonid: "",
            "productid": 0,
            "brandid": 0,
            uomid: "",
            mfgmonth: '',
            mfgyear: '',
            "sizeid": 0,
            "patternid": 0,
            "styleid": 0,
            "colorid": 0,
            "typeid": 0,
            "materialid": 0,
            "pack": "",
            "sleeveid": 0,
            "key": '',
            "sizegroup": { "items": [], "ditems": [] },
            isactive: true,
            createdon: new Date(),
            autonumber: 0,
            version: 1,
            jobcharge: ''
        };

        $scope.SKU = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.SKU.sizegroup.items = [];


        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;

            if (!$scope.ShowSearch && $scope.egrid == null) {
                $timeout(function () {
                    $scope.SKUEditConfig();
                }, 50);
            }
        }


        $scope.imageuploaded = function (value) {
            $scope.SKU.imageid = value[0];
            $scope.SKU.imagelist = value;
            $scope.SKU.timageid = null;
        }

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            code: "",
            name: "",
            fromdate: "",
            todate: ""
        }

        $scope.ConsumptionTypes = [{ id: 1, name: 'Material' }
            , { id: 2, name: 'Charges' }
        ];

        $scope.ConsumptionModes = [{ id: 1, name: 'Colour Based' }
            , { id: 2, name: 'Size Based' }
            , { id: 3, name: 'Colour/Size Based' }
            , { id: 5, name: 'Same Colour' }
        ];

        $scope.ImageStatus = [{ id: 0, name: 'All' }
            , { id: 1, name: 'Available' },
        { id: 2, name: 'Not Available' }];

        $scope.CoreStatus = [{ id: 0, name: 'All' }
            , { id: 1, name: 'Core' },
        { id: 2, name: 'Non Core' }];

        $scope.StatusList = [{ id: 1, name: 'Unapproved' }
            , { id: 2, name: 'Approved' },
        { id: 3, name: 'Deactivated' }];

        $scope.SizeMode = [{ id: 0, name: 'None' }, { id: 1, name: 'Size' }
            , { id: 2, name: 'Range' },
        { id: 3, name: 'Group' }];

        $scope.StyleMode = [{ id: 0, name: 'No' }, { id: 1, name: 'Yes' }];


        $scope.New = function () {
            $scope.SKU = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.EditSKU($scope.SKU);
            $scope.ErrorMessage = "";
        }
        $scope.SaveRequest = false;
        $scope.Save = function () {

            if (typeof ($scope.SKU.code) == 'undefined' || $scope.SKU.code == '') {
                $rootScope.Toaster.Error('Please enter the code');
                return;
            }

            if (typeof ($scope.SKU.productid) == 'undefined' || $scope.SKU.productid == null || $scope.SKU.productid == '') {
                $rootScope.Toaster.Error('Please enter the product');
                return;
            }

            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;
            if ($scope.SKU.processlist && $scope.SKU.processlist != null)
                $scope.SKU.processlist = null;
            var request = {
                "Item": $scope.SKU,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgdesignService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("SKU Saved Successfully.!");
                    $scope.SKU = result;
                    $scope.EditSKU($scope.SKU);
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
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
                                // var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "MfgdesignService/Delete",
                                    data: request
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.igrid.UpdateData($scope.SearchResult);
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

                restService.Get('MfgdesignService/Search/' + item.id, true).then(function (results) {
                    $scope.SKU = results;
                    $scope.EditSKU(item);
                });
                //$scope.SKU = item;
            }
        }

        $scope.EditSKU = function (item) {
            $scope.fabricchanged();
            $scope.SKU.sizegroup.ditems = [];
            if ($scope.SKU.imagelist && $scope.SKU.imagelist != null && $scope.SKU.imagelist.length > 0) {
                $scope.SKU.imageid = $scope.SKU.imagelist[0];
            }

            if ($scope.SKU.sizegroup &&
                $scope.SKU.sizegroup != null &&
                $scope.SKU.sizegroup.items &&
                $scope.SKU.sizegroup.items != null) {
                $.each($scope.SKU.sizegroup.items, function (k, v) {
                    v.id = k;
                });
            }
            $.each($scope.Sizes, function (k, v) {
                v.selected = false;
                if ($scope.SKU.sizegroup != null) {
                    var found = $scope.SKU.sizegroup.items.find(
                        function (el) {
                            return el.id == v.id;
                        }
                    );
                    v.selected = found != null;
                    if (v.selected)
                        v.ro = found.ro;
                }
                else
                    $scope.SKU.sizegroup = { items: [] };
            });
            $scope.ShowPrint = false;
            $scope.SKUEditUpdate();
        }
        $scope.FindItem = function (searchValue, listValues) {
            if (typeof (searchValue) == 'undefined') return null;
            var found = listValues.find(function (arritem) {
                return arritem.id == searchValue;
            });
            if (typeof (found) == 'undefined' || found == null) {
                return null;
            }
            return found;
        }

        $scope.DeleteList = function (item, list) {
            if (item.id && item.id > 0)
                $scope.SKU.sizegroup.ditems.push(item);
            list.splice(list.indexOf(item), 1);
        }

        $scope.fabricchanged = function () {

            if ($scope.system_isb2bapplication) {
                $scope.ColorList = $scope.Colors;
                return;
            }

            var fabric = $scope.Fabrics.find(function (item) {
                return item.id == $scope.SKU.fabricid;
            });

            if (fabric && fabric != null) {

                if ($scope.mfg_fabricbasedorder &&
                    $rootScope.UserInfo.appsettings.mfg_designcode == '[FABRICCODE]') {
                    if (typeof ($scope.SKU.id) == 'undefined' || $scope.SKU.id <= 0) {
                        $scope.SKU.code = fabric.name;
                        $scope.SKU.name = fabric.name;
                    }
                }

                var colourlist = [];
                if (fabric.colourmode == 1) {
                    colourlist.push({
                        'id': fabric.colourid,
                        'code': '',
                        'name': $scope.SearchRef(fabric.colourid, $scope.Colors)
                    });
                }
                else {
                    $.each(fabric.colourlist, function (k, v) {
                        colourlist.push({
                            'id': v.id,
                            'code': v.name,
                            'displayname': v.name + '-' + $scope.SearchRef(v.id, $scope.Colors),
                            'name': $scope.SearchRef(v.id, $scope.Colors)
                        });
                    });
                }
                if (colourlist == undefined || colourlist == null || colourlist.length == 0)
                    $scope.ColorList = $scope.Colors;
                else
                    $scope.ColorList = colourlist;
            }
            else {
                $scope.ColorList = $scope.Colors;
            }
        }

        $scope.colourchanged = function () {
            var selectedcolour = $scope.ColorList.find(function (item) {
                return item.displayname == $scope.SKU.colourdisplayname;
            });

            $scope.SKU.colourid = 0;
            $scope.colourid = 0;
            if (selectedcolour && selectedcolour != null) {
                $scope.SKU.colourid = selectedcolour.id;
                $scope.colourid = selectedcolour.id;
            }
        }

        $scope.ManageSize = function () {

            var stylecode = typeof ($scope.stylecode) == 'undefined' || $scope.stylecode == null ? '' : $scope.stylecode;
            var skucodes = typeof ($scope.skucodes) == 'undefined' || $scope.skucodes == null ? '' : $scope.skucodes;
            var descriptioncodes = typeof ($scope.descriptioncodes) == 'undefined' || $scope.descriptioncodes == null ? '' : $scope.descriptioncodes;
            var descriptionname = typeof ($scope.descriptionname) == 'undefined' || $scope.descriptionname == null ? '' : $scope.descriptionname;
            var eancode = typeof ($scope.mfg_eancode) == 'undefined' || $scope.mfg_eancode == null ? '' : $scope.mfg_eancode;;

            var Details = {};

            var mfgmonth = parseInt($scope.SKU.mfgmonth);
            var mfgyear = parseInt($scope.SKU.mfgyear);
            if (isNaN(mfgyear))
                mfgyear = new Date().getFullYear();

            if (isNaN(mfgmonth))
                mfgmonth = new Date().getMonth();

            var mfgdate = new Date(mfgyear, mfgmonth, 1);

            if (mfgdate.toString() == 'Invalid Date')
                mfgdate = new Date();

            var year = $filter('date')(mfgdate, 'yy');
            var month = $filter('date')(mfgdate, 'MMM').toUpperCase().substring(0, 2);

            if ($rootScope.UserInfo.appsettings.numberencrypt &&
                $rootScope.UserInfo.appsettings.numberencrypt != null &&
                $rootScope.UserInfo.appsettings.numberencrypt.length == 10) {
                year = $rootScope.UserInfo.appsettings.numberencrypt[parseInt(year[0])] +
                    $rootScope.UserInfo.appsettings.numberencrypt[parseInt(year[1])]
            }



            Details.product = $scope.FindItem($scope.SKU.productid, $scope.Products);
            Details.style = $scope.FindItem($scope.SKU.styleid, $scope.Styles);
            Details.supplier = $scope.FindItem($scope.SKU.supplierid, $scope.Supplier);

            Details.colour = $scope.ColorList.find(function (citem) {
                return citem.displayname == $scope.SKU.colourdisplayname;
            });
            Details.pattern = $scope.FindItem($scope.SKU.patternid, $scope.Patterns);
            Details.sleeve = $scope.FindItem($scope.SKU.sleeveid, $scope.SleeveList);
            Details.fit = $scope.FindItem($scope.SKU.fitid, $scope.FitList);

            stylecode = stylecode.replace('[YY]', year);
            descriptioncodes = descriptioncodes.replace('[YY]', year);
            descriptionname = descriptionname.replace('YY', year);
            skucodes = skucodes.replace('[YY]', year);

            stylecode = stylecode.replace('[MM]', month);
            descriptioncodes = descriptioncodes.replace('[MM]', month);
            descriptionname = descriptionname.replace('MM', month);
            skucodes = skucodes.replace('[MM]', month);

            stylecode = stylecode.replace('[CODE]', $scope.SKU.code);
            descriptioncodes = descriptioncodes.replace('[CODE]', $scope.SKU.code);
            descriptionname = descriptionname.replace('CODE', $scope.SKU.code);

            stylecode = stylecode.replace('[NAME]', $scope.SKU.name);
            descriptioncodes = descriptioncodes.replace('[NAME]', $scope.SKU.name);
            descriptionname = descriptionname.replace('NAME', $scope.SKU.name);

            stylecode = stylecode.replace('[DESIGN]', $scope.SKU.code);
            descriptioncodes = descriptioncodes.replace('[DESIGN]', $scope.SKU.code);
            descriptionname = descriptionname.replace('DESIGN', $scope.SKU.code);

            eancode = eancode.replace('[DESIGN]', $scope.SKU.code);
            eancode = eancode.replace('[CODE]', $scope.SKU.code);

            if (Details.supplier != null) {
                descriptioncodes = descriptioncodes.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                descriptionname = descriptionname.replace('SUPPLIER', Details.supplier.name);
                skucodes = skucodes.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                eancode = eancode.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                stylecode = stylecode.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
            }
            if (Details.product != null) {
                descriptioncodes = descriptioncodes.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                descriptionname = descriptionname.replace('PRODUCT', Details.product.name);
                skucodes = skucodes.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                eancode = eancode.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                stylecode = stylecode.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
            }
            if (Details.sleeve != null) {
                descriptioncodes = descriptioncodes.replace('[SLEEVE]', Details.sleeve.code.replace(' ', ''));
                descriptionname = descriptionname.replace('SLEEVE', Details.sleeve.name);
                eancode = eancode.replace('[SLEEVE]', Details.sleeve.code.replace(' ', ''));
                stylecode = stylecode.replace('[SLEEVE]', Details.sleeve.code.replace(' ', ''));
            }
            if (Details.colour != null) {
                descriptioncodes = descriptioncodes.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
                descriptionname = descriptionname.replace('COLOUR', Details.colour.name);
                eancode = eancode.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
                stylecode = stylecode.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
            }
            if (Details.fit != null) {
                descriptioncodes = descriptioncodes.replace('[FIT]', Details.fit.code.replace(' ', ''));
                descriptionname = descriptionname.replace('FIT', Details.fit.name);
                eancode = eancode.replace('[FIT]', Details.fit.code.replace(' ', ''));
                stylecode = stylecode.replace('[FIT]', Details.fit.code.replace(' ', ''));
            }

            //if ($scope.SKU.id == 0 && !$scope.mfg_manualdesigncode) {
            //    $scope.SKU.code = skucodes;
            //    $scope.SKU.name = skucodes;
            //}

            var idList = [];

            if (typeof ($scope.SKU.sleeveid) == 'undefined' || $scope.SKU.sleeveid == null)
                $scope.SKU.sleeveid = 0;
            if (typeof ($scope.SKU.colourid) == 'undefined' || $scope.SKU.colourid == null)
                $scope.SKU.colourid = 0;

            var existList = [];

            var vv = $scope.Sizes.find(function (item) {
                return item.id == $scope.SKU.sizeid;
            });

            /*item.sizegroup == sizegroup.name &&
            item.sizeid == vv.id &&*/

            var existsize = $scope.SKU.sizegroup.items.find(function (item) {
                return
                ($scope.SKU.colourid == 0 || item.colourid == $scope.SKU.colourid) &&
                    ($scope.SKU.sleeveid == 0 || item.sleeveid == $scope.SKU.sleeveid);
            });

            var currentautonumber = 0;
            if (eancode.indexOf('AUTO') >= 0) {
                if (typeof (existsize) == 'undefined' || existsize == null ||
                    existsize.autonumber == null || existsize.autonumber == '') {
                    if ($scope.SKU.sizegroup.colouritems &&
                        $scope.SKU.sizegroup.colouritems != null &&
                        $scope.SKU.sizegroup.colouritems.length >= 0) {

                        var existcolour = $scope.SKU.sizegroup.colouritems.find(function (item) {
                            return item.colourid == $scope.colourid && item.styleid == $scope.styleid;
                        });
                        if (existcolour && existcolour != null) {
                            $scope.CreateSKUItem(
                                vv,
                                existcolour.autonumber,
                                descriptioncodes,
                                descriptionname,
                                stylecode,
                                eancode);
                            return;
                        }
                    }

                    $.confirm({
                        title: 'Auto Number',
                        content: 'Do you want to add a new product ' + eancode + "?",
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
                                    restService.Get('MfgdesignService/NextAutoNumber', true).then(function (result) {

                                        if ($scope.SKU.sizegroup.colouritems &&
                                            $scope.SKU.sizegroup.colouritems != null &&
                                            $scope.SKU.sizegroup.colouritems.length >= 0) {
                                            $scope.SKU.sizegroup.colouritems.push({
                                                'colourid': $scope.colourid,
                                                'styleid': $scope.styleid,
                                                'autonumber': result
                                            });
                                        }
                                        else {
                                            $scope.SKU.sizegroup.colouritems = [];
                                            $scope.SKU.sizegroup.colouritems.push({
                                                'colourid': $scope.colourid,
                                                'styleid': $scope.styleid,
                                                'autonumber': result
                                            });
                                        }

                                        $scope.CreateSKUItem(
                                            vv,
                                            result,
                                            descriptioncodes,
                                            descriptionname,
                                            stylecode,
                                            eancode);
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
                            this.$content.find('form').on('submit', function (e) {
                                // if the user submits the form by pressing enter in the field.
                                e.preventDefault();
                                jc.$$formSubmit.trigger('click'); // reference the button and click it
                            });
                        }
                    });

                    return;
                }
                else {
                    currentautonumber = existsize.autonumber;
                }
            }
            $scope.CreateSKUItem(vv,
                currentautonumber,
                descriptioncodes,
                descriptionname,
                stylecode,
                eancode);
        }

        $scope.GetColourIndex = function (colourid, styleid) {
            var list = [];
            var index = 1;
            if (typeof (styleid) == 'undefined' || styleid == null)
                styleid = 0;
            if (typeof (colourid) == 'undefined' || colourid == null)
                colourid = 0;

            $.each($scope.SKU.sizegroup.items, function (k, v) {
                var exist = list.find(function (item) {
                    return item.colourid == v.colourid && item.styleid == v.styleid;
                });
                if (exist && exist != null)
                    exist.count++;
                else {
                    list.push({
                        'colourid': v.colourid, 'styleid': v.styleid, 'count': 1,
                        'index': index++
                    });
                }
            });
            var exist = list.find(function (item) {
                return item.colourid == colourid && item.styleid == styleid;
            });
            if (exist && exist != null)
                return exist.index.toString();
            return (list.length + 1).toString();
        }

        $scope.CreateSKUItem = function (
            vv,
            currentautonumber,
            descriptioncodes,
            descriptionname,
            stylecode,
            eancode) {
            var autonumber = currentautonumber.toString().padStart(4, '0');
            var colourindex = $scope.GetColourIndex($scope.SKU.colourid);
            var escappedsize = vv.name.replace(/[^A-Z0-9]+/ig, "") //ESCAPEDSIZE

            newstylecode = stylecode.replace('AUTO', autonumber).replace('[COLOURINDEX]', colourindex);

            newdescriptioncodes = descriptioncodes.replace('[AUTO]', autonumber).replace('[ESCAPEDSIZE]', escappedsize);
            newdescriptionname = descriptionname.replace('AUTO', autonumber).replace('[ESCAPEDSIZE]', escappedsize);

            newdescriptioncodes = newdescriptioncodes.replace('[SIZE]', vv.code);
            newdescriptionname = newdescriptionname.replace('SIZE', vv.name);

            var neweancode = eancode.replace('[SIZE]', vv.code).replace('[ESCAPEDSIZE]', escappedsize).
                replace('[SIZEINDEX]', $scope.Sizes.indexOf(vv)).replace('[AUTO]', autonumber);

            newstylecode = stylecode.replace('[SIZE]', vv.code).replace('[ESCAPEDSIZE]', escappedsize);

            var existing = $scope.SKU.sizegroup.items.find(function (item) {
                return item.code == newdescriptioncodes &&
                    item.sizeid == vv.id;
            });

            if ($scope.mfg_manualdesigncode || typeof (existing) == "undefined" || existing == null) {
                $scope.SKU.sizegroup.items.push({
                    'id': $scope.SKU.sizegroup.items.length,
                    'sizeid': vv.id,
                    'code': $scope.mfg_manualdesigncode ? '' : newstylecode,
                    'colourid': $scope.SKU.colourid,
                    'sleeveid': $scope.SKU.sleeveid,
                    'name': newdescriptionname,
                    'sizegroup': '',
                    'alloworder': true,
                    'autonumber': currentautonumber,
                    'qty': $scope.SKU.sizeratio,
                    'cost': $scope.SKU.cost,
                    'wsrate': $scope.SKU.wsrate,
                    'dealerprice': $scope.SKU.dealerprice,
                    'salerate': $scope.SKU.salerate,
                    'mrp': $scope.SKU.mrp,
                    'jobcharge': $scope.SKU.jobcharge,
                    'uid': neweancode
                });
                $scope.SKUEditUpdate();
            }
            else {
                $rootScope.Toaster.Error(newdescriptioncodes + ' code duplicate.');
            }
        }


        $scope.ManageSizeGroup = function () {

            var existsize = $scope.SKU.sizegroup.items.find(function (item) {
                return ($scope.SKU.colourid == 0 || item.colourid == $scope.SKU.colourid) &&
                    ($scope.SKU.sleeveid == 0 || item.sleeveid == $scope.SKU.sleeveid);
            });

            if (typeof (existsize) == 'undefined' || existsize == null ||
                existsize.autonumber == null || existsize.autonumber == '') {

                var mfgmonth = parseInt($scope.SKU.mfgmonth);
                var mfgyear = parseInt($scope.SKU.mfgyear);
                if (isNaN(mfgyear))
                    mfgyear = new Date().getFullYear();

                if (isNaN(mfgmonth))
                    mfgmonth = new Date().getMonth();

                var mfgdate = new Date(mfgyear, mfgmonth, 1);

                if (mfgdate.toString() == 'Invalid Date')
                    mfgdate = new Date();

                var year = $filter('date')(mfgdate, 'yy');
                var month = $filter('date')(mfgdate, 'MMM').toUpperCase().substring(0, 2);

                var Details = {};
                Details.product = $scope.FindItem($scope.SKU.productid, $scope.Products);
                Details.supplier = $scope.FindItem($scope.SKU.supplierid, $scope.Supplier);

                Details.colour = $scope.ColorList.find(function (citem) {
                    return citem.displayname == $scope.SKU.colourdisplayname;
                });
                Details.pattern = $scope.FindItem($scope.SKU.patternid, $scope.Patterns);
                Details.sleeve = $scope.FindItem($scope.SKU.sleeveid, $scope.SleeveList);
                Details.fit = $scope.FindItem($scope.SKU.fitid, $scope.FitList);

                var descriptionname = typeof ($scope.descriptionname) == 'undefined' || $scope.descriptionname == null ? '' : $scope.descriptionname;;
                descriptionname = descriptionname.replace('YY', year);
                descriptionname = descriptionname.replace('MM', month);
                descriptionname = descriptionname.replace('CODE', $scope.SKU.code);
                descriptionname = descriptionname.replace('NAME', $scope.SKU.name);
                descriptionname = descriptionname.replace('DESIGN', $scope.SKU.code);
                if (Details.supplier)
                    descriptionname = descriptionname.replace('SUPPLIER', Details.supplier.name);
                if (Details.product)
                    descriptionname = descriptionname.replace('PRODUCT', Details.product.name);
                if (Details.sleeve)
                    descriptionname = descriptionname.replace('SLEEVE', Details.sleeve.name);
                if (Details.colour)
                    descriptionname = descriptionname.replace('COLOUR', Details.colour.name);
                if (Details.fit)
                    descriptionname = descriptionname.replace('FIT', Details.fit.name);

                if ($scope.SKU.sizegroup.colouritems &&
                    $scope.SKU.sizegroup.colouritems != null &&
                    $scope.SKU.sizegroup.colouritems.length >= 0) {

                    var existcolour = $scope.SKU.sizegroup.colouritems.find(function (item) {
                        return item.colourid == $scope.colourid && item.styleid == $scope.styleid;
                    });
                    if (existcolour && existcolour != null) {
                        $scope.CreateSizeGroupItems(existcolour.autonumber);
                        return;
                    }
                }

                $.confirm({
                    title: 'Auto Number',
                    content: 'Do you want to add a new product ' + descriptionname + "?",
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
                                restService.Get('MfgdesignService/NextAutoNumber', true).then(function (result) {
                                    if ($scope.SKU.sizegroup.colouritems &&
                                        $scope.SKU.sizegroup.colouritems != null &&
                                        $scope.SKU.sizegroup.colouritems.length >= 0) {
                                        $scope.SKU.sizegroup.colouritems.push({
                                            'colourid': $scope.colourid,
                                            'styleid': $scope.styleid,
                                            'autonumber': result
                                        });
                                    }
                                    else {
                                        $scope.SKU.sizegroup.colouritems = [];
                                        $scope.SKU.sizegroup.colouritems.push({
                                            'colourid': $scope.colourid,
                                            'styleid': $scope.styleid,
                                            'autonumber': result
                                        });
                                    }
                                    $scope.CreateSizeGroupItems(result);
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
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });
            }
            else {
                $scope.CreateSizeGroupItems(existsize.autonumber);
            }
        }

        $scope.CreateSizeGroupItems = function (currentautonumber) {

            var sizegroup = $scope.SizeGroupList.find(function (item) {
                return item.id == $scope.SKU.sizegroupid;
            });
            var SizeIdList = [];
            if (sizegroup && sizegroup.list) {
                $.each(sizegroup.list, function (k, v) {

                    var size = $scope.Sizes.find(function (item) {
                        return item.id == v.id;
                    });

                    SizeIdList.push({
                        'id': size.id, 'name': size.name, 'code': size.code, 'index': k
                    });
                });
            }

            SizeIdList.sort(function (a, b) {
                return a.index - b.index;
            });

            var skucodes = $scope.skucodes;
            var descriptioncodes = $scope.descriptioncodes;
            var descriptionname = $scope.descriptionname;
            var eancode = $scope.mfg_eancode;
            var stylecode = $scope.stylecode == null ? '' : $scope.stylecode;
            var Details = {};
            var mfgmonth = parseInt($scope.SKU.mfgmonth);
            var mfgyear = parseInt($scope.SKU.mfgyear);
            if (isNaN(mfgyear))
                mfgyear = new Date().getFullYear();

            if (isNaN(mfgmonth))
                mfgmonth = new Date().getMonth();

            var mfgdate = new Date(mfgyear, mfgmonth, 1);

            if (mfgdate.toString() == 'Invalid Date')
                mfgdate = new Date();

            var year = $filter('date')(mfgdate, 'yy');
            var month = $filter('date')(mfgdate, 'MMM').toUpperCase().substring(0, 2);

            if ($rootScope.UserInfo.appsettings.numberencrypt &&
                $rootScope.UserInfo.appsettings.numberencrypt != null &&
                $rootScope.UserInfo.appsettings.numberencrypt.length == 10) {
                year = $rootScope.UserInfo.appsettings.numberencrypt[parseInt(year[0])] +
                    $rootScope.UserInfo.appsettings.numberencrypt[parseInt(year[1])]
            }

            Details.product = $scope.FindItem($scope.SKU.productid, $scope.Products);
            Details.style = $scope.FindItem($scope.SKU.styleid, $scope.Styles);
            Details.supplier = $scope.FindItem($scope.SKU.supplierid, $scope.Supplier);

            Details.colour = $scope.ColorList.find(function (citem) {
                return citem.displayname == $scope.SKU.colourdisplayname;
            });
            Details.pattern = $scope.FindItem($scope.SKU.patternid, $scope.Patterns);
            Details.sleeve = $scope.FindItem($scope.SKU.sleeveid, $scope.SleeveList);
            Details.fit = $scope.FindItem($scope.SKU.fitid, $scope.FitList);

            Details.fabric = $scope.FindItem($scope.SKU.fabricid, $scope.Fabrics);
            Details.fabriccolour = $scope.FindItem($scope.SKU.colourid, $scope.ColorList);

            descriptioncodes = descriptioncodes.replace('[YY]', year);
            descriptionname = descriptionname.replace('YY', year);
            skucodes = skucodes.replace('[YY]', year);
            stylecode = stylecode.replace('[YY]', year);

            descriptioncodes = descriptioncodes.replace('[MM]', month);
            descriptionname = descriptionname.replace('MM', month);
            skucodes = skucodes.replace('[MM]', month);
            stylecode = stylecode.replace('[MM]', year);


            if (Details.fabric != null) {
                descriptioncodes = descriptioncodes.replace('[FABRICCODE]', Details.fabric.name);
                stylecode = stylecode.replace('[FABRICCODE]', Details.fabric.name);
                eancode = eancode.replace('[FABRICCODE]', Details.fabric.name);
            }


            if (Details.fabriccolour != null) {
                var fabriccolourcode = Details.fabriccolour.code;
                if (fabriccolourcode.indexOf('/') > 0) {
                    fabriccolourcode = fabriccolourcode.substring(fabriccolourcode.indexOf('/'));
                }
                descriptioncodes = descriptioncodes.replace('[FABRICCOLOURCODE]', fabriccolourcode);
                stylecode = stylecode.replace('[FABRICCOLOURCODE]', fabriccolourcode);
                eancode = eancode.replace('[FABRICCOLOURCODE]]', fabriccolourcode);
            }

            stylecode = stylecode.replace('[CODE]', $scope.SKU.code);
            descriptioncodes = descriptioncodes.replace('[CODE]', $scope.SKU.code);
            descriptionname = descriptionname.replace('CODE', $scope.SKU.code);

            stylecode = stylecode.replace('[NAME]', $scope.SKU.name);
            descriptioncodes = descriptioncodes.replace('[NAME]', $scope.SKU.name);
            descriptionname = descriptionname.replace('NAME', $scope.SKU.name);

            stylecode = stylecode.replace('[SIZESET]', sizegroup.name);
            descriptioncodes = descriptioncodes.replace('[SIZESET]', sizegroup.name);
            descriptionname = descriptionname.replace('SIZESET', sizegroup.name);

            stylecode = stylecode.replace('[DESIGN]', $scope.SKU.code);
            descriptioncodes = descriptioncodes.replace('[DESIGN]', $scope.SKU.code);
            descriptionname = descriptionname.replace('DESIGN', $scope.SKU.code);

            eancode = eancode.replace('[SIZESET]', sizegroup.name);
            eancode = eancode.replace('[DESIGN]', $scope.SKU.code);
            eancode = eancode.replace('[CODE]', $scope.SKU.code);

            if (Details.supplier != null) {
                descriptioncodes = descriptioncodes.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                descriptionname = descriptionname.replace('SUPPLIER', Details.supplier.name);
                skucodes = skucodes.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                eancode = eancode.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                stylecode = stylecode.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
            }
            if (Details.product != null) {
                descriptioncodes = descriptioncodes.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                descriptionname = descriptionname.replace('PRODUCT', Details.product.name);
                skucodes = skucodes.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                eancode = eancode.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                stylecode = stylecode.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
            }
            if (Details.sleeve != null) {
                descriptioncodes = descriptioncodes.replace('[SLEEVE]', Details.sleeve.code.replace(' ', ''));
                descriptionname = descriptionname.replace('SLEEVE', Details.sleeve.name);
                eancode = eancode.replace('[SLEEVE]', Details.sleeve.code.replace(' ', ''));
                stylecode = stylecode.replace('[SLEEVE]', Details.sleeve.code.replace(' ', ''));
            }
            if (Details.colour != null) {
                descriptioncodes = descriptioncodes.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
                descriptionname = descriptionname.replace('COLOUR', Details.colour.name);
                eancode = eancode.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
                stylecode = stylecode.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
            }
            if (Details.fit != null) {
                descriptioncodes = descriptioncodes.replace('[FIT]', Details.fit.code.replace(' ', ''));
                descriptionname = descriptionname.replace('FIT', Details.fit.name);
                eancode = eancode.replace('[FIT]', Details.fit.code.replace(' ', ''));
                stylecode = stylecode.replace('[FIT]', Details.fit.code.replace(' ', ''));
            }

            //if ($scope.SKU.id == 0 && !$scope.mfg_manualdesigncode) {
            //    $scope.SKU.code = skucodes;
            //    $scope.SKU.name = skucodes;
            //}

            var idList = [];

            if (typeof ($scope.SKU.sleeveid) == 'undefined' || $scope.SKU.sleeveid == null)
                $scope.SKU.sleeveid = 0;
            if (typeof ($scope.SKU.colourid) == 'undefined' || $scope.SKU.colourid == null)
                $scope.SKU.colourid = 0;

            var skippeditem = 0;
            var addeditem = 0;
            var existList = [];
            var autonumber = currentautonumber.toString().padStart(4, '0');
            var colourindex = $scope.GetColourIndex($scope.SKU.colourid);
            stylecode = stylecode.replace('AUTO', autonumber).replace('[COLOURINDEX]', colourindex);
            descriptioncodes = descriptioncodes.replace('[AUTO]', autonumber);
            descriptionname = descriptionname.replace('AUTO', autonumber);
            descriptioncodes = descriptioncodes.replace('[COLOURID]', $scope.SKU.colourid);
            descriptionname = descriptionname.replace('COLOURID', $scope.SKU.colourid);
            stylecode = stylecode.replace('[COLOURID]', $scope.SKU.colourid);

            $.each(SizeIdList, function (kk, vv) {

                var newdescriptionname = descriptionname;
                var newdescriptioncodes = descriptioncodes;

                var escappedsize = vv.name.replace(/[^A-Z0-9]+/ig, "") //ESCAPEDSIZE

                newstylecode = stylecode.replace('[SIZE]', vv.name);
                newdescriptioncodes = newdescriptioncodes.replace('[SIZE]', vv.code).replace('[ESCAPEDSIZE]', escappedsize);
                newdescriptionname = newdescriptionname.replace('SIZE', vv.name);

                var neweancode = eancode.replace('[SIZE]', vv.code).replace('[ESCAPEDSIZE]', escappedsize).
                    replace('[SIZEINDEX]', $scope.Sizes.indexOf(vv)).replace('[AUTO]', autonumber);

                newstylecode = stylecode.replace('[SIZE]', vv.code).replace('[ESCAPEDSIZE]', escappedsize);

                var existing = $scope.SKU.sizegroup.items.find(function (item) {
                    return item.code == newdescriptioncodes &&
                        item.sizeid == vv.id;
                });

                if ($scope.mfg_manualdesigncode || typeof (existing) == "undefined" || existing == null) {
                    $scope.SKU.sizegroup.items.push({
                        'id': $scope.SKU.sizegroup.items.length,
                        'sizeid': vv.id,
                        'code': $scope.mfg_manualdesigncode ? '' : newstylecode,
                        'colourid': $scope.SKU.colourid,
                        'sleeveid': $scope.SKU.sleeveid,
                        'name': newdescriptionname,
                        'sizegroup': sizegroup.name,
                        'qty': $scope.SKU.sizeratio,
                        'cost': $scope.SKU.cost,
                        'wsrate': $scope.SKU.wsrate,
                        'dealerprice': $scope.SKU.dealerprice,
                        'salerate': $scope.SKU.salerate,
                        'mrp': $scope.SKU.mrp,
                        'jobcharge': $scope.SKU.jobcharge,
                        'autonumber': currentautonumber,
                        'uid': neweancode,
                        'alloworder': true
                    });
                }
                else {
                    existList.push(newdescriptioncodes);
                }

            });
            $scope.SKUEditUpdate();
            if (existList.length > 0) {
                $rootScope.Toaster.Error(existList.join(' , ') + ' code duplicate.');
            }
        }

        $scope.GenerateUID = function () {
            var Details = {};

            var mfgmonth = parseInt($scope.SKU.mfgmonth);
            var mfgyear = parseInt($scope.SKU.mfgyear);
            if (isNaN(mfgyear))
                mfgyear = new Date().getFullYear();

            if (isNaN(mfgmonth))
                mfgmonth = new Date().getMonth();

            var mfgdate = new Date(mfgyear, mfgmonth, 1);

            var year = $filter('date')(mfgdate, 'yy');
            var month = $filter('date')(mfgdate, 'MMM').toUpperCase().substring(0, 2);

            if (typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings.numberencrypt) != 'undefined' &&
                $rootScope.UserInfo.appsettings.numberencrypt != null &&
                $rootScope.UserInfo.appsettings.numberencrypt.length == 10) {
                year = $rootScope.UserInfo.appsettings.numberencrypt[parseInt(year[0])] +
                    $rootScope.UserInfo.appsettings.numberencrypt[parseInt(year[1])]
            }

            Details.product = $scope.FindItem($scope.SKU.productid, $scope.Products);
            Details.style = $scope.FindItem($scope.SKU.styleid, $scope.Styles);
            Details.supplier = $scope.FindItem($scope.SKU.supplierid, $scope.Supplier);

            Details.colour = $scope.ColorList.find(function (citem) {
                return citem.displayname == $scope.SKU.colourdisplayname;
            });
            Details.pattern = $scope.FindItem($scope.SKU.patternid, $scope.Patterns);
            Details.sleeve = $scope.FindItem($scope.SKU.sleeveid, $scope.SleeveList);
            Details.fit = $scope.FindItem($scope.SKU.fitid, $scope.FitList);


            $.each($scope.SKU.sizegroup.items, function (k, v) {

                var skucodes = $scope.skucodes;
                var descriptioncodes = $scope.descriptioncodes;
                var descriptionname = $scope.descriptionname;
                var eancode = $scope.mfg_eancode;
                var mfgmonth = parseInt($scope.SKU.mfgmonth);
                var mfgyear = parseInt($scope.SKU.mfgyear);
                if (isNaN(mfgyear))
                    mfgyear = new Date().getFullYear();

                if (isNaN(mfgmonth))
                    mfgmonth = new Date().getMonth();

                var mfgdate = new Date(mfgyear, mfgmonth, 1);

                if (mfgdate.toString() == 'Invalid Date')
                    mfgdate = new Date();


                descriptioncodes = descriptioncodes.replace('[YY]', year);
                descriptionname = descriptionname.replace('YY', year);
                skucodes = skucodes.replace('[YY]', year);

                descriptioncodes = descriptioncodes.replace('[MM]', month);
                descriptionname = descriptionname.replace('MM', month);
                skucodes = skucodes.replace('[MM]', month);

                descriptioncodes = descriptioncodes.replace('[CODE]', $scope.SKU.code);
                descriptionname = descriptionname.replace('CODE', $scope.SKU.code);

                descriptioncodes = descriptioncodes.replace('[NAME]', $scope.SKU.name);
                descriptionname = descriptionname.replace('NAME', $scope.SKU.name);

                descriptioncodes = descriptioncodes.replace('[SIZESET]', v.sizegroup);
                descriptionname = descriptionname.replace('SIZESET', v.sizegroup);

                eancode = eancode.replace('[SIZESET]', v.sizegroup);
                eancode = eancode.replace('[DESIGN]', $scope.SKU.code);

                if (Details.supplier != null) {
                    descriptioncodes = descriptioncodes.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                    descriptionname = descriptionname.replace('SUPPLIER', Details.supplier.name);
                    skucodes = skucodes.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                    eancode = eancode.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                }
                if (Details.product != null) {
                    descriptioncodes = descriptioncodes.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                    descriptionname = descriptionname.replace('PRODUCT', Details.product.name);
                    skucodes = skucodes.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                    eancode = eancode.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                }
                if (Details.sleeve != null) {
                    descriptioncodes = descriptioncodes.replace('[SLEEVE]', Details.sleeve.code.replace(' ', ''));
                    descriptionname = descriptionname.replace('SLEEVE', Details.sleeve.name);
                    eancode = eancode.replace('[SLEEVE]', Details.sleeve.code.replace(' ', ''));
                }
                if (Details.colour != null) {
                    descriptioncodes = descriptioncodes.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
                    descriptionname = descriptionname.replace('COLOUR', Details.colour.name);
                    eancode = eancode.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
                }
                if (Details.fit != null) {
                    descriptioncodes = descriptioncodes.replace('[FIT]', Details.fit.code.replace(' ', ''));
                    descriptionname = descriptionname.replace('FIT', Details.fit.name);
                    eancode = eancode.replace('[FIT]', Details.fit.code.replace(' ', ''));
                }

                //if ($scope.SKU.id == 0 && !$scope.mfg_manualdesigncode) {
                //    $scope.SKU.code = skucodes;
                //    $scope.SKU.name = skucodes;
                //}

                var newdescriptionname = descriptionname;
                var newdescriptioncodes = descriptioncodes;


                if (descriptionname.indexOf('AUTO') > 0 || descriptioncodes.indexOf('AUTO') > 0) {
                    $scope.SKU.autonumber = $scope.SKU.autonumber + 1;
                }

                var autonumber = $scope.SKU.autonumber.toString().padStart(4, '0');
                newdescriptioncodes = descriptioncodes.replace('[AUTO]', autonumber);
                newdescriptionname = descriptionname.replace('AUTO', autonumber);

                var size = $scope.Sizes.find(function (item) {
                    return item.id == v.sizeid;
                });

                var escappedsize = size.name.replace(/[^A-Z0-9]+/ig, "") //ESCAPEDSIZE

                newdescriptioncodes = newdescriptioncodes.replace('[SIZE]', size.code).replace('[ESCAPEDSIZE]', escappedsize);
                newdescriptionname = newdescriptionname.replace('SIZE', size.name);
                newdescriptioncodes = newdescriptioncodes.replace('[DESIGN]', $scope.SKU.code).replace('[ESCAPEDSIZE]', escappedsize);
                newdescriptionname = newdescriptionname.replace('DESIGN', $scope.SKU.code);

                var neweancode = eancode.replace('[SIZE]', size.code).replace('[ESCAPEDSIZE]', escappedsize).
                    replace('[SIZEINDEX]', $scope.Sizes.indexOf(size)).replace('[AUTO]', autonumber);

                v.code = newdescriptioncodes;
                v.name = newdescriptionname;
                v.uid = neweancode;
                v.cost = $scope.SKU.cost;
                v.wsrate = $scope.SKU.wsrate;
                v.dealerprice = $scope.SKU.dealerprice;
                v.salerate = $scope.SKU.salerate;
                v.mrp = $scope.SKU.mrp;
                v.jobcharge = $scope.SKU.jobcharge;

            });
        }


        $scope.SKUComboConfig = function () {
            var dialog = ngDialog.open({
                template: 'SKUComboConfiguration',
                data: [$scope],
                width: '800px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {

                    $scope.SKUComboItemList = [];
                    $scope.SKUComboItem = {};
                    $scope.parent = $scope.ngDialogData[0];
                    var webrequest = {
                        "item": { 'id': $scope.parent.SKU.id },
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(webrequest);
                    $http({
                        method: 'POST',
                        url: "MfgdesignService/GetComboConfig",
                        data: srequest
                    }).success(function (result) {

                        if (result && result != null) {
                            $scope.noofproducts = result.qty;
                            $scope.sizeformat = result.sizeformat;
                            $scope.colourformat = result.colourformat;
                            $scope.SKUComboItemList = result.configlist;
                            if ($scope.SKUComboItemList == null) $scope.SKUComboItemList = [];
                        }

                    }).error(function (result) {
                    });


                    $scope.BarcodeModes = [{ id: 0, name: 'Single' }, { id: 1, name: 'Multiple' }, { id: 2, name: 'Design' }, { id: 3, name: 'Product' }];
                    $scope.AddProductConfig = function () {
                        if (this.SKUComboItemList.indexOf($scope.SKUComboItem) < 0)
                            $scope.SKUComboItemList.push(JSON.parse(JSON.stringify($scope.SKUComboItem)));
                        $scope.SKUComboItem = {};
                    };
                    $scope.DeleteConfigItem = function (item) {
                        this.SKUComboItemList.splice(this.SKUComboItemList.indexOf(item), 1);
                    }
                    $scope.EditConfigItem = function (item) {
                        $scope.SKUComboItem = item;
                    }
                    $scope.SaveConfig = function () {
                        $scope.closeThisDialog({ 'colourformat': $scope.colourformat, 'sizeformat': $scope.sizeformat, 'qty': $scope.noofproducts, 'configlist': $scope.SKUComboItemList });
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var request = {
                            'id': $scope.SKU.id, 'qty': data.value.qty,
                            'colourformat': data.value.colourformat, 'sizeformat': data.value.sizeformat,
                            'configlist': data.value.configlist
                        };


                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "MfgdesignService/SetComboConfig",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info('Attributes successfully changed.');
                            restService.Get('MfgdesignService/Search/' + $scope.SKU.id, true).then(function (results) {
                                $scope.SKU = results;
                                $scope.SKUEditConfig();
                                $scope.egrid.UpdateData($scope.SKU.sizegroup.items, '_id');
                            });

                        }).error(function (result) {
                            restService.Get('MfgdesignService/Search/' + $scope.SKU.id, true).then(function (results) {
                                $scope.SKU = results;
                                $scope.SKUEditConfig();
                                $scope.egrid.UpdateData($scope.SKU.sizegroup.items, '_id');
                            });
                        });
                    }
                }
            });
        }

        $scope.ShowPriceDialog = function () {

            var dialog = ngDialog.open({
                template: 'ChangePriceDialog',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.Actions = [{ 'id': 'Cost', 'name': 'Cost' },
                    { 'id': 'MRP', 'name': 'MRP' },
                    { 'id': 'Warehouse', 'name': 'Warehouse' },
                    { 'id': 'MrpAsWarehouse', 'name': 'Set Warehouse Price as MRP' },
                    { 'id': 'Wholesale', 'name': 'Set Retail Price as Wholesale Price' },
                    { 'id': 'Retail', 'name': 'Set Retail Price as Retail Price' },
                    { 'id': 'MrpAsRetail', 'name': 'Set Retail Price as MRP' }];
                    $scope.Update = function () {
                        $scope.closeThisDialog({ 'locationid': $scope.locationid, 'mode': $scope.mode });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var request = data.value;
                        request.ids = [];

                        $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                            request.ids.push(v.id);
                        });

                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "MfgdesignService/ChangePrice",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info(result + ' rows changed.');
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.ShowStatusDialog = function () {

            var dialog = ngDialog.open({
                template: 'ChangeStatusDialog',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.ChangeStock = function () {
                        $scope.closeThisDialog($scope.statusid);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var request = { 'statusid': data.value, ids: [] };

                        $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                            request.ids.push(v.id);
                        });

                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "MfgdesignService/ChangeStatus",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info('Attributes successfully changed.');
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.ShowClassificationDialog = function () {

            var dialog = ngDialog.open({
                template: 'ChangeClassDialog',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.ChangeStock = function () {
                        $scope.closeThisDialog($scope.classid);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var request = { 'classid': data.value, ids: [] };

                        $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                            request.ids.push(v.id);
                        });

                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "MfgdesignService/ChangeClass",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info('Attributes successfully changed.');
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.ShowImageDialog = function () {

            var dialog = ngDialog.open({
                template: 'ImageGalleryDialog',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];

                    if (typeof ($scope.parent.SKU.sizegroup.colouritems) == 'undefined' ||
                        $scope.parent.SKU.sizegroup.colouritems == null) {
                        $scope.parent.SKU.sizegroup.colouritems = [];
                    }

                    $scope.ImageList = [];
                    $.each($scope.parent.SKU.sizegroup.colouritems, function (k, v) {
                        $scope.ImageList.push({ 'name': v.name, 'colourid': v.colourid, 'styleid': v.styleid, 'imageid': v.imageid });
                    })
                    $.each($scope.parent.SKU.sizegroup.items, function (k, v) {

                        if (v.styleid == undefined) v.styleid = 0;
                        var imgexist = $scope.ImageList.find(function (item) {
                            return item.colourid == v.colourid && item.styleid == v.styleid;
                        })
                        if (imgexist == undefined || imgexist == null)
                            $scope.ImageList.push({ 'name': v.name, 'colourid': v.colourid, 'styleid': v.styleid });
                    })


                    $scope.imageLineuploaded = function (value, scope) {
                        scope.item.imageid = value[0];
                        scope.item.timageid = null;
                    }

                    $scope.ChangeStock = function () {
                        $scope.closeThisDialog($scope.ImageList);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {

                        $scope.SKU.sizegroup.colouritems = data.value;

                        var request = {
                            "Item": $scope.SKU,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "MfgdesignService/Save",
                            data: srequest
                        }).success(function (result) {
                            if (result != null && result.id > 0) {
                                $rootScope.Toaster.Info("SKU Saved Successfully.!");
                                $scope.SKU = result;
                                $scope.EditSKU($scope.SKU);
                            }
                            else {
                                $rootScope.Toaster.Error(result.ErrorMessage);
                            }
                        }).error(function (result) {
                        });

                    }
                }
            });
        }

        $scope.ShowChangeDialog = function () {

            var dialog = ngDialog.open({
                template: 'ChangeAttributeDialog',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.ChangeStock = function () {
                        $scope.closeThisDialog($scope.SearchCondition);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var request = { 'criteria': data.value };
                        request.sku = $scope.SKU;
                        request.items = $scope.egrid.GetSelectedRows();
                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "MfgdesignService/ChangeAtributes",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Error('Attributes successfully changed.');
                            restService.Get('MfgdesignService/Search/' + $scope.SKU.id, true).then(function (results) {
                                $scope.SKU = results;
                                $scope.EditSKU(item);
                            });
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.FocusSize = function () {
            if ($scope.SKU.sizemode == 3) {
                $scope.setFocussizegroupid();
            }
            else if ($scope.SKU.sizemode == 1) {
                $scope.setFocussizeid();
            }
            else if ($scope.SKU.sizemode == 2) {
                $scope.setFocussizerangeid();
            }
            else {
                $scope.setFocusdstyleid();
            }
        }

        $scope.ManageList = function (value, list) {

            var hasautonumber = false;
            var stylecode = typeof ($scope.stylecode) == 'undefined' || $scope.stylecode == null ? '' : $scope.stylecode;
            var skucodes = typeof ($scope.skucodes) == 'undefined' || $scope.skucodes == null ? '' : $scope.skucodes;
            var descriptioncodes = typeof ($scope.descriptioncodes) == 'undefined' || $scope.descriptioncodes == null ? '' : $scope.descriptioncodes;
            var descriptionname = typeof ($scope.descriptionname) == 'undefined' || $scope.descriptionname == null ? '' : $scope.descriptionname;
            hasautonumber = $scope.mfg_eancode.indexOf('AUTO') >= 0 ||
                skucodes.indexOf('AUTO') >= 0 ||
                stylecode.indexOf('AUTO') >= 0 ||
                descriptioncodes.indexOf('AUTO') >= 0 ||
                descriptionname.indexOf('AUTO') >= 0;

            if (!hasautonumber) {
                $scope.ManageListInternal(value, list, 0);
                return;
            }

            var available = list.find(function (item) {
                return item.colourid == $scope.SKU.colourid &&
                    item.sleeveid == $scope.SKU.sleeveid;
            });


            if (typeof (available) != 'undefined' &&
                typeof (available.autonumber) == 'undefined') {
                var eancode = typeof ($scope.mfg_eancode) == 'undefined' || $scope.mfg_eancode == null ? '' : $scope.mfg_eancode;
                if (eancode == '[AUTO]-[SIZE]' && available.uid &&
                    available.uid != null && available.uid != '') {
                    available.autonumber = available.uid.split('-')[0];
                }
            }

            var currentautonumber = 0;
            if (typeof (available) == 'undefined' || available == null ||
                available.autonumber == null || available.autonumber == '') {

                $.confirm({
                    title: 'Auto Number',
                    content: 'Do you want to add a new style ?',
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
                                restService.Get('MfgdesignService/NextAutoNumber', true).then(function (result) {
                                    if ($scope.SKU.sizegroup.colouritems &&
                                        $scope.SKU.sizegroup.colouritems != null &&
                                        $scope.SKU.sizegroup.colouritems.length >= 0) {
                                        $scope.SKU.sizegroup.colouritems.push({
                                            'colourid': $scope.colourid,
                                            'styleid': $scope.styleid,
                                            'autonumber': result
                                        });
                                    }
                                    else {
                                        $scope.SKU.sizegroup.colouritems = [];
                                        $scope.SKU.sizegroup.colouritems.push({
                                            'colourid': $scope.colourid,
                                            'styleid': $scope.styleid,
                                            'autonumber': result
                                        });
                                    }
                                    $scope.ManageListInternal(value, list, result)
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
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });
                return;
            }
            else {
                currentautonumber = available.autonumber;
                $scope.ManageListInternal(value, list, currentautonumber)
            }

        }

        $scope.ManageListInternal = function (value, list, currentautonumber) {

            if (typeof (value) == 'undefined' || value == null) return;

            var sizeidList = value.toString().split(',');
            var sizeList = sizeidList[0].split('-');
            if (sizeList.length == 2 && /^[a-zA-Z2-6]*$/.test(sizeList[0])) {


                var sizeIndex = '3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL'.split(',');
                var startsize = sizeIndex.indexOf(sizeList[0]);
                var endsize = sizeIndex.indexOf(sizeList[1]);
                var xxl = $scope.Sizes.find(function (item) {
                    return item.name == 'XXL';
                });
                if (typeof (xxl) == 'undefined' || xxl == null) {
                    startsize = -1;
                    endsize = -1;
                }
                if (startsize < 0 || endsize < 0) {
                    sizeIndex = '3XS,XXS,XS,S,M,L,XL,2XL,3XL,4XL,5XL,6XL,7XL'.split(',');
                    startsize = sizeIndex.indexOf(sizeList[0]);
                    endsize = sizeIndex.indexOf(sizeList[1]);
                }
                sizeList = [];
                for (var index = startsize; index <= endsize; index++) {
                    sizeList.push(sizeIndex[index]);
                }

            }
            else if (sizeList.length == 3) {

                var firstPart = sizeList[0].match(/[^-_ \d]+|\d+/g);
                var lastPart = sizeList[2].match(/[^-_ \d]+|\d+/g);
                var suffix = '';
                var startsize = parseInt(sizeList[0]);
                var increment = parseInt(sizeList[1]);
                var endsize = parseInt(sizeList[2]);

                if (firstPart.length == 2 && /^[a-zA-Z2-6]*$/.test(firstPart[1]))
                    suffix = firstPart[1];

                sizeList = [];
                for (; startsize <= endsize; startsize = startsize + increment) {
                    var searchsize = startsize.toString() + suffix;

                    var excluded = false;
                    for (var i = 1; i < sizeidList.length; i++) {
                        if (sizeidList[i] == searchsize)
                            excluded = true;
                    }
                    if (!excluded)
                        sizeList.push(searchsize);
                }
            }

            var eancode = typeof ($scope.mfg_eancode) == 'undefined' || $scope.mfg_eancode == null ? '' : $scope.mfg_eancode;
            var stylecode = typeof ($scope.stylecode) == 'undefined' || $scope.stylecode == null ? '' : $scope.stylecode;
            var skucodes = typeof ($scope.skucodes) == 'undefined' || $scope.skucodes == null ? '' : $scope.skucodes;
            var descriptioncodes = typeof ($scope.descriptioncodes) == 'undefined' || $scope.descriptioncodes == null ? '' : $scope.descriptioncodes;
            var descriptionname = typeof ($scope.descriptionname) == 'undefined' || $scope.descriptionname == null ? '' : $scope.descriptionname;

            var Details = {};
            var mfgmonth = parseInt($scope.SKU.mfgmonth);
            var mfgyear = parseInt($scope.SKU.mfgyear);
            if (isNaN(mfgyear))
                mfgyear = new Date().getFullYear();

            if (isNaN(mfgmonth))
                mfgmonth = new Date().getMonth();

            var mfgdate = new Date(mfgyear, mfgmonth, 1);
            var year = $filter('date')(mfgdate, 'yy');
            var month = $filter('date')(mfgdate, 'MMM').toUpperCase().substring(0, 2);

            if ($rootScope.UserInfo.appsettings.numberencrypt &&
                $rootScope.UserInfo.appsettings.numberencrypt != null &&
                $rootScope.UserInfo.appsettings.numberencrypt.length == 10) {
                year = $rootScope.UserInfo.appsettings.numberencrypt[parseInt(year[0])] +
                    $rootScope.UserInfo.appsettings.numberencrypt[parseInt(year[1])]
            }


            Details.product = $scope.FindItem($scope.SKU.productid, $scope.Products);
            Details.style = $scope.FindItem($scope.SKU.styleid, $scope.Styles);
            Details.supplier = $scope.FindItem($scope.SKU.supplierid, $scope.Supplier);

            Details.colour = $scope.ColorList.find(function (citem) {
                return citem.displayname == $scope.SKU.colourdisplayname;
            });
            Details.pattern = $scope.FindItem($scope.SKU.patternid, $scope.Patterns);
            Details.sleeve = $scope.FindItem($scope.SKU.sleeveid, $scope.SleeveList);
            Details.fit = $scope.FindItem($scope.SKU.fitid, $scope.FitList);
            Details.fabric = $scope.FindItem($scope.SKU.fabricid, $scope.Fabrics);
            Details.fabriccolour = $scope.FindItem($scope.SKU.colourid, $scope.ColorList);

            descriptioncodes = descriptioncodes.replace('[YY]', year);
            descriptionname = descriptionname.replace('YY', year);
            skucodes = skucodes.replace('[YY]', year);
            stylecode = stylecode.replace('[YY]', year);

            descriptioncodes = descriptioncodes.replace('[MM]', month);
            descriptionname = descriptionname.replace('MM', month);
            skucodes = skucodes.replace('[MM]', month);
            stylecode = stylecode.replace('[MM]', year);

            var colourindex = $scope.GetColourIndex($scope.SKU.colourid, $scope.SKU.styleid).replace('[COLOURINDEX]', colourindex);
            descriptioncodes = descriptioncodes.replace('[CODE]', $scope.SKU.code).replace('[COLOURINDEX]', colourindex);
            descriptionname = descriptionname.replace('CODE', $scope.SKU.code).replace('[COLOURINDEX]', colourindex);
            stylecode = stylecode.replace('[CODE]', $scope.SKU.code).replace('[COLOURINDEX]', colourindex);
            eancode = eancode.replace('[CODE]', $scope.SKU.code).replace('[COLOURINDEX]', colourindex);

            descriptioncodes = descriptioncodes.replace('[NAME]', $scope.SKU.name);
            descriptionname = descriptionname.replace('NAME', $scope.SKU.name);
            stylecode = stylecode.replace('[NAME]', $scope.SKU.name);

            var autonumber = currentautonumber.toString().padStart(4, '0');

            if (Details.fabric != null) {
                descriptioncodes = descriptioncodes.replace('[FABRICCODE]', Details.fabric.name);
                stylecode = stylecode.replace('[FABRICCODE]', Details.fabric.name);
                eancode = eancode.replace('[FABRICCODE]', Details.fabric.name);
            }


            if (Details.fabriccolour != null) {
                var fabriccolourcode = Details.fabriccolour.code;
                if (fabriccolourcode.indexOf('/') > 0) {
                    fabriccolourcode = fabriccolourcode.substring(fabriccolourcode.indexOf('/'));
                }
                descriptioncodes = descriptioncodes.replace('[FABRICCOLOURCODE]', fabriccolourcode);
                stylecode = stylecode.replace('[FABRICCOLOURCODE]', fabriccolourcode);
                eancode = eancode.replace('[FABRICCOLOURCODE]]', fabriccolourcode);
            }

            descriptioncodes = descriptioncodes.replace('[AUTO]', autonumber);
            descriptionname = descriptionname.replace('AUTO', autonumber);
            stylecode = stylecode.replace('[AUTO]', autonumber);
            eancode = eancode.replace('[AUTO]', autonumber);

            if (Details.supplier != null) {
                descriptioncodes = descriptioncodes.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                descriptionname = descriptionname.replace('SUPPLIER', Details.supplier.name);
                skucodes = skucodes.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                stylecode = stylecode.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
                eancode = eancode.replace('[SUPPLIER]', Details.supplier.code.replace(' ', ''));
            }
            if (Details.product != null) {
                descriptioncodes = descriptioncodes.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                descriptionname = descriptionname.replace('PRODUCT', Details.product.name);
                skucodes = skucodes.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                stylecode = stylecode.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
                eancode = eancode.replace('[PRODUCT]', Details.product.code.replace(' ', ''));
            }
            if (Details.sleeve != null) {
                descriptioncodes = descriptioncodes.replace('[SLEEVE]', Details.sleeve.code.replace(' ', ''));
                descriptionname = descriptionname.replace('SLEEVE', Details.sleeve.name);
                stylecode = stylecode.replace('SLEEVE', Details.sleeve.name);
                eancode = eancode.replace('SLEEVE', Details.sleeve.name);
            }
            if (Details.colour != null) {
                descriptioncodes = descriptioncodes.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
                descriptionname = descriptionname.replace('COLOUR', Details.colour.name);
                stylecode = stylecode.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
                eancode = eancode.replace('[COLOUR]', Details.colour.code.replace(' ', '')).replace("[ESCAPEDCOLOUR]", Details.colour.code.replace(/[^A-Z0-9]+/ig, ""));
            }
            if (Details.fit != null) {
                descriptioncodes = descriptioncodes.replace('[FIT]', Details.sleeve.code.replace(' ', ''));
                descriptionname = descriptionname.replace('FIT', Details.sleeve.name);
                stylecode = stylecode.replace('FIT', Details.sleeve.name);
                eancode = eancode.replace('FIT', Details.sleeve.name);
            }

            //if (!$scope.mfg_manualdesigncode && $scope.SKU.id == 0) {
            //    $scope.SKU.code = skucodes;
            //    $scope.SKU.name = skucodes;
            //}

            var idList = [];
            var existList = [];

            $.each($scope.Sizes, function (k, v) {
                $.each(sizeList, function (kk, vv) {
                    if (vv == v.name) {

                        var newdescriptionname = descriptionname;
                        var newdescriptioncodes = descriptioncodes;
                        var escappedsize = v.name.replace(/[^A-Z0-9]+/ig, "") //ESCAPEDSIZE

                        newdescriptioncodes = newdescriptioncodes.replace('[SIZE]', v.code).replace('[ESCAPEDSIZE]', escappedsize);
                        newdescriptionname = newdescriptionname.replace('SIZE', v.name);

                        var newstylecode = stylecode.replace('[SIZE]', v.code).replace('[ESCAPEDSIZE]', escappedsize);
                        newstylecode = newstylecode.replace('SIZE', v.name);

                        var neweancode = eancode.replace('[SIZE]', v.code).replace('[ESCAPEDSIZE]', escappedsize);
                        neweancode = neweancode.replace('SIZE', v.name);


                        var existing = $scope.SKU.sizegroup.items.find(function (item) {
                            return item.uid == neweancode;
                        });

                        if ($scope.mfg_manualdesigncode ||
                            typeof (existing) == "undefined" ||
                            existing == null) {

                            list.push({
                                'id': $scope.SKU.sizegroup.items.length + list.length,
                                'sizeid': v.id,
                                'uid': neweancode,
                                'code': newstylecode,
                                'colourid': $scope.SKU.colourid,
                                'sleeveid': $scope.SKU.sleeveid,
                                'name': newdescriptionname,
                                'sizegroup': value,
                                'qty': $scope.SKU.sizeratio,
                                'cost': $scope.SKU.cost,
                                'wsrate': $scope.SKU.wsrate,
                                'dealerprice': $scope.SKU.dealerprice,
                                'salerate': $scope.SKU.salerate,
                                'mrp': $scope.SKU.mrp,
                                'jobcharge': $scope.SKU.jobcharge,
                                'autonumber': currentautonumber,
                                'alloworder': true
                            });
                        }
                        else {
                            existList.push(neweancode);
                        }
                    }
                });
            });


            if (existList.length > 0) {
                $rootScope.Toaster.Error(existList.join(' , ') + ' code duplicate.');
            }
            $scope.SKUEditUpdate();
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            $http({
                method: 'POST',
                url: "MfgdesignService/Search",
                data: request
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.SKUConfig();
            }).error(function (result) {
            });
        }

        $scope.SKUEditUpdate = function () {
            if ($scope.egrid == null) {
                $timeout(function () {
                    $scope.SKUEditConfig();
                    $scope.egrid.UpdateData($scope.SKU.sizegroup.items, '_id');
                }, 10);
            }
            else {
                $scope.egrid.UpdateData($scope.SKU.sizegroup.items, '_id');
            }
        }

        $scope.egrid = null;
        $scope.SKUEditConfig = function () {
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('gridEdit', '450px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;
            if ($scope.IsUANBased) {
                var column = igrid.AddColumn(
                    {
                        id: "code", name: "Code", field: "code",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, 'enablefilter': true
                    }
                );
                var column = igrid.AddCustomColumn('colour', 'Color', 120, function (item, col) {
                    return $scope.SearchRef(item.colourid, $scope.Colors);
                });
                column.getValue = function (item, col) {
                    return $scope.SearchRef(item.colourid, $scope.Colors);
                };
                column.enablefilter = true;
                column.filtertype = 'reference';
            }
            else {
                var column = igrid.AddCustomColumn('sizegroup', 'Color/Sleeve', 120, function (item, col) {
                    return $scope.SearchRef(item.colourid, $scope.Colors) + '/' +
                        $scope.SearchRef(item.sleeveid, $scope.SleeveList);
                });
                column.getValue = function (item, col) {
                    return $scope.SearchRef(item.colourid, $scope.Colors) + '/' +
                        $scope.SearchRef(item.sleeveid, $scope.SleeveList);
                };
                column.enablefilter = true;
                column.filtertype = 'reference';
                var column = igrid.AddColumn(
                    {
                        id: "code", name: "Code", field: "code",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, 'enablefilter': true
                    }
                );
            }



            if ($scope.IsUANBased) {
                column = igrid.AddColumn(
                    {
                        id: "prdsku", name: "SKU", field: "prdsku",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, 'enablefilter': true
                    }
                );
                column = igrid.AddColumn(
                    {
                        id: "prdhsn", name: "HSN", field: "prdhsn",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, 'enablefilter': true
                    }
                );
            }

            column = igrid.AddColumn(
                {
                    id: "uid", name: "UAN", field: "uid",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            column = igrid.AddCustomColumn('size', 'Size', 70, function (item, col) {
                return $scope.SearchRef(item.sizeid, $scope.Sizes);
            });
            column.getValue = function (item, col) {
                return $scope.SearchRef(item.sizeid, $scope.Sizes);
            };
            column.enablefilter = true;
            column.filtertype = 'reference';
            if ($scope.ShowPrint) {
                column = igrid.AddColumn(
                    {
                        id: "pqty", name: "Print", field: "pqty",
                        width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, 'enablefilter': true
                    }
                );
                column.cssClass = 'rightalign';
            }
            else {
                column = igrid.AddColumn(
                    {
                        id: "qty", name: "S.Ratio", field: "qty",
                        width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true, 'enablefilter': true
                    }
                );
                column.cssClass = 'rightalign';
            }
            column = igrid.AddColumn(
                {
                    id: "cost", name: "Cost", field: "cost",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            column.cssClass = 'rightalign';
            column = igrid.AddColumn(
                {
                    id: "jobcharge", name: "Charge", field: "jobcharge",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            column.cssClass = 'rightalign';
            column = igrid.AddColumn(
                {
                    id: "wsrate", name: "WholeSale", field: "wsrate",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            column.cssClass = 'rightalign';
            column = igrid.AddColumn(
                {
                    id: "dealerprice", name: "Franchise", field: "dealerprice",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                });
            column.cssClass = 'rightalign';
            column = igrid.AddColumn(
                {
                    id: "salerate", name: "Retail", field: "salerate",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                });
            column.cssClass = 'rightalign';
            column = igrid.AddColumn(
                {
                    id: "mrp", name: "MRP", field: "mrp",
                    width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                });
            column.cssClass = 'rightalign';

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Delete'/> &nbsp;"

            igrid.AddButtonColumn('action', '..', 50, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Delete') {
                        $scope.DeleteList(args.item, $scope.SKU.sizegroup.items);
                        $scope.SKUEditUpdate();
                    }
                });
            });

            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData().getItem(row);
                if (rowdata.combo) {
                    rowdata.style = 'igridbigrowswithred';
                }
                else {
                    rowdata.style = '';
                }
                return { 'cssClasses': '' };
            }

            $scope.selectedcount = 0;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };

            igrid.grid.onDblClick.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var field = args.grid.getColumns()[args.cell].id;
                if (field == 'sizegroup') {
                    var selectedIndexes = [];
                    for (var i = 0; i < args.grid.getDataLength(); i++) {
                        if (args.grid.getDataItem(i).code == item.code) {
                            selectedIndexes.push(i);
                        }
                    }
                    args.grid.setSelectedRows(selectedIndexes);
                    $timeout(function () {
                        args.grid.invalidateRows(selectedIndexes);
                        args.grid.render();
                    }, 10);

                }
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var field = args.grid.getColumns()[args.cell].id;
                if (field == 'cost' || field == 'jobcharge' || field == 'wsrate' ||
                    field == 'dealerprice' || field == 'salerate' || field == 'mrp' ||
                    field == 'pqty') {

                    var selectedIndexes = args.grid.getSelectedRows();
                    var selectedData = [];
                    for (var index = 0; index < selectedIndexes.length; index++) {
                        selectedData.push(args.grid.getDataItem(selectedIndexes[index]));
                    }
                    selectedData.splice(selectedData.indexOf(item), 1);
                    $.each(selectedData, function (k, v) {
                        v[field] = item[field];
                    })

                    $timeout(function () {
                        args.grid.invalidateRows(selectedIndexes);
                        args.grid.render();
                    }, 10);
                }
            });
            $scope.egrid = igrid;
            if (typeof ($scope.SKU.sizegroup.items) != 'undefined')
                $scope.egrid.UpdateData($scope.SKU.sizegroup.items, '_id');
        }

        $scope.SKUConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddSimpleColumn('name', 'Name', 100);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 150, $scope.Supplier);
            igrid.AddReferenceColumn('productid', 'Product', 120, $scope.Products);
            igrid.AddReferenceColumn('fabricid', 'Fabric', 120, $scope.Fabrics);
            igrid.AddReferenceColumn('brand', 'Brand', 120, $scope.BrandName);
            igrid.AddReferenceColumn('classid', 'Class', 120, $scope.Classification);
            igrid.AddSimpleColumn('mfgyear', 'Year', 50);
            igrid.AddSimpleColumn('mfgmonth', 'Month', 50);
            //igrid.AddSimpleColumn('colour', 'Colour', 120);
            //igrid.AddSimpleColumn('sizegroupname', 'Size', 60);

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Delete', args.item);
                });
            });

            igrid.options.autoEdit = true;
            $scope.selectedrowcount = 0;
            igrid.Initialize();
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedrowcount = range.length;
                }, 10);
            };
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.AllowOrder = function (allow) {
            var seletedRows = $scope.igrid.GetSelectedRows();
            $.each(seletedRows, function (k, v) {
                v.alloworder = allow;
            });
        }


        $scope.pgrid = null;
        $scope.printlist = [];

        $scope.SKUPrintConfig = function (mode) {
            $timeout(function () {
                if (mode == 'Print')
                    $scope.PrintConfig();
                else
                    $scope.SKUEditConfig();
            }, 10)
        }

        $scope.PrintConfig = function () {

            if ($scope.pgrid != null)
                $scope.pgrid.destroy();

            var igrid = new InfyGrid('printGridContainer', '400px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.options.autoEdit = true;
            igrid.AddSimpleColumn('designcode', 'Design', 150);
            igrid.AddReferenceColumn('sleeveid', 'Sleeve', 100, $scope.SleeveList);

            if ($scope.system_possizelist != null &&
                $scope.system_possizelist != '') {
                var slist = $scope.system_possizelist.split(',');
                $.each(slist, function (k, v) {


                    var column = igrid.AddCustomColumn('size_' + k, v, 50, function (item, col) {
                        if (item.list && item.list.length > 0) {
                            var jqty = 0;
                            $.each(item.list, function (k, v) {
                                if (v.name == col.size)
                                    jqty += v.pqty;
                            })
                            return jqty;
                        }
                        return '';
                    });

                    column.cssClass = 'rightalign';
                    column.size = v;
                    column.sortable = true;
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.list && item.list.length > 0) {
                            var jqty = 0;
                            $.each(item.list, function (k, v) {
                                if (v.name == col.size)
                                    jqty += v.pqty;
                            })
                            return jqty;
                        }
                        return 0;
                    };
                    column.setValue = function (item, col, newvalue, callback) {
                        var lastValue = col.getValue(item, col);
                        var newvalue = parseFloat(newvalue);
                        if (isNaN(newvalue))
                            newvalue = 0;

                        $.each(item.list, function (k, v) {
                            if (v.name == col.size) {
                                v.pqty = newvalue;
                            }
                        })
                    }
                    column.editor = Slick.Editors.Text;
                })
            }
            else {
                igrid.AddSimpleColumn('colourcode', 'Colour', 150);
                igrid.AddSimpleColumn('name', 'Size', 150);
                var column = igrid.AddSimpleColumn('pqty', 'Print', 150);
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.total = true;
                column.filtertype = 'number';
                column.editor = Slick.Editors.Text;
            }
            igrid.EnableTotal();
            igrid.Initialize();

            //igrid.grid.onCellChange.subscribe(function (e, args) {
            //    $timeout(function () {
            //        args.grid.invalidateRow(args.row);
            //        args.grid.render();
            //    }, 10);
            //});
            $scope.pgrid = igrid;
            $scope.pgrid.UpdateData($scope.printlist);
        }

        $scope.HidePrintConfig = function () {
            if ($scope.pgrid != null) {
                $scope.pgrid.destroy();
                $scope.ShowPrint = false;
                $scope.pgrid = null;
            }
            if ($scope.egrid != null) {
                $scope.egrid.destroy();
                $scope.egrid = null;
            }
        }

        $scope.Add2Print = function () {

            var request = {
                "Item": {
                    'name': $scope.SearchCondition.designitemcode,
                    'showall': $scope.showall
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgdesignService/SearchByName",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.designid > 0) {

                    var hassizelist = false;
                    if ($scope.system_possizelist != null &&
                        $scope.system_possizelist != '') {
                        hassizelist = $scope.system_possizelist.split(',').length > 1;
                    }

                    $.each(result.sizegroup, function (k, v) {
                        if (hassizelist) {
                            v.design = result.design;
                            v.sleeveid = result.sleeveid;
                            v.fitid = result.fitid;
                            $scope.printlist.push(v);
                            $.each(v.list, function (kk, vv) {
                                vv.pqty = 0;
                                vv.designid = result.designid;
                            });
                        }
                        else {
                            $.each(v.list, function (kk, vv) {
                                vv.design = result.design;
                                vv.sleeveid = result.sleeveid;
                                vv.fitid = result.fitid;
                                vv.pqty = 0;
                                vv.designid = result.designid;
                                vv.designcode = v.designcode;
                                vv.colourid = v.colourid;
                                vv.colourcode = v.colourcode;
                                $scope.printlist.push(vv);
                            });
                        }
                    });
                }
                if ($scope.pgrid == null) {
                    $scope.PrintConfig();
                }
                $scope.pgrid.UpdateData($scope.printlist, '_id');
            });
        }

        $scope.DownloadMfgBarcode = function () {
            var list = [];
            cols = [];
            cols.push('ItemCode');
            cols.push('Design');
            cols.push('Sleeve');
            cols.push('ColourCode');
            cols.push('Colour');
            cols.push('Size');
            cols.push('Barcode');
            cols.push('MRP');
            list.push(cols.join(','));

            $.each($scope.pgrid.GetSelectedRows(), function (k, v) {
                $.each(v.list, function (kk, vv) {
                    vv.designcode = v.designcode;
                    vv.colourcode = v.colourcode;
                    vv.itemcode = v.design;
                    cols = [];
                    cols.push(v.designcode);
                    cols.push(vv.itemcode);
                    cols.push($scope.SearchRef(v.sleeveid, $scope.SleeveList));
                    cols.push(v.colourcode);
                    cols.push($scope.SearchRef(v.colourid, $scope.ColorList));
                    cols.push(vv.name);
                    cols.push(vv.uid);
                    cols.push(vv.mrp);
                    list.push(cols.join(','));
                });
            });

            var blob = new Blob([list.join('\r')], { type: 'text/csv;charset=utf-8;' });
            if (navigator.msSaveBlob) { // IE 10+
                navigator.msSaveBlob(blob, "mfgbarcode.csv");
            } else {
                var link = document.createElement("a");
                if (link.download !== undefined) { // feature detection
                    // Browsers that support HTML5 download attribute
                    var url = URL.createObjectURL(blob);
                    link.setAttribute("href", url);
                    link.setAttribute("download", "mfgbarcode.csv");
                    link.style.visibility = 'hidden';
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                }
            }

        }

        $scope.PrintMfgBarcode = function () {
            var list = [];

            var hassizelist = false;
            if ($scope.system_possizelist != null &&
                $scope.system_possizelist != '') {
                hassizelist = $scope.system_possizelist.split(',').length > 1;
            }

            $.each($scope.pgrid.GetSelectedRows(), function (k, v) {
                if (hassizelist) {
                    $.each(v.list, function (kk, vv) {
                        if (vv.pqty && vv.pqty > 0)
                            list.push(vv);
                    });
                }
                else {
                    if (v.pqty && v.pqty > 0)
                        list.push(v);
                }
            });

            if (list.length > 0) {
                $scope.ChoseReport({
                    'list': list,
                    'reportid': 0
                });
            }
        }

        $scope.selectedcount = 0;
        $scope.PrintBarcode = function () {
            var list = [];
            $.each($scope.egrid.GetSelectedRows(), function (k, v) {
                list.push(v);
            });

            $scope.ChoseReport({
                'list': list,
                'designid': $scope.SKU.id,
                'design': $scope.SKU,
                'reportid': 0
            });
        };

        $scope.uanpofileuploadedimagecomplete = function (result) {
            $scope.uploadfile = null;
            if (result.skustatus.error && result.skustatus.error != null && result.skustatus.error != '') {
                $rootScope.Toaster.Info(result.skustatus.error);
                return;
            }
            $scope.uanpofileuploadedcomplete(result)
        }
        $scope.uanpofileuploadedcomplete = function (result) {
            $scope.uploadfile = undefined;
            $scope.UANSO = result;
            var dialog = ngDialog.open({
                template: 'ImportSKU',
                data: {
                    'OrderInfo': result,
                    'ParentScope': $scope
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.OrderInfo = $scope.ngDialogData.OrderInfo;
                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.Orders = $scope.OrderInfo.skustatus.sku;
                    $scope.Count = 0;
                    $scope.SelectedAll = function () {
                        $scope.Count = 0;
                        $.each($scope.Orders, function (k, v) {
                            v.selected = $scope.selectedall;
                        });
                        $scope.Count = $scope.Orders.length;
                    }
                    $scope.SelectedChanged = function () {
                        $scope.Count = 0;
                        $.each($scope.Orders, function (k, v) {
                            $scope.Count += (typeof (v.selected) != 'undefined' && v.selected) ? 1 : 0;
                        });
                    }
                    $scope.Save = function () {
                        var selected = [];
                        $.each($scope.Orders, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected)
                                selected.push(v);
                        });
                        $scope.closeThisDialog(selected);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        var request = {
                            "items": data.value,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "MfgdesignService/ImportSKU",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Total imported/updated designs - " + result.length);
                        });

                    }
                }
            });

        }

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
                    url: "MfgdesignService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            $.each(printdocument, function (k, v) {
                $rootScope.Sent2Printer(v, printerData);
            })
        }

        $scope.categorychanged = function () {
            if (typeof ($scope.SKU.bomcategoryid) != 'undefined' &&
                $scope.SKU.bomcategoryid != null) {
                var BOMList = [];
                $.each($scope.BOMAllList, function (k, v) {
                    if (v.categoryid == $scope.SKU.bomcategoryid)
                        BOMList.push(v);
                });
                $scope.BOMList = BOMList;
            }
        }

        $scope.focusconsumption = function () {
            if ($scope.SKU.consumtiontype == 1) {
                $scope.setFocusconsumtionmode();
            }
            else {
                $scope.setFocuschargeid();
            }
        }

        $scope.CreateZeroStock = function () {
            var id = $scope.igrid.GetSelectedRows()[0].id;
            restService.Get('MfgdesignService/CreateZeroStock/' + id, true).then(function (result) {
                $rootScope.Toaster.Error(result);
            });
        }

        $scope.PrepareConsumption = function () {
            var colours = [];
            var sizes = [];
            colours.push({ 'id': -1, 'name': 'All Colours' });
            sizes.push({ 'id': -1, 'name': 'All Sizes' });
            $.each($scope.SKU.sizegroup.items, function (k, v) {
                var ecolour = colours.find(function (item) {
                    return item.id == v.colourid;
                });
                if (ecolour == undefined || ecolour == null) {
                    ecolour = $scope.Colors.find(function (item) {
                        return item.id == v.colourid;
                    });
                    colours.push(ecolour);
                }
                var esize = sizes.find(function (item) {
                    return item.id == v.sizeid;
                });
                if (esize == undefined || esize == null) {
                    esize = $scope.Sizes.find(function (item) {
                        return item.id == v.sizeid;
                    });
                    sizes.push(esize);
                }
            })
            $scope.skuColorList = colours;
            $scope.bomSizes = sizes;

            restService.Get('MfgdesignService/MfgDesignCost/' + $scope.SKU.id, true).then(function (results) {
                $scope.SKU.processlist = results;
                if ($scope.SKU.processlist == undefined ||
                    $scope.SKU.processlist == null)
                    $scope.SKU.processlist = [];
                $scope.ShowProcessGrid($scope.SKU.processlist);
            });
        }

        $scope.BOMChanged = function () {
            var bom = $scope.BOMAllList.find(function (item) {
                return item.id == $scope.SKU.bomid;
            });
            var listOfColours = [{ 'id': -1, 'name': 'SKU Colour' }];
            $.each(bom.colourlist, function (k, v) {
                var ecolour = $scope.Colors.find(function (item) {
                    return item.id == v.id;
                });
                listOfColours.push(ecolour);
            });
            $scope.bomColours = listOfColours;
        }

        $scope.PupulateConsumption = function () {
            if ($scope.SKU.consumtiontype == 1) {
                var bom = $scope.BOMAllList.find(function (item) {
                    return item.id == $scope.SKU.bomid;
                });
                var listOfMfgColours = [];
                var listOfColours = [];
                var listOfSizes = [];
                if ($scope.SKU.consumtionmode != 2 &&
                    $scope.SKU.consumtionmode != 5) {
                    if ($scope.SKU.bomcolourid &&
                        $scope.SKU.bomcolourid > 0) {
                        var ecolour = $scope.Colors.find(function (item) {
                            return item.id == $scope.SKU.bomcolourid;
                        });
                        listOfColours.push(ecolour);
                    }
                    else {
                        listOfColours = $scope.bomColours;
                    }

                    if ($scope.SKU.skucolourid &&
                        $scope.SKU.skucolourid > 0) {
                        var ecolour = $scope.Colors.find(function (item) {
                            return item.id == $scope.SKU.skucolourid;
                        });
                        listOfMfgColours.push(ecolour);
                    }
                    else {
                        listOfMfgColours = $scope.skuColorList;
                    }
                }
                else {
                    listOfColours = [];
                    $.each(bom.colourlist, function (k, v) {
                        var ecolour = $scope.Colors.find(function (item) {
                            return item.id == v.id;
                        });
                        listOfColours.push(ecolour);
                    });
                    listOfMfgColours = $scope.skuColorList;
                }
                if ($scope.SKU.consumtionmode != 1 &&
                    $scope.SKU.consumtionmode != 5) {
                    var esize = $scope.Sizes.find(function (item) {
                        return item.id == $scope.SKU.bomsizeid;
                    });
                    listOfSizes.push(esize);
                }
                else {
                    listOfSizes = $scope.bomSizes;
                }
                var rows = [];
                $.each(listOfColours, function (ck, cv) {
                    $.each(listOfSizes, function (sk, sv) {

                        $.each(listOfMfgColours, function (mk, mv) {

                            if (sv.id > 0 && cv.id > 0 && mv.id > 0) {

                                if (($scope.SKU.bomcolourid == -1 && (mv.id == cv.id)) ||
                                    ($scope.SKU.bomcolourid != -1)) {
                                    rows.push({
                                        'mfgdesignid': $scope.SKU.id,
                                        'mfgcolourid': mv.id,
                                        'sizeid': sv.id,
                                        'colourid': cv.id,
                                        'workflowid': $scope.SKU.workflowid,
                                        'stageid': $scope.SKU.workflowstageid,
                                        'type': $scope.SKU.consumtiontype,
                                        'mode': $scope.SKU.consumtionmode,
                                        'isactive': true
                                    });
                                }

                            }
                        });

                    });
                });


                $.each(rows, function (k, v) {
                    v.bomid = $scope.SKU.bomid;
                    v.categoryid = $scope.SKU.bomcategoryid;
                    var bom = $scope.BOMAllList.find(function (item) {
                        return item.id = $scope.SKU.bomid;
                    });
                    if (bom && bom != null) {
                        v.uomid = bom.uomid;
                        v.amount = bom.buyingrate;
                    }
                    v.qty = $scope.SKU.amount;
                });

            }
            else {
                var listOfMfgColours = [];
                if ($scope.SKU.skucolourid &&
                    $scope.SKU.skucolourid > 0) {
                    var ecolour = $scope.Colors.find(function (item) {
                        return item.id == $scope.SKU.skucolourid;
                    });
                    listOfMfgColours.push(ecolour);
                }
                else {
                    listOfMfgColours = $scope.skuColorList;
                }
                var rows = [];

                $.each(listOfMfgColours, function (kk, vv) {
                    $.each($scope.bomSizes, function (sk, sv) {
                        if (sv.id > 0 && vv.id > 0) {
                            var v =
                            {
                                'mfgdesignid': $scope.SKU.id,
                                'sizeid': sv.id,
                                'colourid': vv.id,
                                'mfgcolourid': vv.id,
                                'workflowid': $scope.SKU.workflowid,
                                'stageid': $scope.SKU.workflowstageid,
                                'type': $scope.SKU.consumtiontype,
                                'mode': -1,
                                'isactive': true
                            };
                            v.chargeid = $scope.SKU.chargeid;
                            var charge = $scope.JobCharges.find(function (item) {
                                return item.id = $scope.SKU.chargeid;
                            });
                            if (charge && charge != null && charge.additionalinfo)
                                v.uomid = charge.additionalinfo.uomid;
                            v.amount = $scope.SKU.amount;
                            v.qty = 0;
                            rows.push(v);
                        }
                    });
                });
            }


            if ($scope.SKU.processlist == undefined ||
                $scope.SKU.processlist == null)
                $scope.SKU.processlist = [];

            $.each(rows, function (k, v) {
                var ids = [];
                ids.push(v.stageid);
                ids.push(v.mfgcolourid);
                ids.push(v.type);
                ids.push(v.mode);
                ids.push(v.bomid);
                ids.push(v.chargeid);
                ids.push(v.colourid);
                v.sizegroupid = ids.join('-');
                ids.push(v.sizeid);
                v.uniqueid = ids.join('-');

                var exist = $scope.SKU.processlist.find(function (item) {
                    return item.uid == v.uniqueid;
                });
                if (exist == undefined || exist == null)
                    $scope.SKU.processlist.push(v);
            });
            $scope.ShowProcessGrid($scope.SKU.processlist);
        }

        $scope.SaveConsumption = function () {

            var request = {
                "Items": $scope.SKU.processlist,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgdesignService/MfgDesignCost",
                data: srequest
            }).success(function (result) {
                $scope.SKU.processlist = result;
                $scope.ShowProcessGrid($scope.SKU.processlist);
                $rootScope.Toaster.Info('Successfully saved');
            });
        }

        $scope.RemoveConsumption = function () {
            var drows = $scope.pgrid.GetSelectedRows();
            $.each(drows, function (k, v) {
                $.each(v.items, function (kk, vv) {
                    $scope.SKU.processlist.splice($scope.SKU.processlist.indexOf(vv), 1);
                });
                $scope.SKU.processgroups.splice($scope.SKU.processgroups.indexOf(v), 1);
            });
            $scope.selectedcount = 0;
            $scope.pgrid.UpdateData($scope.SKU.processgroups, '_id');
            $scope.pgrid.grid.setSelectedRows([]);
        }

        $scope.ReplaceConsumption = function () {
            var items = [];
            var rows = $scope.pgrid.GetSelectedRows();
            $.each(rows, function (k, v) {
                items.push({
                    'selected': false,
                    'items': v.items,
                    'type': v.items[0].type,
                    'bomid': v.items[0].bomid,
                    'chargeid': v.items[0].chargeid,
                });
            });
            var dialog = ngDialog.open({
                template: 'SKUReplaceBOM',
                data: [$scope, items],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.SKUProessDesigns = $scope.ngDialogData[1];
                    $scope.selectall = function (value) {
                        $.each($scope.SKUProessDesigns, function (k, v) {
                            v.selected = value;
                        });
                    }
                    $scope.changed = function (item, type) {
                        if (type == 'fabric') {
                            var tbomid = item.tbomid;
                            var fbomid = item.bomid;
                            $.each($scope.SKUProessDesigns, function (k, v) {
                                if (v.selected && v.bomid == fbomid) {
                                    v.tbomid = tbomid;
                                }
                            });
                        } else {
                            var tchargeid = item.tchargeid;
                            var fchargeid = item.chargeid;
                            $.each($scope.SKUProessDesigns, function (k, v) {
                                if (v.selected && v.chargeid == fchargeid) {
                                    v.chargeid = tchargeid;
                                }
                            });
                        }
                    }
                    $scope.replace = function () {
                        $scope.closeThisDialog($scope.SKUProessDesigns);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        $.each(data.value, function (k, v) {
                            $.each(v.items, function (kk, vv) {
                                var existindex = $scope.SKU.processlist.indexOf(vv);
                                var item = $scope.SKU.processlist[existindex];
                                var tbomid = parseInt(v.tbomid);
                                var tchargeid = parseInt(v.tchargeid);
                                if (isNaN(tbomid)) tbomid = 0;
                                if (isNaN(tchargeid)) tchargeid = 0;
                                item.bomid = tbomid;
                                item.chargeid = tchargeid;
                            });
                        });
                        $scope.ShowProcessGrid($scope.SKU.processlist)
                    }
                }
            }
            );
        }

        $scope.ImportConsumption = function () {

            var dialog = ngDialog.open({
                template: 'SKUProcessBOM',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.SKUProessDesigns = [];
                    $scope.productid = 0;
                    $scope.design = '';
                    $scope.import = function () {
                        var design = $scope.SKUProessDesigns.find(function (item) {
                            return item.selected;
                        });
                        if (design && design != null) {
                            $scope.closeThisDialog({
                                'design': design,
                                'chkprocess': $scope.chkprocess,
                                'chkbom': $scope.chkbom
                            });
                        }
                    }
                    $scope.Search = function () {
                        var request = {
                            "Item": {
                                'productid': $scope.productid,
                                'name': '%' + $scope.design + '%',
                                'onlydesign': true
                            },
                            "securitytoken": "String content"
                        }

                        $http({
                            method: 'POST',
                            url: "MfgdesignService/SearchDesignList",
                            data: request
                        }).success(function (result) {
                            $scope.SKUProessDesigns = result;
                        }).error(function (result) {
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var result = data.value;
                        restService.Get('MfgdesignService/MfgDesignCost/' + result.design.id, true).then(function (results) {
                            if ($scope.SKU.processlist == undefined ||
                                $scope.SKU.processlist == null)
                                $scope.SKU.processlist = [];

                            $.each(results, function (k, v) {

                                var exist = $scope.SKU.processlist.find(function (item) {
                                    return item.uniqueid == v.uniqueid;
                                })

                                if (exist == undefined || exist == null) {
                                    if (v.type == 1 && result.chkbom) {
                                        $scope.SKU.processlist.push(v);
                                    }
                                    else if (v.type == 2 && result.chkprocess) {
                                        $scope.SKU.processlist.push(v);
                                    }
                                }
                            });

                            $scope.ShowProcessGrid($scope.SKU.processlist);
                        });
                    }
                }
            });
        }

        $scope.ExportDesign = function () {
            var ids = [];
            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                ids.push(v.id);
            });
            $rootScope.PostDownload('MfgdesignService/Export', { 'ids': ids },
                'skudesign.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
        }

        $scope.ShowSizeGroupMode = function () {
            var dialog = ngDialog.open({
                template: 'SizeGroupMode',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', 'restService',
                    function ($scope, $timeout, $http, hotkeys, $rootScope, restService) {
                        $scope.ParentScope = $scope.ngDialogData[0];
                        $scope.SizeGroupList = $scope.ParentScope.SizeGroupList;
                        $scope.Selected = function () {
                            var sizegroup = $scope.SizeGroupList.find(function (item) {
                                return item.id == $scope.sizegroupid;
                            });
                            if (sizegroup && sizegroup != null)
                                $scope.closeThisDialog(sizegroup);
                        };
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        $scope.system_possizelist = data.value.possizelist;
                        $scope.SizeIndexList = $scope.system_possizelist.split(',');
                        $scope.PrintConfig();
                        $scope.pgrid.UpdateData($scope.printlist, '_id');
                    }
                }
            });
        }

        $scope.StageJobcharges = [];
        $scope.MfgStageChanged = function () {
            var jobcharges = [];
            $.each($scope.JobCharges, function (k, v) {
                if (v.additionalinfo.mfgconfig &&
                    v.additionalinfo.mfgconfig.stageid &&
                    v.additionalinfo.mfgconfig.stageid == $scope.SKU.workflowstageid) {
                    jobcharges.push(v);
                }
            });
            $scope.StageJobcharges = jobcharges;
        }
        $scope.CreateProcessGroup = function (rows) {
            var groups = [];
            $.each(rows, function (k, v) {
                var exist = groups.find(function (item) {
                    return item.sizegroupid == v.sizegroupid;
                });
                if (exist == undefined || exist == null) {
                    exist = (JSON.parse(JSON.stringify(v)));
                    exist.items = [];
                    exist.cost = v.amount;
                    groups.push(exist);
                }
                exist['value_' + v.sizeid] = v.qty
                exist.items.push(v);
            });
            return groups;
        }

        $scope.ShowProcessGrid = function (rows) {
            $scope.SKU.processgroups = $scope.CreateProcessGroup(rows);
            if ($scope.pgrid != null)
                $scope.pgrid.destroy();

            var igrid = new InfyGrid('gridProcess', '450px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('stageid', 'Stage', 100, $scope.MfgStages);
            column = igrid.AddCustomColumn('categoryid', 'Category', 80, function (item, col) {
                if (item.type == 1)
                    return $scope.SearchRef(item.categoryid, $scope.BOMCategories);
                return 'Charge';
            });
            column.getValue = function (item, col) {
                if (item.type == 1)
                    return $scope.SearchRef(item.categoryid, $scope.BOMCategories);
                return 'Charge';
            };
            column.enablefilter = true;
            column.filtertype = 'reference';

            column = igrid.AddCustomColumn('bomid', 'Material/Charge', 120, function (item, col) {
                if (item.type == 1)
                    return $scope.SearchRef(item.bomid, $scope.BOMAllList);
                return $scope.SearchRef(item.chargeid, $scope.JobCharges);
            });
            column.getValue = function (item, col) {
                if (item.type == 1)
                    return $scope.SearchRef(item.bomid, $scope.BOMAllList);
                return $scope.SearchRef(item.chargeid, $scope.JobCharges);
            };
            column.enablefilter = true;
            column.filtertype = 'reference';

            igrid.AddReferenceColumn('uomid', 'UOM', 70, $scope.UOMList);

            column = igrid.AddCustomColumn('bomid', 'Shade', 120, function (item, col) {
                if (item.mfgcolourid == item.colourid) {
                    return $scope.SearchRef(item.colourid, $scope.Colors);
                }
                else {
                    var response = '';
                    if (item.mfgcolourid > 0)
                        response = $scope.SearchRef(item.mfgcolourid, $scope.Colors);
                    else
                        response = 'All'
                    if (item.colourid > 0)
                        return response + ' > ' +
                            $scope.SearchRef(item.colourid, $scope.Colors);
                    else
                        return response + ' > All';
                }
            });
            column.getValue = function (item, col) {
                if (item.mfgcolourid == item.colourid) {
                    return $scope.SearchRef(item.colourid, $scope.Colors);
                }
                else {
                    var response = '';
                    if (item.mfgcolourid > 0)
                        response = $scope.SearchRef(item.mfgcolourid, $scope.Colors);
                    else
                        response = 'All'
                    if (item.colourid > 0)
                        return response + ' > ' +
                            $scope.SearchRef(item.colourid, $scope.Colors);
                    else
                        return response + ' > All';
                }
            };
            column.enablefilter = true;
            column.filtertype = 'reference';

            var column = igrid.AddColumn(
                {
                    id: "cost", name: 'Charges', field: "cost",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true, 'enablefilter': true
                }
            );
            column.cssClass = 'rightalign';

            $.each($scope.bomSizes, function (k, v) {
                if (v.id > 0) {
                    var column = igrid.AddColumn(
                        {
                            id: "value_" + v.id, name: v.name, field: "value_" + v.id,
                            width: 40, cssClass: "cell-title", editor: Slick.Editors.Text,
                            'sortable': true, 'enablefilter': true
                        }
                    );
                    column.internalkey = k;
                    column.internalvalue = v;
                    column.cssClass = 'rightalign';
                }
            })

            column = igrid.AddCustomColumn('total', 'Qty', 60, function (item, col) {
                if (item.type == 1) {
                    var qty = 0;
                    if (item.items && item.items != null) {
                        $.each(item.items, function (k, v) {
                            qty += $rootScope.GetNumber(v.type == 1 ? v.qty : v.amount);
                        });
                    }
                    return qty;
                }
                return '';
            });
            column.getValue = function (item, col) {
                var qty = 0;
                if (item.items && item.items != null) {
                    $.each(item.items, function (k, v) {
                        qty += $rootScope.GetNumber(v.type == 1 ? v.qty : v.amount);
                    });
                }
                return qty;
            };
            column.enablefilter = true;
            column.filtertype = 'reference';
            column.cssClass = 'rightalign';

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.selectedcount = 0;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var field = args.grid.getColumns()[args.cell].id;
                if (field == 'cost') {
                    if (item.type == 1) {
                        $.each(item.items, function (k, v) {
                            v.amount = item.cost;
                            item["value_" + v.sizeid] = item.cost;
                        })
                    }
                }
                else {
                    var sizeid = parseInt(field.split('_')[1]);
                    var fitem = item.items.find(function (sitem) {
                        return sitem.sizeid = sizeid;
                    });
                    if (fitem && fitem != null) {
                        if (item.type == 1)
                            fitem.qty = item[field];
                        else
                            fitem.amount = item[field];
                    }
                }
            });
            $scope.pgrid = igrid;
            $scope.pgrid.UpdateData($scope.SKU.processgroups, '_id');
        }

        $timeout(function () {
            $scope.LoadItems();
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                restService.Get('MfgdesignService/Search/' + $location.search().id, true).then(function (results) {
                    $scope.SKU = results;
                    $scope.EditSKU(item);
                });
            }
        }, 100);

    }]);