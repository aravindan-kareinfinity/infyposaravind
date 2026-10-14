
var app = angular.module('InfinityLab');

app.controller('PurchaseOrderController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', '$state', 'cntxService', '$stateParams', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, $state, cntxService, $stateParams, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.orderqty = 0;
        $scope.ordervalue = 0;
        $scope.ordertax = 0;
        $scope.ordercess = 0;
        $scope.DisableDateChange = false;
        $scope.SupportDesignList = false;
        $scope.ItemNameList = [];
        $scope.ItemList = [];
        $scope.Showitem = false;
        $scope.ShowVendorGLN = false;
        $scope.panelheight = { 'height': '500px' };
        $scope.Display = {
            'internalList': [],
            'List': [],
            'designStyle': { 'width': '100px' },
            'Grouped': false,
            'GroupChanged': function () {
                this.setMode(this.Grouped ? 'Group' : 'Item');
            },
            'Clear': function () {
                this.internalList = [];
                this.List = [];
            },
            'mode': 'Item',
            'setList': function (list) {
                this.internalList = list;
                this.setMode(this.mode);
            },
            'setMode': function (mode) {
                this.mode = mode;
                if (mode == 'Item') {
                    this.Grouped = false;
                    this.List = this.internalList;
                    this.designStyle = { 'width': '100px' };
                }
                else {
                    this.prepareGroup();
                    this.Grouped = true;
                    this.designStyle = { 'width': '170px' };
                }
            },
            'deleteItem': function (item) {
                if (this.Grouped) {
                    var internalList = this.internalList;
                    $.each(item.internalList, function (k, v) {
                        internalList.splice(internalList.indexOf(item), 1);
                    });
                    this.List.splice(this.List.indexOf(item), 1);
                }
                else {
                    this.List.splice(this.List.indexOf(item), 1);
                }
            },
            'addItem': function (item) {
                this.internalList.push(item);
                if (this.mode != 'Item') {
                    this.List.push(item);
                }
            },
            'prepareGroup': function () {
                var glist = [];
                $.each(this.internalList, function (k, v) {
                    var exist = glist.find(function (item) {
                        return item.groupcode == v.groupcode;
                    });
                    if (typeof (exist) == 'undefined' || exist == null) {
                        exist = v;
                        exist.internalList = [];
                        exist.internalList.push(JSON.parse(JSON.stringify(v)));
                        v.size = exist.groupname;
                        glist.push(exist);
                    }
                    else {
                        exist.internalList.push(v);
                        exist.oqty += v.oqty;
                    }
                });
                this.List = glist;
            },
            'changed': function () {
            }
        };
        $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
            $scope.AdditionalChargeList = results;
        });
        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS/9999', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.SizeGroupList = restService.Get('ReferencelistService/SIZEGROUP', true).then(function (results) {
            $scope.SizeGroupList = results;
        });
        $scope.Company = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.LocationList = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });
        $scope.StyleList = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.StyleList = results;
        });
        $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transports = results;
        });
        $scope.AgreementList = restService.Get('ReferencelistService/AGREEMENTPO', true).then(function (results) {
            $scope.AgreementList = results;
        });

        $scope.JumpList = [];
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });
        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.PatternList = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.PatternList = results;
        });

        $scope.MaterialList = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.MaterialList = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });
        $scope.SleeveList = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.SleeveList = results;
        });
        $scope.TaxList = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.TaxList = results;
        });

        $scope.FitList = restService.Get('ReferencelistService/FIT', true).then(function (results) {
            $scope.FitList = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;

            $scope.LocalContext = cntxService.CreatePageContext('POBuilder');
            /*$stateParams.list = $scope.LocalContext.Get('OrderList', false);
            if ($stateParams.list && $stateParams.list != null) {
                $stateParams.directlaunch = false;
                $stateParams.source = 'PO Builder';
            }*/
            if (!$stateParams.directlaunch) {
                $timeout(function () {
                    if ($stateParams.source == 'PO Builder')
                        $scope.LoadFromPOBuilder($stateParams.list, $stateParams.locationid);

                    if ($stateParams.source == 'Reorder')
                        $scope.LoadFromReorder($stateParams.list, $stateParams.locationid);

                }, 50);
            }

        });
        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });
        $scope.City = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.City = results;
        });

        $scope.POType = restService.Get('ReferencelistService/POTYPE', true).then(function (results) {
            $scope.POType = results;
            if (results.length == 0) {
                results.push({ 'id': 0, 'name': 'Textile' });
            }
            results.push({ id: -1, name: 'UAN/GLN' });
            results.push({ id: -2, name: 'Hypermarket' });
            results.push({ id: -3, name: 'Vendor Agreement' });
            results.push({ id: -4, name: 'Daily Trade' });
            results.push({ id: -5, name: 'UAN PO Upload' });
            results.push({ id: -9, name: 'Product Design' });
        });

        $scope.ShowStockView = false;

        $scope.SearchPOMode = [{ id: 0, name: 'Detail' },
        { id: 1, name: 'Summary' },
        { id: 2, name: 'Pending' },
        { id: 3, name: 'Agreement' }];

        $scope.DeliveryMode = [{ id: 0, name: 'Any' },
        { id: 1, name: 'Bag' },
        { id: 2, name: 'Box' },
        { id: 3, name: 'Pcs' },
        { id: 4, name: 'Meter' }];

        $scope.POMode = [{ id: 0, name: 'Size Range Based' },
        { id: 1, name: 'Size Set Based' },
        { id: 2, name: 'Single Size' },
        { id: 3, name: 'Design Based' },
        { id: 5, name: 'UAN Based' }];

        $scope.POCategory = [{ id: 0, name: 'Trade' },
        { id: 1, name: 'Non Trade' },
        { id: 2, name: 'Return' }];

        $scope.FreightList = [{ id: 1, name: 'To Pay' },
        { id: 2, name: 'Paid' }];

        $scope.POStatus = [{ id: 0, name: 'PENDING' }, { id: 1, name: 'APPROVED' }, { id: 2, name: 'CANCELLED' }, { id: 3, name: 'PARTIALLY COMPLETED' }, { id: 4, name: 'COMPLETED' }];
        $scope.POSearchStatus = [{ id: 0, name: 'PENDING APPROVAL' }, { id: 1, name: 'APPROVED' }, { id: 2, name: 'CANCELLED' }, { id: 3, name: 'PARTIALLY COMPLETED' }, { id: 4, name: 'COMPLETED' }, { id: -9, name: 'ALL' }];
        //$scope.POType = [{ id: 1, name: 'ROUTINE' }, { id: 2, name: 'SEASONAL' }, { id: 3, name: 'FESTIVAL' }, { id: 4, name: 'EMERGENCY' }];
        $scope.POList = [];
        $scope.PendingPOList = [];
        $scope.Entity = {
            "id": 0,
            "potype": 0,
            "expectedate": "",
            "podate": new Date(),
            "supplierid": null,
            "locationid": 0,
            "deliverylocationid": 0,
            "requestlocationid": 0,
            "pono": "",
            "termsandcondition": {},
            "status": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "tolerance": 5,
            "freight": 0,
            "agentid": 0,
            "transportid": 0,
            "remarks": "",
            "Purchaseorderitems": [],
            Details: {
                "oqty": 0,
                "productid": null,
                "printingname": null,
                "brandid": null,
                "patternid": null,
                "styleid": null,
                "sizeid": null,
                "colourid": null,
                "materialid": null,
                "typeid": null,
                "sleeveid": null,
                "fitid": null,
                "id": 0,
                "balance": "",
                "price": "",
                "designid": ""
            }
        };
        $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.SearchCondition = {
            'pono': null,
            'supplierid': 0,
            'expectedate': null,
            'statusid': -9
        };

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.ErrorMessage = "";

        $scope.Clear = function () {
            $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ProductRequest.podate = new Date();
            $scope.orderqty = 0;
            $scope.ordervalue = 0;
            $scope.Display.Clear();
        };


        $scope.SaveEANPO = function () {
            var request = {
                "item": $scope.ProductRequest,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PurchaseorderService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with PO No :" + result.pono);
                    $scope.ProductRequest.pono = result.pono;
                    $scope.ProductRequest.id = result.id;
                    $scope.ProductRequest.version = result.version;
                    $scope.ProductRequest.modifiedby = result.modifiedby;
                    $scope.ProductRequest.createdby = result.createdby;
                    $scope.ProductRequest.modifiedon = result.modifiedon;
                    $scope.ProductRequest.createdon = result.createdon;
                    $scope.ProductRequest.Purchaseorderitems = result.Purchaseorderitems;
                    $scope.Display.setList($scope.ProductRequest.Purchaseorderitems);
                }
                else {
                    $rootScope.Toaster.Error("Attempt Failed." + result.ErrorMessage);
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }

            });
        }


        $scope.SaveAllEANPO = function () {
            var request = {
                "item": $scope.UANPoList.postatus,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PurchaseorderService/SaveUANPO",
                data: srequest
            }).success(function (result) {

            });
        }


        $scope.Save = function () {
            if ($scope.ProductRequest.companyid == null || $scope.ProductRequest.companyid == 0) {
                $rootScope.Toaster.Error("Please select company!");
                return;
            }
            if ($scope.ProductRequest.supplierid == null || $scope.ProductRequest.supplierid == 0) {
                $rootScope.Toaster.Error("Please select supplier!");
                return;
            }
            if ($scope.ProductRequest.podate == null || $scope.ProductRequest.podate == 0) {
                $rootScope.Toaster.Error("Please select PO date!");
                return;
            }
            var company = $scope.Company.find(function (item) {
                return item.id == $scope.ProductRequest.companyid;
            });
            var supplier = $scope.Suppliers.find(function (item) {
                return item.id == $scope.ProductRequest.supplierid;
            });

            $scope.ProductRequest.isinterstate = false;
            if (company.gstno != null &&
                company.gstno != '' &&
                supplier.gstno != null &&
                supplier.gstno != '') {
                $scope.ProductRequest.isinterstate = company.gstno.substring(0, 2) != supplier.gstno.substring(0, 2);
            }

            var request = {
                "item": $scope.ProductRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchaseorderService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with PO No :" + result.pono);
                    $scope.ProductRequest.pono = result.pono;
                    $scope.ProductRequest.id = result.id;
                    $scope.ProductRequest.version = result.version;
                    $scope.ProductRequest.modifiedby = result.modifiedby;
                    $scope.ProductRequest.createdby = result.createdby;
                    $scope.ProductRequest.modifiedon = result.modifiedon;
                    $scope.ProductRequest.createdon = result.createdon;
                    $scope.ProductRequest.Purchaseorderitems = result.Purchaseorderitems;
                    $scope.Display.setList($scope.ProductRequest.Purchaseorderitems);
                }
                else {
                    $rootScope.Toaster.Error("Attempt Failed." + result.ErrorMessage);
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }
                $scope.CalculateOrderQty();
            }).error(function (result) {
            });
        }

        $scope.CheckProductEntryKey = function (sourcefrom) {
            var prditem = $scope.ProductList.find(function (item) {
                return (item.id == $scope.ProductRequest.Details.productid);
            });
            $scope.Showitem = prditem != null && prditem.attributes &&
                prditem.attributes.mandatoryitem;

            if (sourcefrom != undefined &&
                sourcefrom == 'dsn') {
                $scope.SearchDesignList();
                $scope['setFocusdsn_colour']();
            }
            else {
                if (!$scope.Showitem)
                    $scope['setFocusbrand']();
                else
                    $scope['setFocusitemid']();

                if ($scope.SupportDesignList) {

                    if ($scope.ProductRequest.supplierid == undefined ||
                        $scope.ProductRequest.supplierid == null ||
                        $scope.ProductRequest.supplierid == 0) {
                        $rootScope.Toaster.Error("Please select supplier!");
                        return;
                    }
                    $scope.SearchDesignList();
                }
            }
        }

        $scope.ChangeOrderStatus = function () {
            if ($scope.ProductRequest.id == null || $scope.ProductRequest.id == 0) {
                $rootScope.Toaster.Error("Please select purchase order!");
                return;
            }

            var request = {
                "item": $scope.ProductRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchaseorderService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Status updated successfully");

            }).error(function (result) {
            });
        }

        $scope.ProductItemChange = function () {
            var selecteditem = $scope.ItemList.find(function (item) {
                return item.id == $scope.ProductRequest.Itemid;
            });
            if (typeof (selecteditem) != 'undefined' && selecteditem != null) {
                $scope.ProductRequest.Details.brandid = selecteditem.brandid;
                $scope.ProductRequest.Details.colourid = selecteditem.colourid;
                $scope.ProductRequest.Details.sizeid = selecteditem.sizeid;
                $scope.ProductRequest.Details.patternid = selecteditem.patternid;
                $scope.ProductRequest.Details.styleid = selecteditem.styleid;
                $scope.ProductRequest.Details.fitid = selecteditem.fitid;
                $scope.ProductRequest.Details.sleeveid = selecteditem.sleeveid;
                $scope.ProductRequest.Details.materialid = selecteditem.materialid;
            }
            $scope['setFocusstyle']();
        }

        $scope.CheckModeAndFocus = function (from) {
            if (from == 'type') {
                if ($scope.ProductRequest.mode == 3)
                    $scope['setFocusdesignpattern']();
                else
                    $scope['setFocuspattern']();
            }
            else if (from == 'design') {
                if ($scope.ProductRequest.mode == 2)
                    $scope['setFocussize']();
                if ($scope.ProductRequest.mode == 1)
                    $scope['setFocussizegroup']();
                else
                    $scope['setFocussizerange']();
            }
        }

        $scope.AddItem = function () {
            if ($scope.ProductRequest.companyid == null || $scope.ProductRequest.companyid == 0) {
                $scope.Toaster.Info("Please enter company");
                return;

            }
            if ($scope.ProductRequest.deliverylocationid == null || $scope.ProductRequest.deliverylocationid == 0) {
                $scope.Toaster.Info("Please enter delivery location");
                return;

            }
            if ($scope.ProductRequest.supplierid == null || $scope.ProductRequest.supplierid == 0) {
                $scope.Toaster.Info("Please enter order location");
                return;

            }
            if ($scope.ProductRequest.Details.productid == null || $scope.ProductRequest.Details.productid == 0) {
                $scope.Toaster.Info("Please enter product");
                return;

            }
            if ($scope.ProductRequest.Details.oqty == null || $scope.ProductRequest.Details.oqty <= 0) {
                $scope.Toaster.Info("Please enter order qty");
                return;
            }


            $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
            $scope.ProductRequest.Details.printingname = $scope.SearchRef($scope.ProductRequest.Details.productid, $scope.ProductList, '');
            $scope.ProductRequest.Details.sizename = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.SizeList, '');
            $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
            $scope.ProductRequest.Details.materialname = $scope.SearchRef($scope.ProductRequest.Details.materialid, $scope.MaterialList, '');
            $scope.ProductRequest.Details.stylename = $scope.SearchRef($scope.ProductRequest.Details.styleid, $scope.StyleList, '');
            $scope.ProductRequest.Details.colourname = $scope.SearchRef($scope.ProductRequest.Details.colourid, $scope.ColorList, '');
            $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');
            $scope.ProductRequest.Details.patternname = $scope.SearchRef($scope.ProductRequest.Details.patternid, $scope.PatternList, '');
            $scope.ProductRequest.Details.sleevename = $scope.SearchRef($scope.ProductRequest.Details.sleeveid, $scope.SleeveList, '');
            $scope.ProductRequest.Details.fitname = $scope.SearchRef($scope.ProductRequest.Details.fitid, $scope.FitList, '');

            var description = [];
            if ($scope.ProductRequest.Details.printingname != '')
                description.push($scope.ProductRequest.Details.printingname);
            if ($scope.ProductRequest.Details.brandname != '')
                description.push($scope.ProductRequest.Details.brandname);
            if ($scope.ProductRequest.Details.stylename != '')
                description.push($scope.ProductRequest.Details.stylename);
            if ($scope.ProductRequest.Details.patternname != '')
                description.push($scope.ProductRequest.Details.patternname);
            if ($scope.ProductRequest.Details.typename != '')
                description.push($scope.ProductRequest.Details.typename);
            if ($scope.ProductRequest.Details.materialname != '')
                description.push($scope.ProductRequest.Details.materialname);
            if ($scope.ProductRequest.Details.colourname != '')
                description.push($scope.ProductRequest.Details.colourname);
            if ($scope.ProductRequest.Details.sleevename != '')
                description.push($scope.ProductRequest.Details.sleevename);
            if ($scope.ProductRequest.Details.fitname != '')
                description.push($scope.ProductRequest.Details.fitname);

            if ($scope.ProductRequest.potype < 0 &&
                $scope.ProductRequest.Details.description &&
                $scope.ProductRequest.Details.description != null &&
                $scope.ProductRequest.Details.description != '') {
                $scope.ProductRequest.Details.size = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.SizeList, '');
                description = [$scope.ProductRequest.Details.description];
                $scope.ProductRequest.Details.pricerange = $scope.ProductRequest.Details.price;

                var exist = $scope.ProductRequest.Purchaseorderitems.find(function (item) {
                    return item.gln == $scope.ProductRequest.Details.gln;
                });

                if (exist && exist != null) {
                    $rootScope.Toaster.Error($scope.ProductRequest.Details.gln + " already added in the order");
                    return;
                }
            }

            if ($scope.ProductRequest.potype == -9 &&
                $scope.ProductRequest.Details.imagechanged) {
                $scope.SaveProductDesignImage();
            }

            item = {
                'productid': $scope.ProductRequest.Details.productid,
                'oqty': $scope.ProductRequest.Details.oqty,
                'designperqty': $scope.ProductRequest.Details.designperqty,
                'designqty': $scope.ProductRequest.Details.designqty,
                'deliverymode': $scope.ProductRequest.Details.deliverymode,
                'cqty': 0,
                'rqty': 0,
                'isactive': true,
                'version': 1,
                'gln': $scope.ProductRequest.Details.gln,
                'imageid': $scope.ProductRequest.Details.imageid,
                'styleid': $scope.ProductRequest.Details.styleid,
                'materialid': $scope.ProductRequest.Details.materialid,
                'brandid': $scope.ProductRequest.Details.brandid,
                'typeid': $scope.ProductRequest.Details.typeid,
                'sleeveid': $scope.ProductRequest.Details.sleeveid,
                'fitid': $scope.ProductRequest.Details.fitid,
                'patternid': $scope.ProductRequest.Details.patternid,
                'colourid': $scope.ProductRequest.Details.colourid,
                'sizeid': $scope.ProductRequest.Details.sizeid,
                'materialname': $scope.ProductRequest.Details.materialname,
                'stylename': $scope.ProductRequest.Details.stylename,
                'printingname': $scope.ProductRequest.Details.printingname,
                'brandname': $scope.ProductRequest.Details.brandname,
                'sizename': $scope.ProductRequest.Details.sizename,
                'typename': $scope.ProductRequest.Details.typename,
                'sleevename': $scope.ProductRequest.Details.sleevename,
                'fitname': $scope.ProductRequest.Details.fitname,
                'patternname': $scope.ProductRequest.Details.patternname,
                'colourname': $scope.ProductRequest.Details.colourname,
                'size': $scope.ProductRequest.Details.size,
                'price': $scope.ProductRequest.Details.price,
                'mrp': $scope.ProductRequest.Details.mrp,
                'sellingprice': $scope.ProductRequest.Details.sellingprice,
                'pricerange': $scope.ProductRequest.Details.pricerange,
                'designid': $scope.ProductRequest.Details.designid,
                'jumpqty': $scope.JumpList,
                'description': description.join('/'),
                'itemid': $scope.ProductRequest.Details.itemid,
                'id': 0
            };

            if (typeof (item.imageid) != 'undefined' && item.imageid != null &&
                item.imageid.length > 0)
                $scope.ProductRequest.ShowImage = true;

            if ($scope.ProductRequest.mode == 2 || $scope.ProductRequest.mode == 3
                || $scope.ProductRequest.mode == 4)
                item.size = $scope.ProductRequest.Details.sizename;
            else
                $scope.ProductRequest.Details.sizename = item.size;

            if ($scope.ProductRequest.Purchaseorderitems.length == 0)
                $scope.Display.setList($scope.ProductRequest.Purchaseorderitems);
            //$scope.ProductRequest.Purchaseorderitems.push(item);
            $scope.Display.addItem(item);

            $scope.ProductRequest.Details.oqty = 0;
            $scope.ProductRequest.Details.id = 0;
            $scope.ProductRequest.Details.imageid = '';
            $scope.ProductRequest.Details.designperqty = 0;
            $scope.ProductRequest.Details.designqty = 0;
            $scope.ProductRequest.Details.deliverymode = 0;
            $scope.ProductRequest.Details.price = '';
            $scope.ProductRequest.Details.mrp = '';
            $scope.ProductRequest.Details.sellingprice = '';
            $scope.ProductRequest.Details.gln = '';
            $scope.ProductRequest.Details.description = '';

            $scope.JumpList = [];
            $scope.CalculateOrderQty();

            if ($scope.ProductRequest.potype == -1) {
                $scope.setFocushserialno();
            }
        };

        $scope.CalculateOrderQty = function () {
            var orderqty = 0;
            var ordervalue = 0;
            var ordertax = 0;
            var ordercess = 0;
            $.each($scope.ProductRequest.Purchaseorderitems, function (k, v) {
                if ($rootScope.GetNumber(v.oqty) == 'undefined' || $rootScope.GetNumber(v.oqty) == 0)
                    v.oqty = 0;
                orderqty += $rootScope.GetNumber(v.oqty);
                ordervalue += $rootScope.GetNumber(v.amount);
                ordertax += $rootScope.GetNumber(v.tax);
                ordercess += $rootScope.GetNumber(v.cessvalue);
            });
            $scope.orderqty = orderqty;
            $scope.ordervalue = ordervalue;
            $scope.ordertax = ordertax;
            $scope.ordercess = ordercess;
        };

        $scope.DesignQtyChanged = function () {
            $scope.ProductRequest.Details.oqty = $scope.ProductRequest.Details.designqty * $scope.ProductRequest.Details.designperqty;
        }

        $scope.ManageItem = function (type, item) {
            if (type == 'Edit') {

                $scope.JumpList = [];
                if (typeof (item.internalList) != 'undefined' &&
                    item.internalList != null) {
                    $.each(item.internalList, function (k, v) {
                        $scope.JumpList.push({ 'id': v.sizeid, 'name': v.sizename, 'qty': v.oqty, 'index': k });
                    });
                }

                if (typeof (item.id) == 'undefined' || item.id == 0) {
                    $scope.ProductRequest.Details = item.Details || {};
                    $scope.ProductRequest.Details.oqty = item.oqty;

                    $scope.ProductRequest.Details.productid = item.productid;
                    $scope.ProductRequest.Details.gln = item.gln;
                    $scope.ProductRequest.Details.mrp = item.mrp;
                    $scope.ProductRequest.Details.sellingprice = item.sellingprice;
                    $scope.ProductRequest.Details.imageid = item.imageid;
                    $scope.ProductRequest.Details.styleid = item.styleid;
                    $scope.ProductRequest.Details.materialid = item.materialid;
                    $scope.ProductRequest.Details.brandid = item.brandid;
                    $scope.ProductRequest.Details.typeid = item.typeid;
                    $scope.ProductRequest.Details.sleeveid = item.sleeveid;
                    $scope.ProductRequest.Details.fitid = item.fitid;
                    $scope.ProductRequest.Details.patternid = item.patternid;
                    $scope.ProductRequest.Details.colourid = item.colourid;
                    $scope.ProductRequest.Details.sizeid = item.sizeid;
                    $scope.ProductRequest.Details.materialname = item.materialname;
                    $scope.ProductRequest.Details.stylename = item.stylename;
                    $scope.ProductRequest.Details.printingname = item.printingname;
                    $scope.ProductRequest.Details.brandname = item.brandname;
                    $scope.ProductRequest.Details.sizename = item.sizename;
                    $scope.ProductRequest.Details.typename = item.typename;
                    $scope.ProductRequest.Details.sleevename = item.sleevename;
                    $scope.ProductRequest.Details.fitname = item.fitname;
                    $scope.ProductRequest.Details.patternname = item.patternname;
                    $scope.ProductRequest.Details.colourname = item.colourname;
                    $scope.ProductRequest.Details.size = item.size;
                    $scope.ProductRequest.Details.pricerange = item.pricerange;
                    $scope.ProductRequest.Details.designid = item.designid;
                    $scope.ProductRequest.Details.id = 0;
                    $scope.JumpList = item.jumpqty;
                    $scope.ProductRequest.Details.designperqty = item.designperqty;
                    $scope.ProductRequest.Details.designqty = item.designqty;
                    $scope.ProductRequest.Details.deliverymode = item.deliverymode;
                    //$scope.Display.deleteItem(item);
                    $scope.CalculateOrderQty();
                    $scope.setFocusproduct();
                }
                else {
                    $scope.ProductRequest.Details = item.Details || {};
                    $scope.ProductRequest.Details.itemid = item.itemid;
                    $scope.ProductRequest.Details.productid = item.productid;
                    $scope.ProductRequest.Details.designperqty = item.designperqty;
                    $scope.ProductRequest.Details.designqty = item.designqty;
                    $scope.ProductRequest.Details.deliverymode = item.deliverymode;
                    if (typeof (item.Details) == 'undefined') {
                        $scope.ProductRequest.Details.productid = item.productid;
                        $scope.ProductRequest.Details.brandid = item.brandid;
                        $scope.ProductRequest.Details.sizeid = item.sizeid;
                        $scope.ProductRequest.Details.patternid = item.patternid;
                        $scope.ProductRequest.Details.styleid = item.styleid;
                        $scope.ProductRequest.Details.colourid = item.colourid;
                        $scope.ProductRequest.Details.typeid = item.typeid;
                        $scope.ProductRequest.Details.sleeveid = item.sleeveid;
                        $scope.ProductRequest.Details.fitid = item.fitid;
                    }
                    if (typeof (item.itemid) == 'undefined') {
                        $scope.ProductRequest.Details.oqty = item.oqty;
                        $scope.ProductRequest.Details.size = item.size;
                        $scope.ProductRequest.Details.pricerange = item.pricerange;
                        $scope.ProductRequest.Details.designid = item.designid;
                        $scope.ProductRequest.Details.designperqty = item.designperqty;
                        $scope.ProductRequest.Details.designqty = item.designqty;
                        $scope.ProductRequest.Details.deliverymode = item.deliverymode;
                        $scope.Display.deleteItem(item);
                        $scope.CalculateOrderQty();
                        $scope.setFocusproduct();
                    }
                    else {
                        var request = {
                            "Item": { 'id': item.itemid },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "ItemsService/Search",
                            data: srequest
                        }).success(function (result) {
                            if (result.length == 1) {
                                $scope.ProductRequest.Details = result[0];
                                $scope.ProductRequest.Details.oqty = item.oqty;
                                $scope.ProductRequest.Details.id = item.id;
                                $scope.ProductRequest.Details.itemid = item.itemid;
                                $scope.ProductRequest.Details.size = item.size;
                                $scope.ProductRequest.Details.pricerange = item.pricerange;
                                $scope.ProductRequest.Details.designid = item.designid;
                                $scope.ProductRequest.Details.designperqty = item.designperqty;
                                $scope.ProductRequest.Details.designqty = item.designqty;
                                $scope.ProductRequest.Details.deliverymode = item.deliverymode;
                                $scope.Display.deleteItem(item);
                                $scope.CalculateOrderQty();
                                $scope.setFocusproduct();
                            }
                        });
                    }
                }
            } else if (type == 'Delete') {
                if (item.rqty > 0) {
                    $rootScope.Toaster.Error("Item Can't be deleted!! Item already received from the supplier.")
                    return;
                }
                else
                    $scope.Display.deleteItem(item);
                $scope.CalculateOrderQty();
            } else if (type == 'Cancel') {
                item.cqty += item.oqty - item.rqty;

                var request = {
                    "Item": item,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PurchaseorderitemsService/Update",
                    data: srequest
                }).success(function (result) {
                    if (result.id > 0) {
                        item.version = result.version;
                    }
                });
            }
        };

        $scope.ManageSearchItem = function (command, item) {
            if (command == 'Edit') {
                $scope.ProductRequest = item;


                var request = {
                    "Item": {
                        'purchaseorderid': item.id
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PurchaseorderitemsService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length > 0) {
                        $scope.ProductRequest.Purchaseorderitems = result;
                        $scope.Display.setList($scope.ProductRequest.Purchaseorderitems);
                        if (item.potype == -3 || item.potype == -4) {
                            $timeout(function () {
                                $scope.ConfigureVendorItemGrid();
                                $scope.SupplierChanged();
                            }, 50);
                        }
                        if (item.potype == -5) {
                            $timeout(function () {
                                $scope.ConfigureUANItemGrid();
                                var newitems = [];
                                $.each($scope.ProductRequest.Purchaseorderitems, function (k, v) {
                                    newitems.push({
                                        'Design': v.designid,
                                        'Product': v.productname,
                                        'Size': v.sizename,
                                        'UAN': v.gln,
                                        'Percentage': v.taxpercentage,
                                        'Qty': v.oqty,
                                        'Cost': v.price,
                                        'Tax': v.tax * v.oqty,
                                        'MRP': v.mrp,
                                        'id': v.id,
                                        'item': v
                                    });
                                })
                                $scope.gridvendororderitem.UpdateData(newitems);
                            }, 50);
                        }
                    }
                    $scope.ShowSearch = false;
                    $scope.Navigation.SetMode('EDIT');
                    $scope.CalculateOrderQty();
                });

            } else if (command == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ?',
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
                                    url: "PurchaseorderService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $rootScope.Toaster.Error("Successfully deleted");
                                        //$scope.ErrorMessage = "Successfully deleted";
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
                /*ngDialog.open({
                    preCloseCallback: function (value) {
                        if (confirm('Do you want to delete?')) {
                            return true;
                        }
                        return false;
                    }
                });*/
            }
        };

        $scope.qtyChanged = function () {
            $scope.ProductRequest.Details.oqty = 0;
            $.each($scope.JumpList, function (k, v) {
                if ($rootScope.GetNumber(v.qty) == 'undefined' || $rootScope.GetNumber(v.qty) == 0)
                    $scope.ProductRequest.Details.oqty += 0;
                else
                    $scope.ProductRequest.Details.oqty += $rootScope.GetNumber(v.qty);
            });
        }

        $scope.CreateSupplier = function () {

            var dialog = ngDialog.open({
                template: 'SupplierMaster',
                data: { 'scope': $scope },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {

                    $scope.Supplier = { 'isactive': true, 'name': '', 'cityid': 0, 'agentid': 0 };
                    $scope.Agent = $scope.ngDialogData.scope.Agent;
                    $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
                        $scope.CityList = results;
                    });


                    $scope.Create = function () {

                        $scope.Supplier.companylist = [];
                        $scope.Supplier.companylist.push({
                            'cityid': $scope.Supplier.cityid, 'code': $scope.Supplier.code, 'name': $scope.Supplier.name,
                            'agentid': $scope.Supplier.agentid, 'isactive': true, 'contactperson': $scope.Supplier.contactperson,
                            'companycontactno': $scope.Supplier.contactnumber
                        });

                        var request = {
                            "item": $scope.Supplier,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "SupplierService/Save",
                            data: srequest
                        }).success(function (result) {
                            if (result.id > 0) {
                                $scope.closeThisDialog(result);
                            }
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.Suppliers.push(data.value);
                        $timeout(function () {
                            $scope.ProductRequest.supplierid = data.value.id;
                        }, 10)
                    }
                }
            });

        }

        $scope.CheckSizeGroup = function () {

            if (typeof ($scope.ProductRequest.Details.sizegroupid) == 'undefined' || $scope.ProductRequest.Details.sizegroupid == null)
                return;
            var sizegroup = $scope.SizeGroupList.find(function (item) {
                return item.id == $scope.ProductRequest.Details.sizegroupid;
            });
            if (typeof (sizegroup) == 'undefined' || sizegroup == null)
                return;

            var idList = [];
            $.each(sizegroup.list, function (k, v) {
                var size = $scope.SizeList.find(function (item) {
                    return item.id == v.id;
                });
                idList.push({
                    'id': size.id, 'name': size.name, 'qty': 0, 'index': k
                });
            });

            idList.sort(function (a, b) {
                return a.index - b.index;
            });

            $scope.JumpList = idList;
            //$scope.ProductRequest.Details.qty = 0;
            $timeout(function () {
                $('#jumplist INPUT').first().focus();
            }, 10);
        }

        $scope.CheckSize = function () {

            if (typeof ($scope.ProductRequest.Details.size) != 'undefined' &&
                $scope.ProductRequest.Details.size != null && $scope.ProductRequest.mode == 0) {
                $scope.GetJUMPInformation($scope.ProductRequest.Details.size);
            }

            //$rootScope.Prompt('Jump', 'Jump (start-increment-end)', function (count) {
            //});
        }

        $scope.findDesignQty = function () {
            if ($scope.ProductRequest.Details.designid &&
                $scope.ProductRequest.Details.designid != null) {
                $scope.ProductRequest.Details.designqty = $scope.ProductRequest.Details.designid.split(',').length;
                $scope.DesignQtyChanged();
            }

            $scope.setFocusdeliverymode();
        }

        $scope.GetJUMPInformation = function (count) {

            if (count.length > 0) {

                var sizeidList = count.split(',');
                var sizeList = sizeidList[0].split('-');
                if (sizeList.length == 2 && /^[a-zA-Z2-6]*$/.test(sizeList[0])) {

                    var sizeIndex = '3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL'.split(',');
                    var startsize = sizeIndex.indexOf(sizeList[0]);
                    var endsize = sizeIndex.indexOf(sizeList[1]);
                    if (startsize < 0 || endsize < 0) {
                        sizeIndex = '3XS,XXS,XS,S,M,L,XL,2XL,3XL,4XL,5XL,6XL,7XL'.split(',');
                        startsize = sizeIndex.indexOf(sizelist[0]);
                        endsize = sizeIndex.indexOf(sizelist[1]);
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

                var idList = [];
                $.each($scope.SizeList, function (k, v) {
                    $.each(sizeList, function (kk, vv) {
                        if (vv == v.name) {
                            if (idList.find(function (item) {
                                return item.name == v.name;
                            }) == null) {
                                idList.push({ 'id': v.id, 'name': v.name, 'qty': 0, 'index': kk });
                            }
                        }
                    });
                });

                idList.sort(function (a, b) {
                    return a.index - b.index;
                });

                $scope.JumpList = idList;
                //$scope.ProductRequest.Details.qty = 0;
                $('#jumplist INPUT').first().focus();
            }
        }

        $scope.SearchItem = function () {

            if ($scope.ProductRequest.Details.productid == null || $scope.ProductRequest.Details.productid == 0) {
                $scope.Toaster.Info("Please select product");
                return;
            }

            $scope.ProductRequest.Details.id = 0;
            $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
            $scope.ProductRequest.Details.printingname = $scope.SearchRef($scope.ProductRequest.Details.productid, $scope.ProductList, '');
            $scope.ProductRequest.Details.patternname = $scope.SearchRef($scope.ProductRequest.Details.patternid, $scope.PatternList, '');
            $scope.ProductRequest.Details.materialname = $scope.SearchRef($scope.ProductRequest.Details.materialid, $scope.MaterialList, '');
            $scope.ProductRequest.Details.colourname = $scope.SearchRef($scope.ProductRequest.Details.colourid, $scope.ColorList, '');
            $scope.ProductRequest.Details.sizename = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.SizeList, '');
            $scope.ProductRequest.Details.stylename = $scope.SearchRef($scope.ProductRequest.Details.styleid, $scope.StyleList, '');
            $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');
            $scope.ProductRequest.Details.sleevename = $scope.SearchRef($scope.ProductRequest.Details.sleeveid, $scope.SleeveList, '');
            $scope.ProductRequest.Details.fitname = $scope.SearchRef($scope.ProductRequest.Details.fitid, $scope.FitList, '');
            $scope.ProductRequest.Details.autocreate = true;

            var request = {
                "Item": $scope.ProductRequest.Details,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length == 0) {
                    $rootScope.Toaster.Info("No Item found. Please create the item");
                    return;
                }
                if (result.length == 1) {
                    $scope.ProductRequest.Details = result[0];
                    $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify($scope.ProductRequest.Details)));
                }
                else if (result.length > 1)
                    $scope.ShowProductSearch(result);
            });
        };

        $scope.SearchItemByName = function () {
            var request = {
                "Item": {
                    'name': $scope.ProductRequest.Details.printingname, 'productid': $scope.ProductRequest.Details.productid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/SearchByName",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {
                    $scope.ProductRequest.Details = result[0];
                    $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify($scope.ProductRequest.Details)));
                }
                else if (result.length > 1)
                    $scope.ShowProductSearch(result);
            });
        };

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
        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'PURCHASE ORDER'
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

        $scope.Search = function () {
            var request = {
                "Item": {
                    'supplierid': $scope.SearchCondition.supplierid,
                    'pono': $scope.SearchCondition.pono,
                    'potype': $scope.SearchCondition.potype,
                    'podate': $scope.SearchCondition.podate,
                    'expectedate': $scope.SearchCondition.expectedate,
                    'locationid': $scope.SearchCondition.locationid,
                    'companyid': $scope.SearchCondition.companyid,
                    'suppliercityid': $scope.SearchCondition.suppliercityid,
                    'agentid': $scope.SearchCondition.agentid,
                    'statusid': $scope.SearchCondition.statusid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PurchaseorderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ConfigSearchGrid(result);
            });
        };

        $scope.ConfigSearchGrid = function (result) {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('supplierid', 'Supplier', 150, $scope.Suppliers);
            igrid.AddSimpleColumn('suppliercity', 'Supplier City', 100);
            igrid.AddReferenceColumn('agentid', 'Agent', 120, $scope.Agent);
            igrid.AddSimpleColumn('pono', 'PO Number', 80);
            igrid.AddReferenceColumn('potype', 'PO Type', 80, $scope.POType);
            igrid.AddReferenceColumn('status', 'Status', 80, $scope.POStatus);
            igrid.AddSimpleColumn('employeeincharge', 'PO Incharge', 100);
            igrid.AddReferenceColumn('agreementpoid', 'Agreement PO', 80, $scope.AgreementList);
            //igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('deliverylocationid', 'Delivery Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('requestlocationid', 'Requeste Location', 100, $scope.LocationList);
            igrid.AddFormatedColumn('podate', 'PO Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expectedate', 'Due Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('overduedays', 'Over Due Days', 80, 'N0', 0);
            igrid.AddSimpleColumn('remarks', 'Remark', 80);

            var commands = [];
            if ($scope.UserInfo.p_PURCHASEORDER.delete)
                commands.push("<input type='button' class='btn btn-primary btn-xs' value='Delete'/>");

            if ($scope.UserInfo.p_PURCHASEORDER.modify)
                commands.push("<input type='button' class='btn btn-primary btn-xs' value='Edit'/>");

            if ($scope.UserInfo.p_PURCHASEORDER.print)
                commands.push("<input type='button' class='btn btn-primary btn-xs' value='Print'/>");

            if (commands.length > 0) {
                igrid.AddButtonColumn('action', 'ACTION', 175, commands.join(' &nbsp;'), function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Print')
                            $scope.PrintPO(args.item.id);
                    });
                });
            }

            //igrid.options.showHeaderRow = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result);
        }

        $scope.ShowProductSearch = function (list, callback, iteminfo) {
            $.each(list, function (k, v) {
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
                if (v.sleevename != null && v.sleevename.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.sleevename;
                if (v.fitname != null && v.fitname.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.fitname;
            });

            var dialog = ngDialog.open({
                template: 'ProductSearch',
                data: { 'scope': $scope, 'list': list, 'iteminfo': iteminfo, 'callback': callback },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.PopupList = $scope.ngDialogData.list;
                    $scope.ShowCreate = $scope.PopupList.length <= 1;
                    $scope.printingname = '';
                    $scope.Create = function () {

                        var coreScope = $scope.ngDialogData.scope;
                        var item = coreScope.ProductRequest.Details;
                        item.brandname = coreScope.SearchRef(item.brandid, coreScope.BrandName, '');
                        item.productname = coreScope.SearchRef(item.productid, coreScope.Products, '');
                        item.patternname = coreScope.SearchRef(item.patternid, coreScope.Patterns, '');
                        item.materialname = coreScope.SearchRef(item.materialid, coreScope.Material, '');
                        item.colourname = coreScope.SearchRef(item.colourid, coreScope.Colors, '');
                        item.typename = coreScope.SearchRef(item.typeid, coreScope.TypeList, '');
                        item.sleevename = coreScope.SearchRef(item.sleeveid, coreScope.SleeveList, '');
                        item.fitname = coreScope.SearchRef(item.fitid, coreScope.FitList, '');
                        item.sizename = coreScope.SearchRef(item.sizeid, coreScope.Sizes, '');
                        item.stylename = coreScope.SearchRef(item.styleid, coreScope.Styles, '');
                        item.printingname = $scope.printingname;

                        var request = {
                            "item": item,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "ItemsService/Save",
                            data: srequest
                        }).success(function (result) {
                            if (result.id > 0) {
                                $scope.ngDialogData.result = result;
                                $scope.closeThisDialog($scope.ngDialogData);
                            }
                        });

                    };
                    $scope.Selected = function (item) {
                        $scope.ngDialogData.result = item;
                        $scope.closeThisDialog($scope.ngDialogData);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        if (typeof (data.value.callback) != 'undefined') {
                            data.value.iteminfo.printingname = data.value.result.printingname;
                            data.value.callback(data.value.iteminfo, data.value.result);
                        }
                        else {
                            $scope.ProductRequest.Details = data.value.result;
                            $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify(data.value.result)));
                        }
                    }
                }
            });
        }

        $scope.Publish = function () {
            restService.Get('PurchaseorderService/Publish/' + $scope.UserInfo.OrganizationID + '/' + $scope.ProductRequest.id, true).then(function (results) {
                if (results != null && results != 'undefined' && results == 'true')
                    $rootScope.Toaster.Info("Purchase Order Sent Successfully.")
                else
                    $rootScope.Toaster.Info("Error in Sending Purchase Order.")
            });
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == 'posummary')
                subtitle = 'PURCHASE ORDER - SUMMARY REPORT';
            else if (tableid == 'podetails')
                subtitle = 'PURCHASE ORDER - ITEM DETAILS REPORT';
            //subtitle = 'PURCHASE ORDER - SUMMARY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company) + " ) ";

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

        $scope.PrintPO = function (id) {
            $rootScope.Download('PurchaseorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + id, 'po.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "po.pdf";
                link.href = 'PurchaseorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }
        $scope.Print = function () {
            $rootScope.Download('PurchaseorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.ProductRequest.id, 'po.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "po.pdf";
                link.href = 'PurchaseorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.ProductRequest.id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.SearchPOSummary = function () {
            $scope.totalrecords = 0;
            $scope.POList = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchaseorderService/SearchPOSummary",
                data: srequest
            }).success(function (result) {
                $scope.POList = result;
                $scope.totalrecords = result.length;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

                $scope.POConfig(false);
            }).error(function (result) {
            });
        }

        $scope.SearchAgreementPOSummary = function () {
            $scope.totalrecords = 0;
            $scope.POList = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchaseorderService/SearchAgreementPOSummary",
                data: srequest
            }).success(function (result) {
                $scope.POList = result;
                $scope.totalrecords = result.length;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }
                $scope.POConfig(true);
            }).error(function (result) {
            });
        }

        $scope.ShowOrderItems = function (action, item) {
            $scope.POItemsList = [];
            if (action == "VIEW") {

                request = {
                    "item": {
                        "pono": item.pono,
                        "withinvoice": true
                    },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "PurchaseorderitemsService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.POItemsList = result;
                    //$scope.POOrderDetailConfig();
                    $scope.printDiv('productviewprintdiv', 'PRODUCT ITEM VIEW');

                    if (result == null || result.length == 0) {
                        $rootScope.Toaster.Info("No Results Found.");
                    }
                }).error(function (result) {
                });
            }
        }


        $scope.SearchPODetail = function () {
            $scope.totalrecords = 0;
            $scope.POOrderList = [];

            request = {
                "item": { 'pono': $scope.ProductRequest.pono, 'withinvoice': true },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchaseorderitemsService/Search",
                data: srequest
            }).success(function (result) {
                $scope.POItemsList = result;
                $scope.POOrderDetailConfig();
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

            }).error(function (result) {
            });
        }

        $scope.SearchPendingPO = function () {
            $scope.totalrecords = 0;
            $scope.PendingPOList = [];

            request = {
                "item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PurchaseorderService/SearchPendingPO",
                data: srequest
            }).success(function (result) {
                $scope.PendingPOList = result;
                $scope.totalrecords = result.length;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

                $scope.PendingPOConfig();
            }).error(function (result) {
            });
        }


        $scope.POConfig = function (agreement) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('pono', 'Order No', 80);
            igrid.AddReferenceColumn('potype', 'Order type', 80, $scope.POType);
            igrid.AddFormatedColumn('podate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 150, $scope.Suppliers);
            igrid.AddReferenceColumn('agentid', 'Agent', 120, $scope.Agent);

            if (!agreement)
                igrid.AddReferenceColumn('agreementpoid', 'Agreement Ref', 100, $scope.AgreementList);
            //igrid.AddReferenceColumn('locationid', 'Order Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('deliverylocationid', 'Delivery Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('requestlocationid', 'Request Location', 100, $scope.LocationList);
            igrid.AddFormatedColumn('expectedate', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('tolerance', 'Qty Tolerance %', 100);
            igrid.AddReferenceColumn('agentid', 'Agent Name', 100, $scope.Agent);
            igrid.AddReferenceColumn('status', 'PO Status', 100, $scope.POStatus);
            if (agreement)
                igrid.AddFormatedColumn('agreed', 'Agreed Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('ordered', 'Ordered Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('received', 'Received Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cancelled', 'Cancelled Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('balance', 'Balance Qty', 100, 'N2', 2).total = true;
            igrid.AddSimpleColumn('remarks', 'Remark', 100);

            igrid.AddButtonColumn('action', 'Action', 50, 'View', function (e, args) {

                $scope.$apply(function () {
                    $scope.ShowOrderItems('VIEW', args.item);
                });
                return true;

            });
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.POList);


        }


        $scope.compare = function (obj1, obj2) {
            for (var p in obj1) {
                if (obj1.hasOwnProperty(p) !== obj2.hasOwnProperty(p)) return false;
                if (typeof (obj1[p]) == 'object') continue;
                if (typeof (obj1[p]) == 'function') continue;
                if (p.toLowerCase().startsWith('size')) continue;
                if (obj1[p] != obj2[p]) return false;
            }
        };

        $scope.LoadFromReorder = function (list, locationid) {
            var sizeList = [];
            $scope.ProductRequest = {
                'Details': {}
            };
            $scope.ProductRequest.Purchaseorderitems = [];
            $scope.Display.setList($scope.ProductRequest.Purchaseorderitems);
            $scope.ProductRequest.podate = new Date();
            $scope.ProductRequest.deliverylocationid = locationid;
            $.each(list, function (k, v) {

                var item = {
                    'productid': v.productid,
                    'brandid': v.brandid,
                    'materialid': v.materialid,
                    'styleid': v.styleid,
                    'colourid': v.colourid,
                    'typeid': v.typeid,
                    'sleeveid': v.sleeveid,
                    'fitid': v.fitid,
                    'patternid': v.patternid,
                    'sizeid': v.sizeid
                };
                item.supplierid = v.supplieid;
                item.brandname = $scope.SearchRef(v.brandid, $scope.BrandName, '');
                item.printingname = $scope.SearchRef(v.productid, $scope.ProductList, '');
                item.brandname = $scope.SearchRef(v.brandid, $scope.BrandName, '');
                item.materialname = $scope.SearchRef(v.materialid, $scope.MaterialList, '');
                item.stylename = $scope.SearchRef(v.styleid, $scope.StyleList, '');
                item.colourname = $scope.SearchRef(v.colourid, $scope.ColorList, '');
                item.typename = $scope.SearchRef(v.typeid, $scope.TypeList, '');
                item.sleevename = $scope.SearchRef(v.sleeveid, $scope.SleeveList, '');
                item.fitname = $scope.SearchRef(v.fitid, $scope.FitList, '');
                item.patternname = $scope.SearchRef(v.patternid, $scope.PatternList, '');
                item.sizename = $scope.SearchRef(v.sizeid, $scope.SizeList, '');
                item.size = $scope.SearchRef(v.sizeid, $scope.SizeList, '');
                item.oqty = v.order;
                item.pricerange = v.pricerange;

                item.designid = v.designid;
                item.pricerange = v.pricerange;
                item.oqty = v.order;

                var description = [];
                if (item.printingname != '')
                    description.push(item.printingname);
                if (item.brandname != '')
                    description.push(item.brandname);
                if (item.stylename != '')
                    description.push(item.stylename);
                if (item.patternname != '')
                    description.push(item.patternname);
                if (item.typename != '')
                    description.push(item.typename);
                if (item.materialname != '')
                    description.push(item.materialname);
                if (item.colourname != '')
                    description.push(item.colourname);
                if (item.sleevename != '')
                    description.push(item.sleevename);
                if (item.fitname != '')
                    description.push(item.fitname);

                item.isactive = true;
                item.version = 1;
                item.description = description.join('/');
                $scope.ProductRequest.Purchaseorderitems.push(item);

            });

            $scope.CalculateOrderQty();
        }

        $scope.LoadFromPOBuilder = function (list) {
            var sizeList = [];
            $scope.ProductRequest = {};
            $scope.ProductRequest.Purchaseorderitems = [];
            $scope.Display.setList($scope.ProductRequest.Purchaseorderitems);
            $.each(list, function (k, v) {

                var exist = sizeList.find(function (item) {
                    return $scope.compare(v, item);
                });
                if (typeof (exist) == 'undefined' || exist == null) {
                    v.jumpqty = [];
                    v.jumpqty.push({ 'id': k.sizeid, 'qty': v.order });
                    sizeList.push(v);
                }
                else {
                    exist.jumpqty.push({ 'id': k.sizeid, 'qty': v.order });
                }

            });

            $.each(sizeList, function (k, v) {

                v.brandname = $scope.SearchRef(v.brandid, $scope.BrandName, '');
                v.printingname = $scope.SearchRef(v.productid, $scope.ProductList, '');
                v.brandname = $scope.SearchRef(v.brandid, $scope.BrandName, '');
                v.materialname = $scope.SearchRef(v.materialid, $scope.MaterialList, '');
                v.stylename = $scope.SearchRef(v.styleid, $scope.StyleList, '');
                v.colourname = $scope.SearchRef(v.colourid, $scope.ColorList, '');
                v.typename = $scope.SearchRef(v.typeid, $scope.TypeList, '');
                v.sleevename = $scope.SearchRef(v.sleeveid, $scope.SleeveList, '');
                v.fitname = $scope.SearchRef(v.fitid, $scope.FitList, '');
                v.patternname = $scope.SearchRef(v.patternid, $scope.PatternList, '');

                v.sizename = '';
                v.oqty = 0;
                $.each(v.jumpqty, function (kk, vv) {
                    vv.name = $scope.SearchRef(vv.sizeid, $scope.SizeList, '');
                    if (v.sizename == '')
                        v.sizename = vv.name;
                    else
                        v.sizename += ',' + vv.name;
                    v.oqty = vv.qty;
                });

                var description = [];
                if (v.printingname != '')
                    description.push(v.printingname);
                if (v.brandname != '')
                    description.push(v.brandname);
                if (v.stylename != '')
                    description.push(v.stylename);
                if (v.patternname != '')
                    description.push(v.patternname);
                if (v.typename != '')
                    description.push(v.typename);
                if (v.materialname != '')
                    description.push(v.materialname);
                if (v.colourname != '')
                    description.push(v.colourname);
                if (v.sleevename != '')
                    description.push(v.sleevename);
                if (v.fitname != '')
                    description.push(v.fitname);

                v.isactive = true;
                v.version = 1;
                v.description = description.join('/');
                $scope.ProductRequest.Purchaseorderitems.push(v);

            });

            $scope.CalculateOrderQty();
        }

        $scope.POOrderDetailConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('griddetailContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.POItemsList);

            igrid.AddSimpleColumn('pono', 'PO NUMER', 50);
            igrid.AddReferenceColumn('supplierid', 'SUPPLIER NAME', 120, $scope.Suppliers);
            igrid.AddSimpleColumn('productname', 'PRODUCT NAME', 100);
            igrid.AddSimpleColumn('description', 'DESCRIPTION', 100);
            igrid.AddSimpleColumn('brandname', 'BRAND', 100);
            igrid.AddSimpleColumn('designid', 'DESIGN NO', 100);
            igrid.AddSimpleColumn('pricerange', 'PRICE RANGE');
            igrid.AddSimpleColumn('size', 'SIZE RANGE', 100);
            igrid.AddSimpleColumn('gln', 'UAN', 100);
            igrid.AddSimpleColumn('itbuscode', 'Item Code', 100);
            igrid.AddSimpleColumn('taxpercentage', 'Tax %', 60);
            igrid.AddSimpleColumn('discount', 'Discount %', 60);
            igrid.AddSimpleColumn('discountpercentage', 'Discount', 60);

            igrid.AddFormatedColumn('oqty', 'ORDER QTY', 100, 'N2', 2);
            igrid.AddFormatedColumn('rqty', 'RECEIVED QTY', 80, 'N2', 2);
            igrid.AddFormatedColumn('cqty', 'CANCELLED QTY', 80, 'N2', 2);
            igrid.AddFormatedColumn('bqty', 'BALANCE QTY', 80, 'N2', 2);
            igrid.AddSimpleColumn('invoiceno', 'INVOICE NO', 80);
            igrid.AddFormatedColumn('invoicedate', 'INVOICE DATE', 100, 'dd-MM-yyyy');
            //igrid.AddFormatedColumn('sno', 'INVOICE SNO', 60, 'N2', 0);
            igrid.AddFormatedColumn('aqty', 'INVOICE QTY', 80, 'N2', 2);
            igrid.AddSimpleColumn('colourname', 'COLOUR');
            igrid.AddSimpleColumn('sizename', 'SIZE', 100);
            igrid.AddSimpleColumn('stylename', 'STYLE', 100);
            igrid.AddSimpleColumn('materialname', 'MATERIAL', 100);
            igrid.AddSimpleColumn('patternname', 'PATTERN', 100);
            igrid.AddSimpleColumn('sleevename', 'SLEEVE', 100);
            igrid.AddSimpleColumn('fitname', 'FIT', 100);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.POItemsList);


        }

        $scope.PendingPOConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('pono', 'Order No', 80);
            igrid.AddReferenceColumn('potype', 'Type', 80, $scope.POType);
            igrid.AddFormatedColumn('podate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expectedate', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 150, $scope.Suppliers);
            igrid.AddReferenceColumn('agentid', 'Agent', 120, $scope.Agent);
            //igrid.AddReferenceColumn('locationid', 'Order Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('deliverylocationid', 'Delivery Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('requestlocationid', 'Request Location', 100, $scope.LocationList);
            igrid.AddSimpleColumn('tolerance', 'Qty Tolerance %', 100);
            igrid.AddReferenceColumn('agentid', 'Agent Name', 100, $scope.Agent);
            igrid.AddReferenceColumn('status', 'PO Status', 100, $scope.POStatus);
            igrid.AddReferenceColumn('agreementpoid', 'Agreement Ref', 100, $scope.AgreementList);
            igrid.AddSimpleColumn('productname', 'PRODUCT NAME', 80);
            igrid.AddSimpleColumn('brandname', 'BRAND', 100);
            igrid.AddSimpleColumn('designid', 'DESIGN NO', 100);
            igrid.AddSimpleColumn('colourname', 'COLOUR');
            igrid.AddSimpleColumn('sizename', 'SIZE', 100);
            igrid.AddSimpleColumn('stylename', 'STYLE', 100);
            igrid.AddSimpleColumn('materialname', 'MATERIAL', 100);
            igrid.AddSimpleColumn('patternname', 'PATTERN', 100);
            igrid.AddSimpleColumn('sleevename', 'SLEEVE', 100);
            igrid.AddSimpleColumn('fitname', 'FIT', 100);
            igrid.AddSimpleColumn('description', 'DESCRIPTION', 100);
            igrid.AddSimpleColumn('pricerange', 'PRICE RANGE');
            igrid.AddSimpleColumn('size', 'SIZE RANGE', 100);
            igrid.AddFormatedColumn('ordered', 'Ordered Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('received', 'Received Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cancelled', 'Cancelled Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('balance', 'Balance Qty', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.PendingPOList);


        }

        $scope.Validate = function (id) {
            $state.go('PurchaseInsight', {
                source: 'purchaseorder',
                data: { 'poid': id }
            });
        }

        $scope.printDiv = function (div, title) {

            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
                var titlele = angular.element('#apptitle').context.title = title;

                var docHead = document.head.outerHTML;
                var printContents = document.getElementById(div).outerHTML;
                var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

                var newWin = window.open("", "_blank", winAttr);
                var writeDoc = newWin.document;
                writeDoc.open();
                writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
                writeDoc.close();
                newWin.focus();

                angular.element('#apptitle').context.title = otitle;
            }, 100);

        }


        $scope.Clone = function (list, item) {
            list.push(item);
            return list.slice();
        }

        $scope.ManageAttribute = function (callback) {
            var dialog = ngDialog.open({
                template: 'ManageAttribute',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.RetialLocations = $scope.ngDialogData.scope.RetialLocations;
                    $scope.BuyingCompany = $scope.ngDialogData.scope.BuyingCompany;

                    $scope.AttributeList = [
                        { 'id': 'PRODUCT', 'name': 'Product' },
                        { 'id': 'BRAND', 'name': 'Brand' },
                        { 'id': 'SIZE', 'name': 'Size' },
                        { 'id': 'COLOUR', 'name': 'Colour' },
                        { 'id': 'STYLE', 'name': 'Style' },
                        { 'id': 'PATTERN', 'name': 'Pattern' },
                        { 'id': 'MATERIAL', 'name': 'Material' },
                        { 'id': 'TYPE', 'name': 'Type' },
                        { 'id': 'SUPPLIER', 'name': 'Supplier' },
                        { 'id': 'TRANSPORT', 'name': 'Transport' }]


                    $scope.Selected = function () {
                        $scope.closeThisDialog({ 'type': $scope.type, 'code': $scope.code, 'name': $scope.name });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var request = {
                            "Item": data.value,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "ReferencelistService/ManageAttribute",
                            data: srequest
                        }).success(function (result) {

                            var item = {
                                'id': result.id, 'type': result.type, 'code': result.code,
                                'name': result.name, 'new': true
                            };

                            switch (result.type) {
                                case 'PRODUCT':
                                    $scope.ProductList = $scope.Clone($scope.ProductList, item);
                                    break;
                                case 'BRAND':
                                    $scope.BrandName = $scope.Clone($scope.BrandName, item);
                                    break;
                                case 'SIZE':
                                    $scope.SizeList = $scope.Clone($scope.SizeList, item);
                                    break;
                                case 'COLOUR':
                                    $scope.ColorList = $scope.Clone($scope.ColorList, item);
                                    break;
                                case 'STYLE':
                                    $scope.StyleList = $scope.Clone($scope.StyleList, item);
                                    break;
                                case 'PATTERN':
                                    $scope.PatternList = $scope.Clone($scope.PatternList, item);
                                    break;
                                case 'MATERIAL':
                                    $scope.MaterialList = $scope.Clone($scope.MaterialList, item);
                                    break;
                                case 'TYPE':
                                    $scope.TypeList = $scope.Clone($scope.TypeList, item);
                                    break;
                                case 'SUPPLIER':
                                    $scope.Suppliers = $scope.Clone($scope.Suppliers, item);
                                    break;
                                case 'TRANSPORT':
                                    $scope.Transport = $scope.Clone($scope.Transport, item);
                                    break;
                            }
                        }).error(function (result) {
                        });

                    }
                }
            });
        }

        $scope.UItemList = [];
        $scope.gproductChange = function () {
            var request = {
                "Item": {
                    'productid': $scope.ProductRequest.Details.productid,
                    'onlysearchable': true
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.UItemList = result;
                    $timeout(function () {
                        $scope.setFocusgitemid();
                    }, 20);
                }
            });
        }

        $scope.SerialNoEntered = function (event) {
            var request = {
                "Item": { 'gln': $scope.ProductRequest.Details.gln },
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "ItemsService/Search",
                data: request
            }).success(function (result) {
                if (result != null &&
                    result.length > 0) {
                    $scope.ProductRequest.Details.productid = result[0].productid;
                    $scope.SelectItem(result[0]);
                    $scope.setFocushbuyingqty();
                }
            });
        }

        $scope.productItemChange = function () {
            if ($scope.ProductRequest.Details == undefined ||
                $scope.ProductRequest.Details == null) {
                $scope.ProductRequest.Details = {};
            }
            var item = $scope.UItemList.find(function (item) {
                return item.id == $scope.ProductRequest.Details.itemid;
            });
            if (item && item != null) {
                $scope.ProductRequest.ProductID = item.productid;
                $scope.ProductRequest.Details.gln = item.gln;
                $scope.SelectItem(item);
            }
        }

        $scope.VendorProductList = [];
        $scope.VendorAgreementList = [];
        $scope.TradeAgreementItems = [];
        $scope.DailyPriceGroup = null;
        $scope.SupplierChanged = function () {
            if ($scope.ProductRequest.potype == -3) {
                if ($scope.ProductRequest.supplierid &&
                    $scope.ProductRequest.supplierid > 0) {
                    restService.Get('SupplierproductgroupsService/ActiveProducts/' + $scope.ProductRequest.supplierid, true).then(function (results) {
                        var TradeAgreementItems = [];
                        var uniqueid = 1;
                        $.each(results, function (k, v) {
                            var prd = $scope.ProductList.find(function (item) {
                                return item.id == v.productgroupid;
                            });
                            if (prd && prd != null) {
                                v.productid = v.productgroupid;
                                v.name = prd.name;
                            }
                            if (v.priceagreements == undefined || v.priceagreements == null)
                                v.priceagreements = [];

                            $.each(v.priceagreements, function (kk, vv) {
                                if (vv.id == undefined)
                                    vv.id = kk;
                            });

                            if (v.tradeagreements == undefined || v.tradeagreements == null)
                                v.tradeagreements = [];

                            $.each(v.tradeagreements, function (kk, vv) {
                                if (vv.items == undefined || vv.items == null)
                                    vv.items = [];

                                $.each(vv.items, function (kkk, vvv) {
                                    vvv.tradeagreementid = vv.id;
                                    vvv.brandid = vv.brandid;
                                    vvv.productid = v.productid;
                                    vvv.itbuscode = vv.itemcode;
                                    if (vvv.price == 0) vvv.price = vv.price;
                                    if (vvv.mrp == 0) vvv.mrp = vv.mrp;
                                    if (vvv.cost == 0) vvv.cost = vv.cost;
                                    vvv.uniqueid = uniqueid++;
                                    vvv.displayname = vvv.printingname + ' - ' + vvv.gln;
                                    TradeAgreementItems.push(vvv);
                                })
                            });
                        });
                        $scope.VendorProductList = results;
                        $scope.TradeAgreementItems = TradeAgreementItems;
                    });
                }
            }

            if ($scope.ProductRequest.potype == -9) {
                if ($scope.ProductRequest.Details.designid != undefined &&
                    $scope.ProductRequest.Details.designid != null &&
                    $scope.ProductRequest.Details.designid != '' &&
                    $scope.ProductRequest.Details.designid != 'ADD DESIGN') {
                    $scope.ShowProductDesignImage();
                }
            }

            if ($scope.ProductRequest.potype == -4) {
                if ($scope.ProductRequest.supplierid &&
                    $scope.ProductRequest.supplierid > 0) {
                    if ($scope.DailyPriceGroup == null) {
                        restService.Get('ReferencelistService/DAILYPRICEGROUP', true).then(function (results) {
                            $scope.DailyPriceGroup = results;
                        });
                    }
                }
            }

        }

        $scope.VendorAgreement = null;
        $scope.VendorOrderItems = [];
        $scope.ShowVenodrItems = function () {
            $scope.VendorAgreement = $scope.VendorAgreementList.find(function (item) {
                return item.id == $scope.ProductRequest.Details.agreementid;
            });
            if ($scope.VendorAgreement && $scope.VendorAgreement != null) {

                if ($scope.additionalgrid && $scope.additionalgrid != null)
                    $scope.additionalgrid.destroy();

                if ($scope.itemsgrid && $scope.itemsgrid != null)
                    $scope.itemsgrid.destroy();
                var igrid = new InfyGrid('itemsgrid', '450px', '100%', true, true, true, 300);
                igrid.options.showHeaderRow = true;
                var hassize = false;
                var sizeids = null;
                var listoffields = [];
                $.each($scope.VendorAgreement.matrixlist, function (k, v) {
                    var datalist = [];
                    var field = '';
                    if (v.attributetype != 'Size')
                        listoffields.push('[' + v.attributetype + ']');

                    switch (v.attributetype) {
                        case 'Size':
                            hassize = true;
                            sizeids = v.items;
                            break;
                        case 'Colour':
                            field = 'colourid';
                            datalist = $scope.ColorList;
                            break;
                        case 'Style':
                            field = 'styleid';
                            datalist = $scope.StyleList;
                            break;
                        case 'Pattern':
                            field = 'patternid';
                            datalist = $scope.PatternList;
                            break;
                        case 'Sleeve':
                            field = 'sleeveid';
                            datalist = $scope.SleeveList;
                            break;
                        case 'Fit':
                            field = 'fitid';
                            datalist = $scope.FitList;
                            break;
                        case 'Material':
                            field = 'materialid';
                            datalist = $scope.MaterialList;
                            break;
                        case 'Type':
                            field = 'typeid';
                            datalist = $scope.TypeList;
                            break;
                    }
                    if (field != '')
                        igrid.AddReferenceColumn(field, v.attributetype, 120, datalist);
                });

                var rows = [];
                if (hassize && sizeids && sizeids != null && sizeids.length > 0) {
                    var uniqueid = listoffields.join('-');
                    $.each($scope.VendorAgreement.items, function (k, v) {
                        v.oqty = 0;
                        var newuniqueid = uniqueid.replace('[Colour]', v.colourid)
                            .replace('[Style]', v.styleid)
                            .replace('[Sleeve]', v.sleeveid)
                            .replace('[Fit]', v.fitid)
                            .replace('[Material]', v.materialid)
                            .replace('[Type]', v.typeid)
                            .replace('[Pattern]', v.patternid);
                        var exist = rows.find(function (item) {
                            return item.uniqueid == newuniqueid
                        });
                        if (exist == undefined || exist == null) {
                            exist = JSON.parse(JSON.stringify(v));
                            exist.uniqueid = newuniqueid;
                            exist.sizeitems = [];
                            rows.push(exist);
                        }
                        exist.sizeitems.push(v);
                    })

                    $.each(sizeids, function (k, v) {
                        var size = $scope.SizeList.find(function (item) {
                            return item.id == v.id;
                        });
                        var column = igrid.AddCustomColumn('size_' + size.id, size.name, 40, function (item, col) {
                            if (item.sizeitems && item.sizeitems.length > 0) {
                                var jqty = 0;
                                $.each(item.sizeitems, function (k, v) {
                                    if (v.sizeid == col.sizeid)
                                        jqty += v.oqty;
                                })
                                return jqty;
                            }
                            return '';
                        });
                        column.cssClass = 'rightalign';
                        column.sizeid = v.id;
                        column.sortable = true;
                        column.total = true;
                        column.filtertype = 'number';
                        column.getValue = function (item, col) {
                            if (item.sizeitems && item.sizeitems.length > 0) {
                                var jqty = 0;
                                $.each(item.sizeitems, function (k, v) {
                                    if (v.sizeid == col.sizeid)
                                        jqty += v.oqty;
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
                            if (lastValue != newvalue) {
                                $.each(item.sizeitems, function (k, v) {
                                    if (v.sizeid == col.sizeid) {
                                        v.oqty = newvalue;
                                    }
                                })
                                $scope.UpdateVenderqty();
                            }
                        }
                        column.editor = Slick.Editors.Text;
                    })
                }
                else {
                    var column = igrid.AddCustomColumn('toqty', "Qty", 40, function (item, col) {
                        return item.oqty;
                    });
                    column.cssClass = 'rightalign';
                    column.sortable = true;
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        return item.oqty;
                    };
                    column.setValue = function (item, col, newvalue, callback) {
                        var lastValue = col.getValue(item, col);
                        var newvalue = parseFloat(newvalue);
                        if (isNaN(newvalue))
                            newvalue = 0;
                        if (lastValue != newvalue) {
                            item.oqty = newvalue;
                            $scope.UpdateVenderdailyqty();
                        }
                    }
                    column.editor = Slick.Editors.Text;
                    $.each($scope.VendorAgreement.items, function (k, v) {
                        v.oqty = 0;
                        v.itbuscode = $scope.VendorAgreement.itemcode;
                        rows.push(v);
                    });
                }

                var column = igrid.AddSimpleColumn('printingname', "printingname", 100);
                igrid.options.autoEdit = true;
                igrid.Initialize();
                $scope.itemsgrid = igrid;
                $scope.itemsgrid.UpdateData(rows);
                $scope.VendorOrderItems = rows;
            }
        }


        $scope.ShowDailPriceItems = function () {

            var request = {
                "Item": {
                    'dailyproducts': true,
                    'supplierid': $scope.ProductRequest.supplierid,
                    'dailypricegroupid': $scope.ProductRequest.Details.dailypricegroupid
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SupplierproductgroupsService/Search",
                data: srequest
            }).success(function (result) {

                if ($scope.additionalgrid && $scope.additionalgrid != null)
                    $scope.additionalgrid.destroy();

                if ($scope.dailyitemsgrid && $scope.dailyitemsgrid != null)
                    $scope.dailyitemsgrid.destroy();
                var igrid = new InfyGrid('dailyitemsgrid', '450px', '100%', true, true, true, 300);
                igrid.options.showHeaderRow = true;

                var rows = [];
                igrid.AddSimpleColumn('printingname', 'Product Name', 120);
                igrid.AddSimpleColumn('itbuscode', 'Item Code', 120);
                igrid.AddColumn(
                    {
                        id: "cost", name: "Cost", field: "cost",
                        width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );

                var column = igrid.AddCustomColumn('toqty', "Qty", 60, function (item, col) {
                    return item.oqty;
                });
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    return item.oqty;
                };
                column.setValue = function (item, col, newvalue, callback) {
                    var lastValue = col.getValue(item, col);
                    var newvalue = parseFloat(newvalue);
                    if (isNaN(newvalue))
                        newvalue = 0;
                    if (lastValue != newvalue) {
                        item.oqty = newvalue;
                        $scope.UpdateVenderdailyqty();
                    }
                }
                column.editor = Slick.Editors.Text;

                $.each(result, function (k, v) {
                    var row = { 'oqty': 0, 'productid': v.productgroupid };
                    $.each(v.tradeagreements, function (tk, tv) {
                        row.tradeagreementid = tv.id;
                        row.cost = tv.cost;
                        row.sellingprice = tv.price;
                        row.itbuscode = tv.itemcode;
                        $.each(tv.items, function (ik, iv) {
                            var nrow = JSON.parse(JSON.stringify(row));
                            nrow.itemid = iv.itemid;
                            nrow.gln = iv.gln;
                            nrow.printingname = iv.printingname;
                            if (iv.price != 0) nrow.price = iv.price;
                            if (iv.mrp != 0) nrow.mrp = iv.mrp;
                            if (iv.cost != 0) nrow.cost = iv.cost;
                            rows.push(nrow);;
                        });
                    });
                });

                igrid.options.autoEdit = true;
                igrid.Initialize();
                $scope.dailyitemsgrid = igrid;
                $scope.dailyitemsgrid.UpdateData(rows);
                $scope.VendorOrderItems = rows;
            });
        }


        $scope.AddDailyOrder = function () {
            if ($scope.ProductRequest.companyid == null || $scope.ProductRequest.companyid == 0) {
                $scope.Toaster.Info("Please enter company");
                return;

            }
            if ($scope.ProductRequest.deliverylocationid == null || $scope.ProductRequest.deliverylocationid == 0) {
                $scope.Toaster.Info("Please enter delivery location");
                return;

            }
            if ($scope.ProductRequest.supplierid == null || $scope.ProductRequest.supplierid == 0) {
                $scope.Toaster.Info("Please enter order location");
                return;

            }

            if ($scope.ProductRequest.Purchaseorderitems == undefined ||
                $scope.ProductRequest.Purchaseorderitems == null)
                $scope.ProductRequest.Purchaseorderitems = [];

            $.each($scope.VendorOrderItems, function (k, v) {


                if (v.oqty == undefined || v.oqty == null || v.oqty <= 0) return;

                v.cost = parseFloat(v.cost);
                v.oqty = parseFloat(v.oqty);

                $scope.ProductRequest.Details.productid = v.productid;
                $scope.ProductRequest.Details.price = v.cost;
                $scope.ProductRequest.Details.mrp = v.mrp;
                $scope.ProductRequest.Details.sellingprice = v.price;

                var prditem = $scope.ProductList.find(function (item) {
                    return item.id == v.productid;
                });

                if (prditem && prditem != null) {
                    v.minmargin = prditem.minmargin;
                    v.maxmargin = prditem.maxmargin;
                    if (v.printingname == null || v.printingname == '')
                        v.printingname = prditem.name;
                }

                if (v.minmargin > 0)
                    $scope.ProductRequest.Details.sellingprice = (v.cost + (v.cost * v.minmargin / 100)).round(2);
                if (v.maxmargin > 0)
                    $scope.ProductRequest.Details.mrp = (v.cost + (v.cost * v.maxmargin / 100)).round(0);
                $scope.ProductRequest.Details.gln = v.gln;
                $scope.ProductRequest.Details.printingname = v.printingname;
                $scope.ProductRequest.Details.fitname = $scope.SearchRef($scope.ProductRequest.Details.fitid, $scope.FitList, '');

                var description = [];

                if ($scope.ProductRequest.Details.printingname != '')
                    description.push($scope.ProductRequest.Details.printingname);

                var item = {
                    'productid': $scope.ProductRequest.Details.productid,
                    'oqty': v.oqty,
                    'itbuscode': v.itbuscode,
                    'amount': $scope.ProductRequest.Details.price * v.oqty,
                    'deliverymode': $scope.ProductRequest.Details.deliverymode,
                    'tradeagreementid': v.tradeagreementid,
                    'cqty': 0,
                    'rqty': 0,
                    'isactive': true,
                    'version': 1,
                    'gln': $scope.ProductRequest.Details.gln,
                    'price': $scope.ProductRequest.Details.price,
                    'sellingprice': $scope.ProductRequest.Details.sellingprice,
                    'mrp': $scope.ProductRequest.Details.mrp,
                    'description': description.join('/'),
                    'printingname': $scope.ProductRequest.Details.printingname,
                    'itemid': v.itemid,
                    'id': 0
                };
                $scope.UpdateTax(item);
                $scope.ProductRequest.Purchaseorderitems.push(item);
            })
            $scope.ProductRequest.Details.oqty = 0;
            $scope.ProductRequest.Details.id = 0;
            $scope.ProductRequest.Details.imageid = '';
            $scope.ProductRequest.Details.designperqty = 0;
            $scope.ProductRequest.Details.designqty = 0;
            $scope.ProductRequest.Details.deliverymode = 0;
            $scope.ProductRequest.Details.price = '';
            $scope.ProductRequest.Details.mrp = '';
            $scope.ProductRequest.Details.sellingprice = '';
            $scope.ProductRequest.Details.gln = '';
            $scope.ProductRequest.Details.description = '';
            $scope.CalculateOrderQty();
            $scope.ConfigureVendorItemGrid();
            $scope.VenderOqty = '';
        }

        $scope.AddVenodrOrder = function () {

            if ($scope.ProductRequest.companyid == null || $scope.ProductRequest.companyid == 0) {
                $scope.Toaster.Info("Please enter company");
                return;

            }
            if ($scope.ProductRequest.deliverylocationid == null || $scope.ProductRequest.deliverylocationid == 0) {
                $scope.Toaster.Info("Please enter delivery location");
                return;

            }
            if ($scope.ProductRequest.supplierid == null || $scope.ProductRequest.supplierid == 0) {
                $scope.Toaster.Info("Please enter order location");
                return;

            }
            if ($scope.ProductRequest.Details.productid == null || $scope.ProductRequest.Details.productid == 0) {
                $scope.Toaster.Info("Please enter product");
                return;
            }

            if ($scope.ProductRequest.Purchaseorderitems == undefined ||
                $scope.ProductRequest.Purchaseorderitems == null)
                $scope.ProductRequest.Purchaseorderitems = [];

            $.each($scope.VendorAgreement.items, function (k, v) {

                if (v.oqty == undefined || v.oqty == null || v.oqty <= 0) return;

                $scope.ProductRequest.Details.brandid = $scope.VendorAgreement.brandid;
                $scope.ProductRequest.Details.price = v.cost;
                $scope.ProductRequest.Details.sellingprice = v.price;
                $scope.ProductRequest.Details.mrp = v.mrp;
                $scope.ProductRequest.Details.itemid = v.itemid;
                $scope.ProductRequest.Details.sizeid = v.sizeid;
                $scope.ProductRequest.Details.materialid = v.materialid;
                $scope.ProductRequest.Details.styleid = v.styleid;
                $scope.ProductRequest.Details.colourid = v.colourid;
                $scope.ProductRequest.Details.typeid = v.typeid;
                $scope.ProductRequest.Details.patternid = v.patternid;
                $scope.ProductRequest.Details.sleeveid = v.sleeveid;
                $scope.ProductRequest.Details.fitid = v.fitid;
                $scope.ProductRequest.Details.gln = v.gln;
                $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
                $scope.ProductRequest.Details.printingname = $scope.SearchRef($scope.ProductRequest.Details.productid, $scope.ProductList, '');
                $scope.ProductRequest.Details.sizename = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.SizeList, '');
                $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
                $scope.ProductRequest.Details.materialname = $scope.SearchRef($scope.ProductRequest.Details.materialid, $scope.MaterialList, '');
                $scope.ProductRequest.Details.stylename = $scope.SearchRef($scope.ProductRequest.Details.styleid, $scope.StyleList, '');
                $scope.ProductRequest.Details.colourname = $scope.SearchRef($scope.ProductRequest.Details.colourid, $scope.ColorList, '');
                $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');
                $scope.ProductRequest.Details.patternname = $scope.SearchRef($scope.ProductRequest.Details.patternid, $scope.PatternList, '');
                $scope.ProductRequest.Details.sleevename = $scope.SearchRef($scope.ProductRequest.Details.sleeveid, $scope.SleeveList, '');
                $scope.ProductRequest.Details.fitname = $scope.SearchRef($scope.ProductRequest.Details.fitid, $scope.FitList, '');

                var description = [];

                if ($scope.ProductRequest.Details.printingname != '')
                    description.push($scope.ProductRequest.Details.printingname);
                if ($scope.ProductRequest.Details.brandname != '')
                    description.push($scope.ProductRequest.Details.brandname);
                if ($scope.ProductRequest.Details.stylename != '')
                    description.push($scope.ProductRequest.Details.stylename);
                if ($scope.ProductRequest.Details.patternname != '')
                    description.push($scope.ProductRequest.Details.patternname);
                if ($scope.ProductRequest.Details.typename != '')
                    description.push($scope.ProductRequest.Details.typename);
                if ($scope.ProductRequest.Details.materialname != '')
                    description.push($scope.ProductRequest.Details.materialname);
                if ($scope.ProductRequest.Details.colourname != '')
                    description.push($scope.ProductRequest.Details.colourname);
                if ($scope.ProductRequest.Details.sleevename != '')
                    description.push($scope.ProductRequest.Details.sleevename);
                if ($scope.ProductRequest.Details.fitname != '')
                    description.push($scope.ProductRequest.Details.fitname);

                var item = {
                    'productid': $scope.ProductRequest.Details.productid,
                    'oqty': v.oqty,
                    'itbuscode': $scope.VendorAgreement.itemcode,
                    'tradeagreementid': $scope.VendorAgreement.id,
                    'amount': $scope.VendorAgreement.cost * v.oqty,
                    'deliverymode': $scope.ProductRequest.Details.deliverymode,
                    'cqty': 0,
                    'rqty': 0,
                    'isactive': true,
                    'version': 1,
                    'gln': $scope.ProductRequest.Details.gln,
                    'imageid': $scope.ProductRequest.Details.imageid,
                    'styleid': $scope.ProductRequest.Details.styleid,
                    'materialid': $scope.ProductRequest.Details.materialid,
                    'brandid': $scope.ProductRequest.Details.brandid,
                    'typeid': $scope.ProductRequest.Details.typeid,
                    'sleeveid': $scope.ProductRequest.Details.sleeveid,
                    'fitid': $scope.ProductRequest.Details.fitid,
                    'patternid': $scope.ProductRequest.Details.patternid,
                    'colourid': $scope.ProductRequest.Details.colourid,
                    'sizeid': $scope.ProductRequest.Details.sizeid,
                    'materialname': $scope.ProductRequest.Details.materialname,
                    'stylename': $scope.ProductRequest.Details.stylename,
                    'printingname': $scope.ProductRequest.Details.printingname,
                    'brandname': $scope.ProductRequest.Details.brandname,
                    'sizename': $scope.ProductRequest.Details.sizename,
                    'typename': $scope.ProductRequest.Details.typename,
                    'sleevename': $scope.ProductRequest.Details.sleevename,
                    'fitname': $scope.ProductRequest.Details.fitname,
                    'patternname': $scope.ProductRequest.Details.patternname,
                    'colourname': $scope.ProductRequest.Details.colourname,
                    'size': $scope.ProductRequest.Details.sizename,
                    'price': $scope.ProductRequest.Details.price,
                    'mrp': $scope.ProductRequest.Details.mrp,
                    'sellingprice': $scope.ProductRequest.Details.sellingprice,
                    'description': (v.printingname && v.printingname != null & v.printingname != '') ? v.printingname : description.join('/'),
                    'itemid': $scope.ProductRequest.Details.itemid,
                    'id': 0
                };
                item.amount = item.oqty * item.price;
                $scope.UpdateTax(item);
                $scope.ProductRequest.Purchaseorderitems.push(item);
            })
            $scope.ProductRequest.Details.oqty = 0;
            $scope.ProductRequest.Details.id = 0;
            $scope.ProductRequest.Details.imageid = '';
            $scope.ProductRequest.Details.designperqty = 0;
            $scope.ProductRequest.Details.designqty = 0;
            $scope.ProductRequest.Details.deliverymode = 0;
            $scope.ProductRequest.Details.price = '';
            $scope.ProductRequest.Details.mrp = '';
            $scope.ProductRequest.Details.sellingprice = '';
            $scope.ProductRequest.Details.gln = '';
            $scope.ProductRequest.Details.description = '';
            $scope.CalculateOrderQty();
            $scope.ConfigureVendorItemGrid();
            $scope.VenderOqty = '';
        }

        $scope.ConfigureVendorItemGrid = function () {

            $scope.ConfigAdditionalGrid();

            if ($scope.gridvendororderitem != null)
                $scope.gridvendororderitem.destroy();
            var igrid = new InfyGrid('gridvendororderitem', '450px', '100%', true, true, true, 140);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('description', 'Description', 250);

            if ($scope.ProductRequest.potype == -3)
                igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.SizeList);

            if ($scope.ProductRequest.potype == -3 || $scope.ProductRequest.potype == -4) {
                igrid.AddSimpleColumn('gln', 'UAN', 100);
                igrid.AddSimpleColumn('itbuscode', 'Item Code', 100);
                igrid.AddSimpleColumn('taxpercentage', 'Tax %', 60);
            }

            var editable = true;

            if (editable) {
                var column = igrid.AddColumn(
                    {
                        id: "price", name: "Cost", field: "price",
                        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );

                var column = igrid.AddColumn(
                    {
                        id: "discount", name: "Discount", field: "discount",
                        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.cssClass = 'rightalign';
                var column = igrid.AddColumn(
                    {
                        id: "discountpercentage", name: "Discount %", field: "discountpercentage",
                        width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('gross', 'Gross', 70, function (item, col) {
                    if (item.tax > 0)
                        return $filter('IndianNumber')(item.price + (item.tax / item.oqty), 2);
                    return $filter('IndianNumber')(item.price, 2);
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddColumn(
                    {
                        id: "sellingprice", name: "SRP", field: "sellingprice",
                        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.cssClass = 'rightalign';

                var column = igrid.AddColumn(
                    {
                        id: "mrp", name: "MRP", field: "mrp",
                        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.cssClass = 'rightalign';


                var column = igrid.AddColumn(
                    {
                        id: "oqty", name: "Qty", field: "oqty",
                        width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.total = true;
                column.cssClass = 'rightalign';
            }
            else {
                var column = igrid.AddCustomColumn('price', 'Cost', 70, function (item, col) {
                    return $filter('IndianNumber')(item.price, 2);
                });
                column.cssClass = 'rightalign';



                var column = igrid.AddCustomColumn('discount', 'Discount', 70, function (item, col) {
                    return $filter('IndianNumber')(item.discount, 2);
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('discountpercentage', 'Discount %', 80, function (item, col) {
                    return $filter('IndianNumber')(item.discountpercentage, 2);
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('gross', 'Gross', 70, function (item, col) {
                    if (item.tax > 0)
                        return $filter('IndianNumber')(item.price + (item.tax / item.oqty), 2);
                    return $filter('IndianNumber')(item.price, 2);
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('sellingprice', 'SRP', 70, function (item, col) {
                    return $filter('IndianNumber')(item.sellingprice, 2);
                });
                column.cssClass = 'rightalign';

                var column = igrid.AddCustomColumn('mrp', 'MRP', 70, function (item, col) {
                    return $filter('IndianNumber')(item.mrp, 2);
                });
                column.cssClass = 'rightalign';


                var column = igrid.AddCustomColumn('oqty', 'Qty', 70, function (item, col) {
                    return $filter('IndianNumber')(item.oqty, 2);
                });
                column.total = true;
                column.cssClass = 'rightalign';
            }

            var column = igrid.AddCustomColumn('amount', 'Amount', 70, function (item, col) {
                return $filter('IndianNumber')(item.amount, 2);
            });
            column.getValue = function (item) {
                return item.amount;
            }
            column.cssClass = 'rightalign';
            column.total = true;
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.gridvendororderitem = igrid;
            $scope.gridvendororderitem.UpdateData($scope.ProductRequest.Purchaseorderitems, '_id');

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $timeout(function () {
                    if (command == 'Delete') {
                        $scope.ProductRequest.Purchaseorderitems.splice($scope.ProductRequest.Purchaseorderitems.indexOf(item), 1);
                        $scope.gridvendororderitem.UpdateData($scope.ProductRequest.Purchaseorderitems, '_id');

                        $scope.UpdateTax(item);
                        $scope.CalculateOrderQty()
                        igrid.UpdateAllTotals(igrid.grid);
                        igrid.grid.invalidateRow(cell.row);
                        igrid.grid.render();

                    }
                }, 10);
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var command = args.grid.getColumns()[args.cell].id;
                var item = args.grid.getDataItem(args.row);

                if (command == 'price') {
                    var discount = $rootScope.GetNumber(item.discount);
                    if (isNaN(discount))
                        discount = 0;
                    var amount = ($rootScope.GetNumber(item.price) - discount) * $rootScope.GetNumber(item.oqty);
                    if (isNaN(amount))
                        amount = 0;
                    item.amount = amount;

                    $timeout(function () {
                        $scope.UpdateTax(item);
                        $scope.CalculateOrderQty()
                        igrid.UpdateAllTotals(args.grid);
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                    return;
                } else if (command == 'oqty') {
                    var discount = $rootScope.GetNumber(item.discount);
                    if (isNaN(discount))
                        discount = 0;
                    var amount = ($rootScope.GetNumber(item.price) - discount) * $rootScope.GetNumber(item.oqty);
                    if (isNaN(amount))
                        amount = 0;
                    item.amount = amount;

                    $timeout(function () {
                        $scope.UpdateTax(item);
                        $scope.CalculateOrderQty()
                        igrid.UpdateAllTotals(args.grid);
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                    return;
                } else if (command == 'discount') {
                    var discount = $rootScope.GetNumber(item.discount);
                    var price = $rootScope.GetNumber(item.price);
                    if (isNaN(discount))
                        discount = 0;
                    if (discount == 0)
                        item.discountpercentage = 0;
                    else
                        item.discountpercentage = (discount / price * 100).round(2);

                    var amount = ($rootScope.GetNumber(item.price) - discount) * $rootScope.GetNumber(item.oqty);
                    item.amount = amount;
                    $timeout(function () {
                        $scope.UpdateTax(item);
                        $scope.CalculateOrderQty()
                        igrid.UpdateAllTotals(args.grid);
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (command == 'discountpercentage') {
                    var discountpercentage = $rootScope.GetNumber(item.discountpercentage);
                    var price = $rootScope.GetNumber(item.price);
                    if (isNaN(discountpercentage))
                        discountpercentage = 0;
                    if (discountpercentage == 0)
                        item.discount = 0;
                    else
                        item.discount = (price * discountpercentage / 100).round(2);

                    var amount = ($rootScope.GetNumber(item.price) - item.discount) * $rootScope.GetNumber(item.oqty);
                    item.amount = amount;
                    $timeout(function () {
                        $scope.UpdateTax(item);
                        $scope.CalculateOrderQty()
                        igrid.UpdateAllTotals(args.grid);
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });
        }

        $scope.VenderOqty = 0;
        $scope.UpdateVenderqty = function () {
            var oqty = 0;
            $.each($scope.VendorAgreement.items, function (k, v) {
                if (v.oqty && v.oqty != null) {
                    var nqty = parseFloat(v.oqty);
                    if (!isNaN(nqty) && nqty > 0)
                        oqty += nqty;
                }
            })
            $timeout(function () {
                $scope.VenderOqty = oqty;
            }, 10);
        }


        $scope.UpdateVenderdailyqty = function () {
            var oqty = 0;
            $.each($scope.VendorOrderItems, function (k, v) {
                if (v.oqty && v.oqty != null) {
                    var nqty = parseFloat(v.oqty);
                    if (!isNaN(nqty) && nqty > 0)
                        oqty += nqty;
                }
            })
            $timeout(function () {
                $scope.VenderOqty = oqty;
            }, 10);
        }

        $scope.UpdateAgreementList = function () {

            var vendorproduct = $scope.VendorProductList.find(function (item) {
                return item.id == $scope.ProductRequest.Details.SupplierProductID;
            });

            if (vendorproduct && vendorproduct != null) {
                $scope.ProductRequest.Details.productid = vendorproduct.productgroupid;
                if (vendorproduct && vendorproduct != null) {
                    restService.Get('SuppliertradeagreementService/ActiveAgreement/' + vendorproduct.id, true).then(function (results) {
                        $scope.VendorAgreementList = results;
                    });
                }
            }
        }

        $scope.POTypeChanged = function () {
            if ($scope.ProductRequest.potype == -3 || $scope.ProductRequest.potype == -4) {
                $scope.SupplierChanged();
                $timeout(function () {
                    $scope.ConfigureVendorItemGrid();
                }, 20);
            }
            else if ($scope.ProductRequest.potype == -5) {
                $scope.SupplierChanged();
                $timeout(function () {
                    $scope.ConfigureUANItemGrid();
                }, 20);
            }
        }

        $scope.uanpofileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            $scope.UANPoList = value;
            $scope.additionalgrid.UpdateData(value.postatus.orders);
            $scope.NetGross = 0;
            $scope.NetQty = 0;
            $.each(value.postatus.orders, function (k, v) {
                $scope.NetGross += v.Amount;
                $scope.NetQty += v.Qty;
            })
        }

        $scope.UANPoList = null;
        $scope.ConfigureUANItemGrid = function () {
            if ($scope.dailyitemsgrid != null)
                $scope.dailyitemsgrid.destroy();

            if ($scope.itemsgrid != null)
                $scope.itemsgrid.destroy();

            if ($scope.additionalgrid != null)
                $scope.additionalgrid.destroy();

            var iagrid = new InfyGrid('additionaluangrid', '450px', '100%', true, true, true,
                ($scope.ProductRequest.id && $scope.ProductRequest.id > 0) ? 300 : 140);

            iagrid.options.showHeaderRow = true;
            var column = iagrid.AddCustomColumn('Error', '', 35, function (item, col) {
                if (item.Error)
                    return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-xs btn-danger'><i class='fa fa-info-circle' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
                return '';
            });
            iagrid.AddSimpleColumn('Supplier', 'Description', 100);
            iagrid.AddFormatedColumn('PODate', 'PO Date', 80, 'dd-MM-yyyy');
            iagrid.AddFormatedColumn('DueDate', 'Due Date', 80, 'dd-MM-yyyy');
            var column = iagrid.AddCustomColumn('Qty', 'Qty', 50, function (item, col) {
                return $filter('IndianNumber')(item.Qty, 2);
            });
            column.cssClass = 'rightalign';
            var column = iagrid.AddCustomColumn('Amount', 'Amount', 70, function (item, col) {
                return $filter('IndianNumber')(item.Amount, 2);
            });
            column.cssClass = 'rightalign';
            iagrid.AddButtonColumn('Show', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            //iagrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            iagrid.Initialize();
            $scope.additionalgrid = iagrid;
            if ($scope.ProductRequest.additionalcharges && $scope.ProductRequest.additionalcharges != null)
                $scope.additionalgrid.UpdateData($scope.ProductRequest.additionalcharges);
            iagrid.grid.onClick.subscribe(function (e) {
                var cell = iagrid.grid.getCellFromEvent(e);
                var item = iagrid.grid.getDataItem(cell.row);
                var command = iagrid.grid.getColumns()[cell.cell].id;
                if (command == 'Show') {
                    $timeout(function () {
                        $scope.orderqty = item.Qty;
                        $scope.ordervalue = item.Amount;
                        $scope.ordertax = item.Tax;
                        $scope.gridvendororderitem.UpdateData(item.items);
                    }, 10);
                }
                else if (command == "Error") {
                    var errors = [];
                    if (item.supplierid == 0) {
                        errors.push('Missing Supplier');
                        errors.push(item.Supplier);
                    }
                    if (item.MissingUAN && item.MissingUAN != null && item.MissingUAN.length > 0) {
                        errors.push('Missing UAN');
                        $.each(item.MissingUAN, function (k, v) {
                            errors.push(v);
                        })
                    }
                    $rootScope.ShowAlert('Error', errors.join('<br>'));
                }
            });

            if ($scope.gridvendororderitem != null)
                $scope.gridvendororderitem.destroy();
            var igrid = new InfyGrid('griduanorderitems', '450px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('Design', 'Design', 100);
            igrid.AddSimpleColumn('Product', 'Product', 250);
            igrid.AddSimpleColumn('Size', 'Size', 50);
            igrid.AddSimpleColumn('UAN', 'UAN', 100);

            var column = igrid.AddSimpleColumn('Percentage', 'Tax %', 60);
            column.cssClass = 'rightalign';
            var column = igrid.AddCustomColumn('Qty', 'Qty', 70, function (item, col) {
                return $filter('IndianNumber')(item.Qty, 2);
            });
            column.getValue = function (item) {
                return item.Qty;
            }
            column.cssClass = 'rightalign';
            column.total = true;
            var column = igrid.AddCustomColumn('Cost', 'Rate', 70, function (item, col) {
                return $filter('IndianNumber')(item.Cost, 2);
            });
            column.getValue = function (item) {
                return item.Cost;
            }
            column.cssClass = 'rightalign';

            var column = igrid.AddCustomColumn('Amount', 'Gross', 70, function (item, col) {
                return $filter('IndianNumber')(item.Cost * item.Qty, 2);
            });
            column.getValue = function (item) {
                return item.Cost * item.Qty;
            }
            column.cssClass = 'rightalign';
            column.total = true;
            var column = igrid.AddCustomColumn('Tax', 'Tax', 70, function (item, col) {
                return $filter('IndianNumber')(item.Tax, 2);
            });
            column.getValue = function (item) {
                return item.Tax;
            }
            column.cssClass = 'rightalign';
            column.total = true;
            var column = igrid.AddCustomColumn('Net', 'Net', 70, function (item, col) {
                return $filter('IndianNumber')(item.Tax + (item.Cost * item.Qty), 2);
            });
            column.getValue = function (item) {
                return item.Tax + (item.Cost * item.Qty);
            }
            column.cssClass = 'rightalign';
            column.total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.gridvendororderitem = igrid;
        }


        $scope.VendorUANChanged = function () {
            var venderitem = $scope.TradeAgreementItems.find(function (item) {
                return item.gln == $scope.vendorgln;
            });
            if (venderitem && venderitem != null) {
                $scope.venderitemuniqueid = venderitem.uniqueid;
                $scope.venderitemprice = venderitem.cost > 0 ? venderitem.cost : '';
                $scope.venderitemdiscount = '';
                $scope.vendoritbuscode = venderitem.itbuscode;
                $scope.venderitemdiscountpercentage = '';
                $scope.venderitemsrp = venderitem.price > 0 ? venderitem.price : '';
                $scope.venderitemmrp = venderitem.mrp > 0 ? venderitem.mrp : '';
                $timeout(function () {
                    $scope.setFocusvenderitemprice();
                }, 20);
            }
            else {
                $rootScope.Toaster.Error("UAN missing");
                $scope.venderitemprice = '';
                $scope.venderitemdiscount = '';
                $scope.vendoritbuscode = '';
                $scope.venderitemdiscountpercentage = '';
                $scope.venderitemsrp = '';
                $scope.venderitemmrp = '';
            }
        }


        $scope.VendorItemChanged = function () {
            var venderitem = $scope.TradeAgreementItems.find(function (item) {
                return item.uniqueid == $scope.venderitemuniqueid;
            });
            if (venderitem && venderitem != null) {
                $scope.venderitemprice = venderitem.cost > 0 ? venderitem.cost : '';
                $scope.venderitemdiscount = '';
                $scope.vendoritbuscode = venderitem.itbuscode;
                $scope.venderitemdiscountpercentage = '';
                $scope.venderitemsrp = venderitem.price > 0 ? venderitem.price : '';
                $scope.venderitemmrp = venderitem.mrp > 0 ? venderitem.mrp : '';
                $timeout(function () {
                    $scope.setFocusvenderitemprice();
                }, 20);
            }
        }

        $scope.VendorDiscountChanged = function (source) {
            if (source == 'discount') {
                $scope.venderitemdiscountpercentage = ($scope.venderitemdiscount / $scope.venderitemprice * 100).round(2);
            }
            else {
                $scope.venderitemdiscount = ($scope.venderitemdiscountpercentage * $scope.venderitemprice / 100).round(2);
            }
        }



        $scope.UpdateTax = function (item) {
            item.taxpercentage = 0;
            var prdInfo = $scope.ProductList.find(function (arritem) {
                return arritem.id == item.productid;
            });

            if (prdInfo && prdInfo != null) {
                item.cessvalue = 0;
                item.cesspercentage = 0;
                item.tax = 0;
                item.taxid = prdInfo.purchasetaxid;
                var taxInfo = $scope.TaxList.find(function (arritem) {
                    return arritem.id == prdInfo.purchasetaxid;
                });
                if (taxInfo != null) {
                    if (taxInfo.taxsplit == null) {
                        taxInfo.taxsplit = { 'taxpercentage': taxInfo.taxpercentage };
                    }
                    taxInfo.taxsplit.findTax = $scope.findTax;
                    var taxPercentage = taxInfo.taxsplit.findTax($scope.GetNumber(item.price) - $scope.GetNumber(item.discount), true);
                    item.taxpercentage = taxPercentage.cgst + taxPercentage.sgst;

                    var amount = ($rootScope.GetNumber(item.price) - $rootScope.GetNumber(item.discount)) * $rootScope.GetNumber(item.oqty);
                    item.tax = (amount * item.taxpercentage / 100).round(2);
                }

                if (prdInfo.cesstaxid && prdInfo.cesstaxid > 0) {
                    var taxInfo = $scope.TaxList.find(function (arritem) {
                        return arritem.id == prdInfo.cesstaxid;
                    });
                    if (taxInfo != null) {
                        item.cesspercentage = taxInfo.taxpercentage;
                        var amount = ($rootScope.GetNumber(item.price) - $rootScope.GetNumber(item.discount)) * $rootScope.GetNumber(item.oqty);
                        item.cessvalue = (amount * item.cesspercentage / 100).round(2);
                    }
                }
            }


        }

        $scope.findTax = function (amount, incost) {

            if (typeof (incost) == 'undefined') incost = false;
            if (typeof (items) == 'undefined' && typeof (this.taxpercentage) != 'undefined')
                return { 'cgst': this.taxpercentage / 2, 'sgst': this.taxpercentage / 2 };

            if (this.items == null || this.items.length == 0)
                return null;

            if (this.items.length == 1)
                return this.items[0];

            if (isNaN(amount)) amount = 0;
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (incost == item.incost && item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            return { 'cgst': 0, 'sgst': 0 };
        }


        $scope.AddAdditionalCharge = function () {
            if ($scope.ProductRequest.additionalcharges == undefined ||
                $scope.ProductRequest.additionalcharges == null)
                $scope.ProductRequest.additionalcharges = [];

            var additionalcharge = $scope.AdditionalChargeList.find(function (item) {
                return item.id == $scope.ProductRequest.AdditionalChargeID;
            });

            if (additionalcharge == undefined || additionalcharge == null) return;

            $scope.ProductRequest.additionalcharges.push({
                'additionalchargeid': additionalcharge.id,
                'name': additionalcharge.name,
                'taxcomponent': additionalcharge.additionalinfo.disabled,
                'hsn': additionalcharge.additionalinfo.formula,
                'taxid': additionalcharge.additionalinfo.referenceid,
                'value': $scope.ProductRequest.AdditionalChargeValue
            });
            $scope.ConfigAdditionalGrid();
        }

        $scope.additionalgrid = null;
        $scope.ConfigAdditionalGrid = function () {

            if ($scope.dailyitemsgrid != null)
                $scope.dailyitemsgrid.destroy();

            if ($scope.itemsgrid != null)
                $scope.itemsgrid.destroy();

            if ($scope.additionalgrid != null)
                $scope.additionalgrid.destroy();
            var igrid = new InfyGrid('additionalgrid', '450px', '100%', true, true, true, 400);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', 'Description', 250);
            var column = igrid.AddCustomColumn('value', 'Amount', 70, function (item, col) {
                return $filter('IndianNumber')(item.value, 2);
            });
            column.cssClass = 'rightalign';
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Show' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.Initialize();
            $scope.additionalgrid = igrid;
            if ($scope.ProductRequest.additionalcharges && $scope.ProductRequest.additionalcharges != null)
                $scope.additionalgrid.UpdateData($scope.ProductRequest.additionalcharges);
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete') {
                    $timeout(function () {
                        $scope.ProductRequest.additionalcharges.splice($scope.ProductRequest.additionalcharges.indexOf(item), 1);
                        $scope.additionalgrid.UpdateData($scope.ProductRequest.additionalcharges);
                    }, 10);
                }
            });

        }

        $scope.AddVendorPOItemInternal = function () {
            if ($scope.ProductRequest.Purchaseorderitems == undefined ||
                $scope.ProductRequest.Purchaseorderitems == null)
                $scope.ProductRequest.Purchaseorderitems = [];

            if ($scope.venderitemqty == undefined || $scope.venderitemqty == null || $scope.venderitemqty <= 0) return;
            var venderitem = $scope.TradeAgreementItems.find(function (item) {
                return item.uniqueid == $scope.venderitemuniqueid;
            });
            if (venderitem == undefined || venderitem == null)
                return;

            if ($scope.ProductRequest.Details == undefined ||
                $scope.ProductRequest.Details == null)
                $scope.ProductRequest.Details = {};

            if ($scope.venderitemdiscount == undefined || $scope.venderitemdiscount == null)
                $scope.venderitemdiscount = 0;

            if ($scope.venderitemdiscountpercentage == undefined || $scope.venderitemdiscountpercentage == null)
                $scope.venderitemdiscountpercentage = 0;

            $scope.ProductRequest.Details.oqty = parseFloat($scope.venderitemqty);
            $scope.ProductRequest.Details.brandid = venderitem.brandid;
            $scope.ProductRequest.Details.productid = venderitem.productid;
            $scope.ProductRequest.Details.price = $scope.venderitemprice;
            $scope.ProductRequest.Details.sellingprice = $scope.venderitemsrp;
            $scope.ProductRequest.Details.mrp = $scope.venderitemmrp;
            $scope.ProductRequest.Details.itemid = venderitem.itemid;
            $scope.ProductRequest.Details.sizeid = venderitem.sizeid;
            $scope.ProductRequest.Details.materialid = venderitem.materialid;
            $scope.ProductRequest.Details.styleid = venderitem.styleid;
            $scope.ProductRequest.Details.colourid = venderitem.colourid;
            $scope.ProductRequest.Details.fitid = venderitem.fitid;
            $scope.ProductRequest.Details.sleeveid = venderitem.sleeveid;
            $scope.ProductRequest.Details.typeid = venderitem.typeid;
            $scope.ProductRequest.Details.patternid = venderitem.patternid;
            $scope.ProductRequest.Details.gln = venderitem.gln;
            $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
            $scope.ProductRequest.Details.printingname = $scope.SearchRef($scope.ProductRequest.Details.productid, $scope.ProductList, '');
            $scope.ProductRequest.Details.sizename = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.SizeList, '');
            $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
            $scope.ProductRequest.Details.materialname = $scope.SearchRef($scope.ProductRequest.Details.materialid, $scope.MaterialList, '');
            $scope.ProductRequest.Details.stylename = $scope.SearchRef($scope.ProductRequest.Details.styleid, $scope.StyleList, '');
            $scope.ProductRequest.Details.colourname = $scope.SearchRef($scope.ProductRequest.Details.colourid, $scope.ColorList, '');
            $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');
            $scope.ProductRequest.Details.patternname = $scope.SearchRef($scope.ProductRequest.Details.patternid, $scope.PatternList, '');
            $scope.ProductRequest.Details.sleevename = $scope.SearchRef($scope.ProductRequest.Details.sleeveid, $scope.SleeveList, '');
            $scope.ProductRequest.Details.fitname = $scope.SearchRef($scope.ProductRequest.Details.fitid, $scope.FitList, '');

            var productdescription = '';
            if (venderitem.printingname == undefined || venderitem.printingname == null || venderitem.printingname == '') {
                var description = [];
                if ($scope.ProductRequest.Details.printingname != '')
                    description.push($scope.ProductRequest.Details.printingname);
                if ($scope.ProductRequest.Details.brandname != '')
                    description.push($scope.ProductRequest.Details.brandname);
                if ($scope.ProductRequest.Details.stylename != '')
                    description.push($scope.ProductRequest.Details.stylename);
                if ($scope.ProductRequest.Details.patternname != '')
                    description.push($scope.ProductRequest.Details.patternname);
                if ($scope.ProductRequest.Details.typename != '')
                    description.push($scope.ProductRequest.Details.typename);
                if ($scope.ProductRequest.Details.materialname != '')
                    description.push($scope.ProductRequest.Details.materialname);
                if ($scope.ProductRequest.Details.colourname != '')
                    description.push($scope.ProductRequest.Details.colourname);
                if ($scope.ProductRequest.Details.sleevename != '')
                    description.push($scope.ProductRequest.Details.sleevename);
                if ($scope.ProductRequest.Details.fitname != '')
                    description.push($scope.ProductRequest.Details.fitname);
                productdescription = description.join('/');
            }
            else {
                productdescription = venderitem.printingname;
            }

            var item = {
                'productid': $scope.ProductRequest.Details.productid,
                'oqty': $scope.ProductRequest.Details.oqty,
                'discount': $scope.venderitemdiscount,
                'itbuscode': $scope.vendoritbuscode,
                'discountpercentage': $scope.venderitemdiscountpercentage,
                'oqty': $scope.ProductRequest.Details.oqty,
                'tradeagreementid': venderitem.tradeagreementid,
                'amount': $scope.ProductRequest.Details.price * $scope.ProductRequest.Details.oqty,
                'deliverymode': $scope.ProductRequest.Details.deliverymode,
                'cqty': 0,
                'rqty': 0,
                'isactive': true,
                'version': 1,
                'gln': $scope.ProductRequest.Details.gln,
                'imageid': $scope.ProductRequest.Details.imageid,
                'styleid': $scope.ProductRequest.Details.styleid,
                'materialid': $scope.ProductRequest.Details.materialid,
                'brandid': $scope.ProductRequest.Details.brandid,
                'typeid': $scope.ProductRequest.Details.typeid,
                'sleeveid': $scope.ProductRequest.Details.sleeveid,
                'fitid': $scope.ProductRequest.Details.fitid,
                'patternid': $scope.ProductRequest.Details.patternid,
                'colourid': $scope.ProductRequest.Details.colourid,
                'sizeid': $scope.ProductRequest.Details.sizeid,
                'materialname': $scope.ProductRequest.Details.materialname,
                'stylename': $scope.ProductRequest.Details.stylename,
                'printingname': $scope.ProductRequest.Details.printingname,
                'brandname': $scope.ProductRequest.Details.brandname,
                'sizename': $scope.ProductRequest.Details.sizename,
                'typename': $scope.ProductRequest.Details.typename,
                'sleevename': $scope.ProductRequest.Details.sleevename,
                'fitname': $scope.ProductRequest.Details.fitname,
                'patternname': $scope.ProductRequest.Details.patternname,
                'colourname': $scope.ProductRequest.Details.colourname,
                'size': $scope.ProductRequest.Details.sizename,
                'price': $scope.ProductRequest.Details.price,
                'mrp': $scope.ProductRequest.Details.mrp,
                'sellingprice': $scope.ProductRequest.Details.sellingprice,
                'description': productdescription,
                'itemid': $scope.ProductRequest.Details.itemid,
                'id': 0
            };
            $scope.UpdateTax(item);
            if ($scope.ProductRequest.inclusivetax &&
                item.taxpercentage && item.taxpercentage > 0) {
                item.price = ((item.price / (100 + item.taxpercentage)) * 100).round(2);
                if (item.discountpercentage &&
                    discountpercentage > 0) {
                    item.discount = (item.discountpercentage * item.price / 100).round(2);
                }
                $scope.UpdateTax(item);
                item.amount = item.price * item.oqty;
            }
            $scope.ProductRequest.Purchaseorderitems.push(item);
        }

        $scope.AddVendorPOItem = function () {
            $scope.AddVendorPOItemInternal();
            $scope.ProductRequest.Details.oqty = 0;
            $scope.ProductRequest.Details.id = 0;
            $scope.ProductRequest.Details.imageid = '';
            $scope.ProductRequest.Details.designperqty = 0;
            $scope.ProductRequest.Details.designqty = 0;
            $scope.ProductRequest.Details.deliverymode = 0;
            $scope.ProductRequest.Details.price = '';
            $scope.ProductRequest.Details.mrp = '';
            $scope.ProductRequest.Details.sellingprice = '';
            $scope.ProductRequest.Details.gln = '';
            $scope.ProductRequest.Details.description = '';
            $scope.CalculateOrderQty();
            $scope.ConfigureVendorItemGrid();
            if ($scope.ShowVendorGLN) {
                $scope.vendorgln = '';
                $scope.setFocusvendorgln();
            }
            else {
                $scope.setFocusvenderitemitemid();
            }
        }

        $scope.SelectItem = function (item) {
            $scope.ProductRequest.Details.itemid = item.id;
            $scope.ProductRequest.Details.brandid = item.brandid;
            $scope.ProductRequest.Details.sizeid = item.sizeid;
            $scope.ProductRequest.Details.styleid = item.styleid;
            $scope.ProductRequest.Details.fitid = item.fitid;
            $scope.ProductRequest.Details.typeid = item.typeid;
            $scope.ProductRequest.Details.sleeveid = item.sleeveid;
            $scope.ProductRequest.Details.materialid = item.materialid;
            $scope.ProductRequest.Details.patternid = item.patternid;
            $scope.ProductRequest.Details.colourid = item.colourid;
            $scope.ProductRequest.Details.dealerprice = item.dealerprice;
            $scope.ProductRequest.Details.description = item.printingname;
            $scope.ProductRequest.Details.mrp = item.mrp;
            $scope.ProductRequest.Details.sellingprice = item.salerate;
            $scope.ProductRequest.Details.price = item.purchaserate;
        }

        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            var barcodeindex = -1;
            var priceindex = -1;
            var qtyindex = -1;
            $.each(value.columns, function (k, v) {
                var colunmname = v.toString().trim().toLowerCase();
                if (colunmname == 'barcode' || colunmname == 'uan' || colunmname == 'gln')
                    barcodeindex = k;
                if (colunmname == 'qty' || colunmname == 'pcs' || colunmname == 'mtr')
                    qtyindex = k;
                if (colunmname == 'price' || colunmname == 'rate' || colunmname == 'cost')
                    priceindex = k;
            });
            if (barcodeindex == -1) {
                $rootScope.Toaster.Error("Barcode column is missing");
                return;
            }
            if (qtyindex == -1) {
                $rootScope.Toaster.Error("Qty column is missing");
                return;
            }
            if (priceindex == -1) {
                $rootScope.Toaster.Error("Price column is missing");
                return;
            }

            var missingln = [];
            $.each(value.rows, function (k, v) {
                if (k == 0) return;
                var glnexist = $scope.TradeAgreementItems.find(function (item) {
                    return item.gln == v.values[barcodeindex];
                })
                if (glnexist == undefined || glnexist == null) {
                    missingln.push(v.values[barcodeindex]);
                }
            });
            if (missingln.length > 0) {
                $rootScope.Toaster.Error("Barcode Missing\r\n" + missingln.join(','));
            }
            else {

                $.each(value.rows, function (k, v) {
                    if (k == 0) return;
                    var glnexist = $scope.TradeAgreementItems.find(function (item) {
                        return item.gln == v.values[barcodeindex];
                    })
                    if (glnexist && glnexist != null) {
                        $scope.venderitemuniqueid = glnexist.uniqueid;
                        $scope.vendoritbuscode = glnexist.itbuscode;
                        $scope.venderitemitemid = glnexist.itemid;
                        $scope.venderitemprice = parseFloat(v.values[priceindex]);
                        $scope.venderitemdiscount = 0;
                        $scope.venderitemdiscountpercentage = 0;
                        $scope.venderitemsrp = glnexist.price;
                        $scope.venderitemmrp = glnexist.mrp;
                        $scope.venderitemqty = parseFloat(v.values[qtyindex]);
                        $scope.AddVendorPOItem();
                    }
                });

                $scope.ProductRequest.Details.oqty = 0;
                $scope.ProductRequest.Details.id = 0;
                $scope.ProductRequest.Details.imageid = '';
                $scope.ProductRequest.Details.designperqty = 0;
                $scope.ProductRequest.Details.designqty = 0;
                $scope.ProductRequest.Details.deliverymode = 0;
                $scope.ProductRequest.Details.price = '';
                $scope.ProductRequest.Details.mrp = '';
                $scope.ProductRequest.Details.sellingprice = '';
                $scope.ProductRequest.Details.gln = '';
                $scope.ProductRequest.Details.description = '';
                $scope.CalculateOrderQty();
                $scope.ConfigureVendorItemGrid();
                if ($scope.ShowVendorGLN) {
                    $scope.vendorgln = '';
                    $scope.setFocusvendorgln();
                }
                else {
                    $scope.setFocusvenderitemitemid();
                }
            }
        }

        $scope.SearchPO = function () {
            if ($scope.SearchCondition.searchmode == undefined || $scope.SearchCondition.searchmode == null)
                $scope.SearchCondition.searchmode = 0;
            else
                $scope.SearchCondition.searchmode = parseInt($scope.SearchCondition.searchmode);

            switch ($scope.SearchCondition.searchmode) {
                case 0:
                    $scope.Search();
                    break;
                case 1:
                    $scope.SearchPOSummary();
                    break;
                case 2:
                    $scope.SearchPendingPO();
                    break;
                case 3:
                    $scope.SearchAgreementPOSummary();
                    break;
            }
        }

        $scope.SearchDesignList = function (selecteddesignid) {
            if ($scope.ProductRequest == undefined || $scope.ProductRequest == null ||
                $scope.ProductRequest.Details == undefined || $scope.ProductRequest.Details == null ||
                $scope.ProductRequest.Details.productid == undefined || $scope.ProductRequest.Details.productid == null)
                return;
            var request = {
                "Item": {
                    'supplierid': $scope.ProductRequest.supplierid,
                    'productid': $scope.ProductRequest.Details.productid
                },
                "securitytoken": "String content"
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ProductdesignService/Search",
                data: srequest
            }).success(function (result) {
                var designlist = result;
                designlist = result.map(function (item) {
                    return { 'id': item.designid, 'name': item.designid };
                });

                if (selecteddesignid && selecteddesignid != null) {
                    var existdesign = designlist.find(function (item) {
                        return item.id == selecteddesignid;
                    });
                    if (existdesign == undefined || existdesign == null) {
                        designlist.push({ 'id': selecteddesignid, 'name': selecteddesignid });
                    }
                }

                designlist.push({ 'id': 'ADD DESIGN', 'name': 'ADD DESIGN' });
                $scope.DesignList = designlist;
                if (selecteddesignid && selecteddesignid != null) {
                    $scope.ProductRequest.Details.designid = null;
                    $timeout(function () {
                        $scope.ProductRequest.Details.designid = selecteddesignid;
                    }, 20)
                }
            })
        }


        $scope.DesignList = [{ 'id': 'ADD DESIGN', 'name': 'ADD DESIGN' }];
        $scope.ShowingDesign = false;
        $scope.AddNewDesign = function () {
            if ($scope.ShowingDesign) return;
            $scope.ProductRequest.Details.designid = '';
            $scope.ShowingDesign = true;
            $rootScope.Prompt('Design', 'Design Name', function (design) {
                $scope.ShowingDesign = false;
                if (design && design != null && design != '') {
                    var srequest = JSON.stringify({
                        'Item': {
                            'supplierid': $scope.Invoice.supplierid,
                            'productid': $scope.ProductSearch.ProductID,
                            'designid': design
                        }
                    });
                    $http({
                        method: 'POST',
                        url: "ProductdesignService/Save",
                        data: srequest
                    }).success(function (result) {
                        if (result && result.id > 0) {
                            var dlist = $scope.DesignList;
                            dlist.splice(dlist.length - 1, 1);
                            $scope.DesignList = null;
                            dlist.push({ 'name': result.designid, 'id': result.designid });
                            dlist.push({ 'id': 'ADD DESIGN', 'name': 'ADD DESIGN' });
                            $timeout(function () {
                                $scope.DesignList = dlist;
                                $scope.ProductRequest.Details.designid = result.designid;

                                if ($scope.ProductRequest.mode == 2) {
                                    $scope.setFocusdsn_size();
                                }
                                else if ($scope.ProductRequest.mode == 1) {
                                    $scope.setFocusdsn_sizegroup();
                                }
                                else {
                                    $scope.setFocusdsn_sizerange();
                                }

                            }, 20);
                            $scope.SearchSKUDesign(true);
                        }
                    });
                }
            }, '', undefined, function () {
                $scope.ShowingDesign = false
            });
        }

        $scope.posizemodechanged = function () {
            if ($scope.ProductRequest.potype == -5) {
                $scope.setFocusshopcontactemployeeid();
            }
            else {
                $scope.setFocusagent();
            }
        }

        $scope.CurrentProductDesign = null;
        $scope.ShowProductDesignImage = function () {
            if ($scope.CurrentProductDesign == null ||
                $scope.CurrentProductDesign.colours == undefined ||
                $scope.CurrentProductDesign.colours == null ||
                $scope.CurrentProductDesign.colours.length == 0) return;

            var img = $scope.CurrentProductDesign.colours.find(function (item) {
                return item.id == $scope.ProductRequest.Details.colourid;
            });
            if (img == undefined || img == null)
                img = $scope.CurrentProductDesign.colours[0];

            $scope.ProductRequest.Details.imagechanged = false;
            $scope.ProductRequest.Details.imageid = img.image;
        }

        $scope.SaveProductDesignImage = function () {
            if ($scope.CurrentProductDesign == undefined ||
                $scope.CurrentProductDesign == null) {
                $scope.CurrentProductDesign = {
                    'supplierid': $scope.ProductRequest.supplierid,
                    'productid': $scope.ProductRequest.Details.productid,
                    'designid': $scope.ProductRequest.Details.designid,
                    'colours':[]
                };
            }

            var img = $scope.CurrentProductDesign.colours.find(function (item) {
                return item.id == $scope.ProductRequest.Details.colourid;
            });
            if (img == undefined || img == nul) {
                img = { 'id': $scope.ProductRequest.Details.colourid };
                $scope.CurrentProductDesign.colours.push(img);
            }
            img.image = $scope.ProductRequest.Details.imageid;

            request = {
                "Item": $scope.CurrentProductDesign,
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "ProductdesignService/SaveImage",
                data: JSON.stringify(request)
            }).success(function (result) {
                
            }).error(function (result) {
            });
        }

        $scope.Productdesignimageuploaded = function () {
            $scope.ProductRequest.Details.imagechanged = true;
        }

        $scope.ValidateNewDesign = function () {
            if ($scope.ProductRequest.Details.designid == 'ADD DESIGN') {
                $scope.AddNewDesign();
            }
            else {

                if ($scope.ProductRequest.Details.designid != undefined &&
                    $scope.ProductRequest.Details.designid != null &&
                    $scope.ProductRequest.Details.designid != '') {

                    if ($scope.ProductRequest.supplierid > 0 &&
                        $scope.ProductRequest.Details.productid > 0) {

                        if ($scope.CurrentProductDesign == null ||
                            $scope.CurrentProductDesign.supplierid != $scope.ProductRequest.supplierid ||
                            $scope.CurrentProductDesign.productid != $scope.ProductRequest.Details.productid ||
                            $scope.CurrentProductDesign.designid != $scope.ProductRequest.Details.designid) {

                            $scope.CurrentProductDesign = {
                                'supplierid': $scope.ProductRequest.supplierid,
                                'productid': $scope.ProductRequest.Details.productid,
                                'designid': $scope.ProductRequest.Details.designid
                            };
                            request = {
                                "Item": $scope.CurrentProductDesign,
                                "SecurityToken": "String content"
                            }

                            $http({
                                method: 'POST',
                                url: "ProductdesignService/SearchImage",
                                data: JSON.stringify(request)
                            }).success(function (result) {
                                $scope.CurrentProductDesign = result;
                                $scope.ShowProductDesignImage();
                            }).error(function (result) {
                            });
                        }
                        else {
                            $scope.ShowProductDesignImage();
                        }

                    }
                }

                if ($scope.ProductRequest.mode == 2) {
                    $scope.setFocusdsn_size();
                }
                else if ($scope.ProductRequest.mode == 1) {
                    $scope.setFocusdsn_sizegroup();
                }
                else {
                    $scope.setFocusdsn_sizerange();
                }
            }
        }

        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo &&
                $rootScope.UserInfo.appsettings) {
                $scope.DisableDateChange = false || $rootScope.UserInfo.appsettings.system_disablepodatechange;

                $scope.IsMfg = $rootScope.UserInfo.appsettings.system_ismfgapplication;
                $scope.IsUANBased = $rootScope.UserInfo.appsettings.mfg_mode == 5;
                $scope.SupportDesignList = $rootScope.UserInfo.appsettings.system_disabledesigneditable || false;
                if ($rootScope.UserInfo.appsettings.system_pomode) {
                    $scope.ProductRequest.potype = $rootScope.UserInfo.appsettings.system_pomode;
                    $scope.Entity.potype = $scope.ProductRequest.potype;
                }
                if ($rootScope.UserInfo.appsettings.system_supportitemwiseentry) {
                    $scope.ItemNameList = restService.Get('ReferencelistService/ITEMS', true).then(function (results) {
                        $scope.ItemNameList = results;
                    });
                    $scope.CurrentProductid = -1;
                    $scope.FilterItemList = function (productid) {
                        if ($scope.CurrentProductid != productid) {
                            $scope.ItemList = [];
                            $scope.CurrentProductid = productid;
                            $.each($scope.ItemNameList, function (k, v) {
                                if (v.productid == productid)
                                    $scope.ItemList.push(v);
                            });
                        }
                    }
                }
                else {
                    $scope.FilterItemList = function (productid) { }
                }
            }
            else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 50);
            }
        }


        $timeout(function () {


            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "PurchaseorderService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.ShowSearch = true;
                    if (angular.isArray(result)) {
                        $scope.SearchResult = result;
                        if (result.length == 1) {
                            $scope.ManageSearchItem('Edit', result[0]);
                        }
                    }
                }).error(function (result) {
                });
            }

            var available = (typeof ($rootScope.windowSize) != 'undefined' &&
                typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 75) + 'px';
            }
            $scope.setFocusfirst();
            $scope.LoadAppSetting();

        }, 100);
    }]);
