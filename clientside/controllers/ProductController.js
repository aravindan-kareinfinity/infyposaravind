
var app = angular.module('InfinityLab');

app.controller('ProductController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.selectedcount = 0;

        $scope.BarcodeIDList = restService.Get('ReferencelistService/BARCODEID', true).then(function (results) {
            $scope.BarcodeIDList = results;
        });
        
        $scope.SalesCounter = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.SalesCounter = results;
        });
        $scope.ProductCoverages = restService.Get('ReferencelistService/PRODUCTCOVERAGE', true).then(function (results) {
            $scope.ProductCoverages = results;
        });
        $scope.SearchRef = restService.SearchReference;

        $scope.Purchase = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.Purchase = results;
        });

        $scope.CessTaxList = restService.Get('ReferencelistService/CESSTAX', true).then(function (results) {
            $scope.CessTaxList = results;
        });

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.Sales = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.Sales = results;
        });
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.SizeGroups = restService.Get('ReferencelistService/SIZEGROUP', true).then(function (results) {
            $scope.SizeGroups = results;
        });
        $scope.DailPriceGroup = restService.Get('ReferencelistService/DAILYPRICEGROUP', true).then(function (results) {
            $scope.DailPriceGroup = results;
        });
        $scope.CoreProductGroup = restService.Get('ReferencelistService/COREPRODUCTGROUP', true).then(function (results) {
            $scope.CoreProductGroup = results;
        });
        $scope.DumpingGroups = restService.Get('ReferencelistService/DUMPINGGROUP', true).then(function (results) {
            $scope.DumpingGroups = results;
        });

        $scope.ProductType = [{ 'id': 0, 'name': 'Textile' },
        { 'id': 1, 'name': 'FMCG Products' },
        { 'id': 2, 'name': 'Groceries' },
        { 'id': 3, 'name': 'Vegitable/Fruites' },
        { 'id': 4, 'name': 'Foods' },
        { 'id': 6, 'name': 'Mobile' },
        { 'id': 7, 'name': 'Accessories' },
        { 'id': 8, 'name': 'Jewellery' },
        { 'id': 9, 'name': 'Consumables' }
        ];

        $scope.UOM = [{ 'id': 0, 'name': 'Piece' }, { 'id': 1, 'name': 'Meter' },
        { 'id': 2, 'name': 'Gram' },
        { 'id': 3, 'name': 'Kg' },
        { 'id': 4, 'name': 'Ml' },
        { 'id': 5, 'name': 'Litter' },
        { 'id': 6, 'name': 'Qty' }];

        $scope.BarcodeMode = [{ 'id': 0, 'name': 'Unique' }, { 'id': 1, 'name': 'CAPS and Number' },
        { 'id': 2, 'name': 'Only Number' }, { 'id': 3, 'name': 'UAN' }, { 'id': 4, 'name': 'PACK / SERIAL' },
        { 'id': 5, 'name': 'IEMI' }
        ];

        $scope.BarcodeSources = [{ 'id': 0, 'name': 'STOCK GENERATION' },
        { 'id': 1, 'name': 'ITEM-GLN/UAN/GS1' },
        { 'id': 2, 'name': 'PURCHASE-IMEI' },
        { 'id': 3, 'name': 'PRODUCT CODE' }];

        $scope.SellingMode = [{ 'id': 1, 'name': 'Piece' }, { 'id': 2, 'name': 'Pack' }, { 'id': 3, 'name': 'Cut' }];

        $scope.PricingMode = [{ 'id': 1, 'name': 'Direct' },
        { 'id': 2, 'name': 'MRP Price' },
        { 'id': 3, 'name': 'Last Price' },
        { 'id': 4, 'name': 'Margin Amount' },
        { 'id': 5, 'name': 'Margin Percentage' }
        ];

        $scope.Discount = [{ 'id': 0, 'name': 'Allow Discount On Bill' }, { 'id': 3, 'name': 'Layolty Discount' }, { 'id': 4, 'name': 'No Addln.Discount On Bill' }, { 'id': 5, 'name': 'Fixed Discount By Percentage' }];
        //$scope.Discount = [{ 'id': 1, 'name': 'No Fixed Discount' }, { 'id': 2, 'name': 'Fixed Discount By Amount' }, { 'id': 3, 'name': 'Fixed Discount By Percentage' }, { 'id': 4, 'name': 'No Discount On Bill' }];
        $scope.PurchasePlanMode = [{ 'id': 1, 'name': 'SUPPLIER VS PRICE' },
        { 'id': 2, 'name': 'BRAND VS SIZE' },
        { 'id': 3, 'name': 'SIZE VS PRICE' },
        { 'id': 4, 'name': 'MATERIAL VS PRICE' },
        { 'id': 5, 'name': 'TYPE VS PRICE' },
        { 'id': 6, 'name': 'SIZE VS FIT VS SLEEVE' },];

        $scope.Gender = [{ 'id': 1, 'name': 'Male' }, { 'id': 2, 'name': 'Female' }, { 'id': 3, 'name': 'Boy' }, { 'id': 5, 'name': 'Girl' }];

        $scope.refattributes = [{ 'id': 1, 'name': 'BRAND', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 2, 'name': 'MATERIAL', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 3, 'name': 'PATTERN', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 4, 'name': 'STYLE', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 5, 'name': 'TYPE', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 6, 'name': 'COLOUR', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 7, 'name': 'SIZE', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 8, 'name': 'DESIGN', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 9, 'name': 'PURCHASE ORDER', 'mandatory': false },
        { 'id': 10, 'name': 'SERIAL NO', 'mandatory': false },
        { 'id': 11, 'name': 'BATCH and EXPIRY', 'mandatory': false },
        { 'id': 15, 'name': 'ITEM', 'mandatory': false },
        { 'id': 16, 'name': 'COLOUROPTION', 'mandatory': false },
        { 'id': 17, 'name': 'MULTIPLE PRICE', 'mandatory': false },
        { 'id': 18, 'name': 'UPLOAD BARCODE', 'mandatory': false, hidemandatory: true },
        { 'id': 20, 'name': 'FIT', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 21, 'name': 'SLEEVE', 'mandatory': false, 'showrol': true, 'rol': false },
        { 'id': 22, 'name': 'WEIGHT', 'mandatory': false },
        { 'id': 23, 'name': 'WASTAGE', 'mandatory': false },
        { 'id': 24, 'name': 'WORKING CHARGE', 'mandatory': false },
        ];

        $scope.brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.brands = results;
        });

        $scope.ProductGroups = restService.Get('ReferencelistService/FULL-PRODUCTGROUPS', true).then(function (results) {
            $scope.ProductGroups = results;
        });

        $scope.Units = restService.Get('ReferencelistService/UNITS', true).then(function (results) {
            $scope.Units = results;
        });
        $scope.AgeSet = restService.Get('ReferencelistService/AGESET', true).then(function (results) {
            $scope.AgeSet = results;
        });


        $scope.GroupColumn = [{ 'id': 1, 'name': 'First Level' },
        { 'id': 2, 'name': 'Second Level' },
        { 'id': 3, 'name': 'Column' }
        ];
        $scope.KeyGroupList = [{ 'id': 1, 'name': 'Supplier', 'option': false, 'groupid': 0 },
        { 'id': 2, 'name': 'Brand', 'option': false, 'groupid': 0 },
        { 'id': 3, 'name': 'Size', 'option': false, 'groupid': 0 },
        { 'id': 4, 'name': 'Pattern', 'option': false, 'groupid': 0 },
        { 'id': 5, 'name': 'Style', 'option': false, 'groupid': 0 },
        { 'id': 6, 'name': 'Colour', 'option': false, 'groupid': 0 },
        { 'id': 7, 'name': 'Fit', 'option': false, 'groupid': 0 },
        { 'id': 8, 'name': 'Sleeve', 'option': false, 'groupid': 0 }];

        $scope.Entity = {
            "code": "",
            "name": "",
            "isactive": true,
            "createdby": 0,
            "barcodemode": 2,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "sellingmode": 2,
            "pricingmode": 0,
            "pricingvalue": 0,
            "salestaxid": 0,
            "discountmode": 0,
            "discountvalue": 0,
            "purchasetaxid": 0,
            "salescounterid": 0,
            "producttype": 0,
            "margin": 0,
            "image": 0,
            "barcodeprefix": "",
            "barcodesuffix": "",
            "brands": [],
            "pricerange": [],
            "keygroup": [],
            "stockholdingperiod": 90,
            "isrefill": false,
            "purchaseplanmode": 0,
            "gender": 0,
            "ageset": 0,
            "producttype": 0,
            'salesman': [],
            'attributes': {
                'brand': false, 'colour': false, 'style': false, 'size': false, 'pattern': false, 'type': false, 'material': false,
                'designid': false, 'purchaseorder': false, 'excludereward': false, 'fit': false, 'sleeve': false, 'weight': false,
                'wastage': false, 'workingcharge': false, 'customfields': []
            },
            productgroup: "",
            unit: "",
            fixedprice: "",
            hsncode: "",
            minmargin: "",
            maxmargin: "",
            activatedon: "",
            deactivatedon: ""
        };

        $scope.Products = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.allattribute = false;

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Products = (JSON.parse(JSON.stringify($scope.Entity)));
            $.each($scope.refattributes, function (k, v) {
                v.mandatory = false;
            });

        }

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            code: "",
            name: "",
            inactive: false
        }

        $scope.New = function () {
            $scope.Products = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
            $.each($scope.refattributes, function (k, v) {
                v.mandatory = false;
            });

        }

        $scope.ProductTypeChanged = function () {
            $.each($scope.refattributes, function (k, v) {
                v.mandatory = false;
                switch ($scope.Products.producttype) {
                    case 6:
                    case 7:
                        if (v.id == 12 || v.id == 13 || v.id == 14 || v.id == 15 || v.id == 16 || v.id == 18) {
                            v.show = true;
                        }
                        break;
                    case 2:
                    case 3:
                        if (v.id == 10 || v.id == 11) {
                            v.show = true;
                        }
                        break;
                }
            });
        }

        $scope.Save = function () {

            $scope.Products.keygroup = $scope.KeyGroupList;
            /*$scope.Products.brands = { items: [] };
            $.each($scope.brands, function (k, v) {
                if (v.selected)
                    $scope.Products.brands.items.push({ 'name': v.name, 'id': v.id, 'available': v.available });
            });*/

            //if ($scope.Products.code == null || $scope.Products.code == '')
            //{
            //    $scope['setFocusfirst']();
            //    $rootScope.Toaster.Error("Please Enter Code");
            //    return;
            //}

            if ($scope.Products.name == null || $scope.Products.name == '') {
                $scope['setFocusname']();
                $rootScope.Toaster.Error("Please Enter Name");
                return;
            }
            if ($scope.Products.hsncode == null || $scope.Products.hsncode == '') {
                $scope['setFocushsncode']();
                $rootScope.Toaster.Error("Please Enter HSN Code");
                return;
            }

            if ($scope.Products.hsncode.length > 8 && $scope.Products.hsncode.length < 4) {
                $scope['setFocushsncode']();
                $rootScope.Toaster.Error("Please Enter HSN length between 4 to 8 characters");
                return;
            }

            if (isNaN(parseInt($scope.Products.hsncode))) {
                $scope['setFocushsncode']();
                $rootScope.Toaster.Error("Please Enter HSN in Numeric");
                return;
            }

            if ($scope.Products.salestaxid == null || $scope.Products.salestaxid == 0) {
                $scope['setFocussalestaxid']();
                $rootScope.Toaster.Error("Please Select Sales Tax");
                return;
            }
            if ($scope.Products.purchasetaxid == null || $scope.Products.purchasetaxid == 0) {
                $scope['setFocuspurchasetaxid']();
                $rootScope.Toaster.Error("Please Select Purchase Tax");
                return;
            }
            if ($scope.Products.sellingmode == null || $scope.Products.sellingmode == 0) {
                $scope['setFocussellingmode']();
                $rootScope.Toaster.Error("Please Select Selling Mode");
                return;
            }

            var excludereward = $scope.Products.attributes.excludereward;
            $scope.Products.attributes = {};
            $scope.Products.attributes.customfields = [];
            $scope.Products.attributes.excludereward = excludereward;
            $.each($scope.refattributes, function (k, v) {

                if (v.id == 1) $scope.Products.attributes.brand = v.show;
                else if (v.id == 2) $scope.Products.attributes.material = v.show;
                else if (v.id == 3) $scope.Products.attributes.pattern = v.show;
                else if (v.id == 4) $scope.Products.attributes.style = v.show;
                else if (v.id == 5) $scope.Products.attributes.type = v.show;
                else if (v.id == 6) $scope.Products.attributes.colour = v.show;
                else if (v.id == 7) $scope.Products.attributes.size = v.show;
                else if (v.id == 8) $scope.Products.attributes.designid = v.show;
                else if (v.id == 9) $scope.Products.attributes.purchaseorder = v.show;
                else if (v.id == 10) $scope.Products.attributes.serialno = v.show;
                else if (v.id == 11) $scope.Products.attributes.batchandexpiry = v.show;
                else if (v.id == 12) $scope.Products.attributes.model = v.show;
                else if (v.id == 13) $scope.Products.attributes.quality = v.show;
                else if (v.id == 14) $scope.Products.attributes.warrenty = v.show;
                else if (v.id == 15) $scope.Products.attributes.item = v.show;
                else if (v.id == 16) $scope.Products.attributes.colouroption = v.show;
                else if (v.id == 17) $scope.Products.attributes.multipleprice = v.show;
                else if (v.id == 18) $scope.Products.attributes.uploadbarcode = v.show;
                else if (v.id == 19) $scope.Products.attributes.simtype = v.show;
                else if (v.id == 20) $scope.Products.attributes.fit = v.show;
                else if (v.id == 21) $scope.Products.attributes.sleeve = v.show;
                else if (v.id == 22) $scope.Products.attributes.weight = v.show;
                else if (v.id == 23) $scope.Products.attributes.wastage = v.show;
                else if (v.id == 24) $scope.Products.attributes.workingcharge = v.show;

                if (v.id == 1) $scope.Products.attributes.rolbrand = v.rol;
                else if (v.id == 2) $scope.Products.attributes.rolmaterial = v.rol;
                else if (v.id == 3) $scope.Products.attributes.rolpattern = v.rol;
                else if (v.id == 4) $scope.Products.attributes.rolstyle = v.rol;
                else if (v.id == 5) $scope.Products.attributes.roltype = v.rol;
                else if (v.id == 6) $scope.Products.attributes.rolcolour = v.rol;
                else if (v.id == 7) $scope.Products.attributes.rolsize = v.rol;
                else if (v.id == 8) $scope.Products.attributes.roldesignid = v.rol;
                else if (v.id == 20) $scope.Products.attributes.rolfit = v.rol;
                else if (v.id == 21) $scope.Products.attributes.rolsleeve = v.rol;


                if (v.id == 1) $scope.Products.attributes.mandatorybrand = v.mandatory;
                else if (v.id == 2) $scope.Products.attributes.mandatorymaterial = v.mandatory;
                else if (v.id == 3) $scope.Products.attributes.mandatorypattern = v.mandatory;
                else if (v.id == 4) $scope.Products.attributes.mandatorystyle = v.mandatory;
                else if (v.id == 5) $scope.Products.attributes.mandatorytype = v.mandatory;
                else if (v.id == 6) $scope.Products.attributes.mandatorycolour = v.mandatory;
                else if (v.id == 7) $scope.Products.attributes.mandatorysize = v.mandatory;
                else if (v.id == 8) $scope.Products.attributes.mandatorydesignid = v.mandatory;

                else if (v.id == 9) $scope.Products.attributes.mandatorypurchaseorder = v.mandatory;
                else if (v.id == 10) $scope.Products.attributes.mandatoryserialno = v.mandatory;
                else if (v.id == 11) $scope.Products.attributes.mandatorybatchandexpiry = v.mandatory;
                else if (v.id == 12) $scope.Products.attributes.mandatorymodel = v.mandatory;
                else if (v.id == 13) $scope.Products.attributes.mandatoryquality = v.mandatory;
                else if (v.id == 14) $scope.Products.attributes.mandatorywarrenty = v.mandatory;
                else if (v.id == 15) $scope.Products.attributes.mandatoryitem = v.mandatory;
                else if (v.id == 16) $scope.Products.attributes.mandatorycolouroption = v.mandatory;
                else if (v.id == 17) $scope.Products.attributes.mandatorymultipleprice = v.mandatory;
                else if (v.id == 18) $scope.Products.attributes.mandatoryuploadbarcode = v.mandatory;
                else if (v.id == 19) $scope.Products.attributes.mandatorysimtype = v.mandatory;
                else if (v.id == 20) $scope.Products.attributes.mandatoryfit = v.mandatory;
                else if (v.id == 21) $scope.Products.attributes.mandatorysleeve = v.mandatory;
                else if (v.id == 22) $scope.Products.attributes.mandatoryweight = v.mandatory;
                else if (v.id == 23) $scope.Products.attributes.mandatorywastage = v.mandatory;
                else if (v.id == 24) $scope.Products.attributes.mandatoryworkingcharge = v.mandatory;

                if (v.id >= 200 && v.cfname && v.cfname != null) {
                    $scope.Products.attributes.customfields.push({
                        'key': v.cfname,
                        'mandatory': v.mandatory,
                        'show': v.show,
                        'rol': v.rol
                    })
                }
            });

            $scope.Products.productgroup_1 = 0;
            $scope.Products.productgroup_2 = 0;
            $scope.Products.productgroup_3 = 0;
            $scope.Products.productgroup_4 = 0;
            $scope.Products.productgroup_5 = 0;
            if ($scope.Products.productgroup &&
                $scope.Products.productgroup != null) {
                var productgroup = $scope.ProductGroups.find(function (item) {
                    return item.id == $scope.Products.productgroup;
                });
                if (productgroup && productgroup != null &&
                    productgroup.additionalinfo != null &&
                    productgroup.additionalinfo.hierarchyconfig != null) {
                    $scope.Products.productgroup_1 = productgroup.additionalinfo.hierarchyconfig.p1;
                    $scope.Products.productgroup_2 = productgroup.additionalinfo.hierarchyconfig.p2;
                    $scope.Products.productgroup_3 = productgroup.additionalinfo.hierarchyconfig.p3;
                    $scope.Products.productgroup_4 = productgroup.additionalinfo.hierarchyconfig.p4;
                    $scope.Products.productgroup_5 = productgroup.additionalinfo.hierarchyconfig.p5;
                }
            }

            var request = {
                "Item": $scope.Products,
                "securitytoken": "String content"
            }

            //  var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ProductsService/Save",
                data: request
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    if (result.ErrorMessage != 'undefined' && result.ErrorMessage != null && result.ErrorMessage != '') {
                        $rootScope.Toaster.Error(result.ErrorMessage);
                        return;
                    }
                    else {
                        $rootScope.Toaster.Info("Products Saved Successfully.!");
                        $scope.Products = (JSON.parse(JSON.stringify($scope.Entity)));
                        $.each($scope.refattributes, function (k, v) {
                            v.mandatory = false;
                        });
                    }
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
                                    url: "ProductsService/Delete",
                                    data: request
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
                if (item.brands == null)
                    item.brands = [];


                $scope.ShowSearch = false;
                $scope.Products = item;
                if ($scope.Products.keygroup != null)
                    $scope.KeyGroupList = $scope.Products.keygroup;

                if (typeof ($scope.Products.attributes) == 'undefined' || $scope.Products.attributes == null)
                    $scope.Products.attributes = {};

                if (new Date(item.activatedon).getFullYear() == 1)
                    $scope.Products.activatedon = null;

                $.each($scope.refattributes, function (k, v) {
                    v.mandatory = false;
                    v.show = false;
                    v.rol = false;

                    if (v.id == 1 && $scope.Products.attributes.mandatorybrand) v.mandatory = true;
                    else if (v.id == 2 && $scope.Products.attributes.mandatorymaterial) v.mandatory = true;
                    else if (v.id == 3 && $scope.Products.attributes.mandatorypattern) v.mandatory = true;
                    else if (v.id == 4 && $scope.Products.attributes.mandatorystyle) v.mandatory = true;
                    else if (v.id == 5 && $scope.Products.attributes.mandatorytype) v.mandatory = true;
                    else if (v.id == 6 && $scope.Products.attributes.mandatorycolour) v.mandatory = true;
                    else if (v.id == 7 && $scope.Products.attributes.mandatorysize) v.mandatory = true;
                    else if (v.id == 8 && $scope.Products.attributes.mandatorydesignid) v.mandatory = true;
                    else if (v.id == 9 && $scope.Products.attributes.mandatorypurchaseorder) v.mandatory = true;
                    else if (v.id == 10 && $scope.Products.attributes.mandatoryserialno) v.mandatory = true;
                    else if (v.id == 11 && $scope.Products.attributes.mandatorybatchandexpiry) v.mandatory = true;
                    else if (v.id == 12 && $scope.Products.attributes.mandatorymodel) v.mandatory = true;
                    else if (v.id == 13 && $scope.Products.attributes.mandatoryquality) v.mandatory = true;
                    else if (v.id == 14 && $scope.Products.attributes.mandatorywarrenty) v.mandatory = true;
                    else if (v.id == 15 && $scope.Products.attributes.mandatoryitem) v.mandatory = true;
                    else if (v.id == 16 && $scope.Products.attributes.mandatorycolouroption) v.mandatory = true;
                    else if (v.id == 17 && $scope.Products.attributes.mandatorymultipleprice) v.mandatory = true;
                    else if (v.id == 18 && $scope.Products.attributes.mandatoryuploadbarcode) v.mandatory = true;
                    else if (v.id == 19 && $scope.Products.attributes.mandatorysimtype) v.mandatory = true;
                    else if (v.id == 20 && $scope.Products.attributes.mandatoryfit) v.mandatory = true;
                    else if (v.id == 21 && $scope.Products.attributes.mandatorysleeve) v.mandatory = true;
                    else if (v.id == 22 && $scope.Products.attributes.mandatoryweight) v.mandatory = true;
                    else if (v.id == 23 && $scope.Products.attributes.mandatorywastage) v.mandatory = true;
                    else if (v.id == 24 && $scope.Products.attributes.mandatoryworkingcharge) v.mandatory = true;

                    if (v.id == 1 && $scope.Products.attributes.rolbrand) v.rol = true;
                    else if (v.id == 2 && $scope.Products.attributes.rolmaterial) v.rol = true;
                    else if (v.id == 3 && $scope.Products.attributes.rolpattern) v.rol = true;
                    else if (v.id == 4 && $scope.Products.attributes.rolstyle) v.rol = true;
                    else if (v.id == 5 && $scope.Products.attributes.roltype) v.rol = true;
                    else if (v.id == 6 && $scope.Products.attributes.rolcolour) v.rol = true;
                    else if (v.id == 7 && $scope.Products.attributes.rolsize) v.rol = true;
                    else if (v.id == 8 && $scope.Products.attributes.roldesignid) v.rol = true;
                    else if (v.id == 20 && $scope.Products.attributes.rolfit) v.rol = true;
                    else if (v.id == 21 && $scope.Products.attributes.rolsleeve) v.rol = true;

                    if (v.id == 1 && $scope.Products.attributes.brand) v.show = true;
                    else if (v.id == 2 && $scope.Products.attributes.material) v.show = true;
                    else if (v.id == 3 && $scope.Products.attributes.pattern) v.show = true;
                    else if (v.id == 4 && $scope.Products.attributes.style) v.show = true;
                    else if (v.id == 5 && $scope.Products.attributes.type) v.show = true;
                    else if (v.id == 6 && $scope.Products.attributes.colour) v.show = true;
                    else if (v.id == 7 && $scope.Products.attributes.size) v.show = true;
                    else if (v.id == 8 && $scope.Products.attributes.designid) v.show = true;
                    else if (v.id == 9 && $scope.Products.attributes.purchaseorder) v.show = true;
                    else if (v.id == 10 && $scope.Products.attributes.serialno) v.show = true;
                    else if (v.id == 11 && $scope.Products.attributes.batchandexpiry) v.show = true;
                    else if (v.id == 12 && $scope.Products.attributes.model) v.show = true;
                    else if (v.id == 13 && $scope.Products.attributes.quality) v.show = true;
                    else if (v.id == 14 && $scope.Products.attributes.warrenty) v.show = true;
                    else if (v.id == 15 && $scope.Products.attributes.item) v.show = true;
                    else if (v.id == 16 && $scope.Products.attributes.colouroption) v.show = true;
                    else if (v.id == 17 && $scope.Products.attributes.multipleprice) v.show = true;
                    else if (v.id == 18 && $scope.Products.attributes.uploadbarcode) v.show = true;
                    else if (v.id == 19 && $scope.Products.attributes.simtype) v.show = true;
                    else if (v.id == 18 && $scope.Products.attributes.uploadbarcode) v.show = true;
                    else if (v.id == 19 && $scope.Products.attributes.simtype) v.show = true;
                    else if (v.id == 20 && $scope.Products.attributes.fit) v.show = true;
                    else if (v.id == 21 && $scope.Products.attributes.sleeve) v.show = true;
                    else if (v.id == 22 && $scope.Products.attributes.weight) v.mandatory = true;
                    else if (v.id == 23 && $scope.Products.attributes.wastage) v.mandatory = true;
                    else if (v.id == 24 && $scope.Products.attributes.workingcharge) v.mandatory = true;

                    if (v.id >= 200 &&
                        $scope.Products.attributes.customfields &&
                        $scope.Products.attributes.customfields != null) {

                        var cfitem = $scope.Products.attributes.customfields.find(function (item) {
                            return item.key == v.cfname;
                        });
                        if (cfitem && cfitem != null) {
                            v.mandatory = cfitem.mandatory;
                            v.show = cfitem.show;
                            v.rol = cfitem.rol;
                        }
                    }
                });
            }
        }

        $scope.ManageSalesman = function (action, value) {
            if (action == 'add') {
                var salesman = $scope.Employees.find(function (item) {
                    return item.id == value;
                });
                if (salesman != null) {
                    $scope.Products.salesman.push({ 'id': salesman.id, 'name': salesman.name, 'isavailable': true });
                }
            }
            else if (action == 'delete') {
                $scope.Products.salesman.splice($scope.Products.salesman.indexOf(value), 1);
            }
        }

        $scope.ManageBrand = function (action, value) {
            if (action == 'add') {
                var brand = $scope.brands.find(function (item) {
                    return item.id == value;
                });
                if (brand != null) {
                    $scope.Products.brands.push({ 'id': brand.id, 'name': brand.name, 'isavailable': true });
                }
            }
            else if (action == 'delete') {
                $scope.Products.brands.splice($scope.Products.brands.indexOf(value), 1);
            }
        }
        $scope.ManagePriceRange = function (action, value) {
            if (action == 'add') {
                if ($scope.Products.pricerange == null || $scope.Products.pricerange == 'undefined')
                    $scope.Products.pricerange = [];
                $scope.Products.pricerange.push({
                    'fromvalue': $scope.pricerangefrom,
                    'tovalue': $scope.pricerangeto
                });
                $scope.pricerangefrom = 0;
                $scope.pricerangeto = 0;
            }
            else if (action == 'delete') {
                $scope.Products.pricerange.splice($scope.Products.pricerange.indexOf(value), 1);
            }
        }
        $scope.Search = function () {

            if ($scope.SearchCondition.producttype == undefined ||
                $scope.SearchCondition.producttype == null ||
                $scope.SearchCondition.producttype == '')
                $scope.SearchCondition.producttype = -1;

            if ($scope.SearchCondition.barcodemode != undefined &&
                $scope.SearchCondition.barcodemode != null && 
                $scope.SearchCondition.barcodemode == 0 )
                $scope.SearchCondition.barcodemode = -1;

            $scope.SearchCondition.showproducttype = true;
            $scope.SearchCondition.showdailyprice = $scope.SearchCondition.isdailyprice;

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            $http({
                method: 'POST',
                url: "ProductsService/Search",
                data: request
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.ProductConfig();
            }).error(function (result) {
            });


        }

        $scope.ProductConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 10);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('producttype', 'Product Type', 100, $scope.ProductType);
            igrid.AddSimpleColumn('code', 'Code', 80);
            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddReferenceColumn('salestaxid', 'Sales Tax', 100, $scope.Sales);
            igrid.AddReferenceColumn('purchasetaxid', 'Purchase Tax', 100, $scope.Purchase);
            igrid.AddReferenceColumn('sellingmode', 'Selling Mode', 100, $scope.SellingMode);
            igrid.AddReferenceColumn('pricingmode', 'Pricing Mode', 100, $scope.PricingMode);

            igrid.AddColumn(
                {
                    id: "hsncode", name: "HSN Code", field: "hsncode",
                    width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddReferenceColumn('productgroup', 'Group', 100, $scope.ProductGroups);
            igrid.AddReferenceColumn('salescounterid', 'Sales Counter', 170, $scope.SalesCounter);
            igrid.AddReferenceColumn('discountmode', 'Discount', 100, $scope.Discount);
            igrid.AddFormatedColumn('discountvalue', 'Discount Value', 100, 'N2', 2);

            var commands = "";
            if ($rootScope.UserInfo.p_PRODUCTMASTER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_PRODUCTMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_PRODUCTMASTER.modify || $rootScope.UserInfo.p_PRODUCTMASTER.delete) {
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
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                    $scope.$apply();
                }, 10);
            };
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult, '_id');

        }

        $scope.MergeSelected = function () {
            var dialog = ngDialog.open({
                template: 'ChooseMerge',
                data: {
                    'rows': $scope.igrid.GetSelectedRows(),
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SelectedRows = (JSON.parse(JSON.stringify($scope.ngDialogData.rows)));
                    $scope.SelectedItem = null;
                    $timeout(function () {
                        $scope.id = 0;
                    }, 10);
                    $scope.rowselected = function () {
                        $scope.SelectedRows = (JSON.parse(JSON.stringify($scope.ngDialogData.rows)));
                        if (typeof ($scope.id) != 'undefined' && $scope.id > 0) {
                            var selected = $scope.SelectedRows.find(function (item) {
                                return item.id == $scope.id;
                            });
                            $scope.SelectedItem = selected;
                        }
                    }
                    $scope.Selected = function () {
                        $scope.closeThisDialog({ 'selectedid': $scope.id });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        request = {
                            "item": {
                                'id': data.value.selectedid,
                                'deletelist': $scope.igrid.GetSelectedRows()
                            },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "ProductsService/Merge",
                            data: srequest
                        }).success(function (result) {
                            if (result) {
                                var rows = $scope.igrid.GetSelectedRows();
                                $.each(rows, function (k, v) {
                                    if (v.id != data.value.selectedid) {
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(v), 1);
                                    }
                                });
                                $scope.igrid.UpdateData($scope.SearchResult, '_id');
                            }
                        });
                    }
                }
            });
        }

        $scope.Export = function () {
            if (typeof ($scope.igrid) == 'undefined') return;
            $scope.exportHref = $scope.igrid.ExportExcel($scope.igrid.grid, 'Product List');
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

        $scope.ChangeHSN = function () {
            var items = [];
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                items.push({ 'productid': v.id, 'hsncode': v.hsncode });
            });

            var request = {
                "Items": items,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ProductsService/ChangeHSN",
                data: srequest
            }).success(function (result) {
                if (result) {
                    $rootScope.Toaster.Info("HSN Updated");
                }
            });
        }

        $scope.ChangeProductGroup = function (ItemDetails, callback) {
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length == 0) return;
            var dialog = ngDialog.open({
                template: 'ProductGroup',
                data: {
                    'scope': $scope,
                    'list': seletedRows
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.ProductGroups = $scope.ngDialogData.scope.ProductGroups;
                    $scope.productids = [];
                    $.each($scope.ngDialogData.list, function (k, v) {
                        $scope.productids.push(v.id);
                    });
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'productids': $scope.productids,
                            'productgroup': $scope.productgroup
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {

                if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                    var request = {
                        "Item": data.value,
                        "SecurityToken": "String content"
                    }

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "ProductsService/ChangeProductGroup",
                        data: srequest
                    }).success(function (result) {
                        $rootScope.Toaster.Info("Updated successfully.");
                    });

                }

            });
        }

        $scope.ChangeTax = function (ItemDetails, callback) {
            var seletedRows = this.igrid.GetSelectedRows();
            if (seletedRows.length == 0) return;
            var dialog = ngDialog.open({
                template: 'ChangeTax',
                data: {
                    'scope': $scope,
                    'list': seletedRows
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Purchase = $scope.ngDialogData.scope.Purchase;
                    $scope.Sales = $scope.ngDialogData.scope.Sales;
                    $scope.productids = [];
                    $.each($scope.ngDialogData.list, function (k, v) {
                        $scope.productids.push(v.id);
                    });
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'productids': $scope.productids,
                            'purchasetaxid': $scope.ptaxid,
                            'salestaxid': $scope.staxid,
                            'fromdate': $scope.fromdate,
                            'todate': $scope.todate
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {

                if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                    var request = {
                        "Item": data.value,
                        "SecurityToken": "String content"
                    }

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "ProductsService/ChangeTax",
                        data: srequest
                    }).success(function (result) {
                        $rootScope.Toaster.Info("Updated successfully.");
                    });

                }

            });
        }

        $scope.SelectAll = function (action) {
            $.each($scope.refattributes, function (k, v) {
                v.mandatory = $scope.allattribute;
            })
        }

        $scope.system_customfields = false;
        $scope.LoadAppsetting = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadAppsetting, 100);
            } else {

                $scope.system_customfields = $rootScope.UserInfo.appsettings.system_customfields || false;

                restService.Get('ReferencelistService/FULL-CUSTOMFIELDS', true).then(function (results) {
                    $.each(results, function (k, v) {
                        if (v.additionalinfo &&
                            v.additionalinfo.customfield &&
                            v.additionalinfo.customfield.cfbuilt) {
                            $scope.refattributes.push(
                                {
                                    'id': 200 + k,
                                    'cfname': v.additionalinfo.customfield.cfname,
                                    'name': v.name,
                                    'mandatory': false,
                                    'show': false,
                                    'showrol': true,
                                    'rol': false
                                }
                            )
                        }
                    });
                });

                if (typeof ($location.search().id) != 'undefined' &&
                    $location.search().id.length > 0) {
                    request = {
                        "Item": { 'id': $location.search().id },
                        "SecurityToken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "ProductsService/Search",
                        data: srequest
                    }).success(function (result) {
                        if (angular.isArray(result)) {
                            if (result[0].brands == null)
                                result[0].brands = [];
                            $scope.ShowSearch = false;
                            $scope.Products = result[0];
                            if ($scope.Products.keygroup != null)
                                $scope.KeyGroupList = $scope.Products.keygroup;
                        }
                    }).error(function (result) {
                    });
                }
            }
        }

        $scope.LoadAppsetting();
    }]);




