
var app = angular.module('InfinityLab');

app.controller('ProductRequestFormController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, $stateParams) {
        $rootScope.AccordionName = $location.path();
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.IsMatrix = false;

        if (typeof ($stateParams.istextile) == 'undefined' || $stateParams.istextile)
            $scope.PurchaseConfig = { 'istextile': true, 'config': null };
        else
            $scope.PurchaseConfig = { 'istextile': $stateParams.istextile, 'config': $stateParams.productgroup };

        $scope.PrfReson = restService.Get('ReferencelistService/PRFRESON', true).then(function (results) {
            $scope.PrfReson = results;
        });
        $scope.PriorityList = restService.Get('ReferencelistService/PRIORITY', true).then(function (results) {
            $scope.PriorityList = results;
        });
        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        });
        $scope.LocationList = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.ProductGroup = { 'id': '', 'typeid': '' };
        if ($scope.PurchaseConfig.istextile == false && $scope.PurchaseConfig.config != null) {
            $scope.ProductGroup.id = '/' + $scope.PurchaseConfig.config.id;
            $scope.ProductGroup.typeid = '_PG_' + $scope.PurchaseConfig.config.id + '/' + $scope.PurchaseConfig.config.id;
        }
        else {
            $scope.PurchaseConfig.config = {};
            $scope.PurchaseConfig.config.materialname = 'Material';
            $scope.PurchaseConfig.config.patternname = 'Pattern';
            $scope.PurchaseConfig.config.stylename = 'Style';
            $scope.PurchaseConfig.config.sizename = 'Size';
            $scope.PurchaseConfig.config.colourname = 'Colour';
            $scope.PurchaseConfig.config.typename = 'Type';
            $scope.PurchaseConfig.istextile = true;
        }


        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS' + $scope.ProductGroup.id, true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.BrandList = restService.Get('ReferencelistService/BRAND' + $scope.ProductGroup.id, true).then(function (results) {
            $scope.BrandList = results;
        });
        $scope.PatternList = restService.Get('ReferencelistService/PATTERN' + $scope.ProductGroup.typeid, true).then(function (results) {
            $scope.PatternList = results;
        });
        $scope.StyleList = restService.Get('ReferencelistService/STYLE' + $scope.ProductGroup.typeid, true).then(function (results) {
            $scope.StyleList = results;
        });
        $scope.JumpList = [];
        $scope.SizeList = restService.Get('ReferencelistService/SIZE' + $scope.ProductGroup.typeid, true).then(function (results) {
            $scope.SizeList = results;
            /*var exist = false;
            $.each($scope.SizeList, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.SizeList.push({ id: -2, name: 'Jump' });
            }*/
        });

        $scope.ColorList = restService.Get('ReferencelistService/COLOUR' + $scope.ProductGroup.typeid, true).then(function (results) {
            $scope.ColorList = results;
        });

        $scope.MaterialList = restService.Get('ReferencelistService/MATERIAL' + $scope.ProductGroup.typeid, true).then(function (results) {
            $scope.MaterialList = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/TYPE' + $scope.ProductGroup.typeid, true).then(function (results) {
            $scope.TypeList = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.supplierid = 0;
        $scope.POResultEntity = {
            'warehouse': 0,
            'showroom': 0,
            'ordered': 0,
            'invoice': 0,
            'reorder': 0,
            'today': 0
        }
        $scope.Entity = {
            "id": 0,
            "requestedby": null,
            "requestedon": new Date(),
            "prfno": null,
            "reasonid": 0,
            "priorityid": 0,
            "companyid": 0,
            "locationid": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "Productrequestformitems": [],
            Details: {
                "productid": 0,
                "printingname": '',
                "supplierid": 0,
                "brandid": null,
                "patternid": null,
                "styleid": null,
                "sizeid": null,
                "colourid": null,
                "materialid": null,
                "typeid": null,
                "id": 0
            }
        };
        $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ManageItem = function (type, item) {
            if (type == 'Edit') {
                $scope.ProductRequest.productrequestformid = item.productrequestformid;
                $scope.ProductRequest.Details.id = item.itemid;
                $scope.ProductRequest.Details.productid = item.productid;
                $scope.ProductRequest.qty = item.qty;
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
                    if (result.length == 1) {
                        $scope.ProductRequest.Details = result[0];
                        $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify($scope.ProductRequest.Details)));
                    }
                });

            } else if (type == 'Delete') {
                if (item.productrequestformid == 0 || item.rqty == 0)
                    $scope.ProductRequest.Productrequestformitems.splice($scope.ProductRequest.Productrequestformitems.indexOf(item), 1);
            } else if (type == 'Cancel') {
                item.cqty += item.qty - item.rqty;

                var request = {
                    "Item": item,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "ProductrequestformitemsService/Save",
                    data: srequest
                }).success(function (result) {
                    if (result.id > 0) {
                        item.version = result.version;
                    }
                });

            }
        };

        $scope.ValidateAndAddItem = function (item) {
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
                if (result.length == 1 && !$scope.HasDiffer(result[0], $scope.ProductRequest.Details)) {
                    $scope.AddItemDirect(item, result[0].id);
                } else {
                    $scope.ShowProductSearch(result, $scope.AddItemDirect, item);
                }
            });
        };

        $scope.AddItemDirect = function (item, itemid) {
            item.itemid = itemid;
            item.printingname = item.printingname;
            item.qty = $scope.ProductRequest.Details.qty;
            item.pricerange = $scope.ProductRequest.Details.pricerange;
            item.salerange = $scope.ProductRequest.Details.salerange;
            
            if (!item.productrequestformid > 0)
                $scope.ProductRequest.Productrequestformitems.push(item);

            $scope.ProductRequest.Details.qty = 0;
            $scope.ProductRequest.productrequestformid = 0;
            $scope.ProductRequest.Actual = null;
        }

        $scope.HasDiffer = function (actual, item) {
            return false;
            return actual == null || item == null ||
                actual.productid != item.productid ||
                actual.sizeid != item.sizeid ||
                actual.materialid != item.materialid ||
                actual.patternid != item.patternid ||
                actual.colourid != item.colourid ||
                actual.styleid != item.styleid;
        }

        $scope.AddItem = function () {

            $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandList, '');
            $scope.ProductRequest.Details.printingname = $scope.SearchRef($scope.ProductRequest.Details.productid, $scope.ProductList, '');
            $scope.ProductRequest.Details.patternname = $scope.SearchRef($scope.ProductRequest.Details.patternid, $scope.PatternList, '');
            $scope.ProductRequest.Details.materialname = $scope.SearchRef($scope.ProductRequest.Details.materialid, $scope.MaterialList, '');
            $scope.ProductRequest.Details.colourname = $scope.SearchRef($scope.ProductRequest.Details.colourid, $scope.ColorList, '');
            $scope.ProductRequest.Details.sizename = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.SizeList, '');
            $scope.ProductRequest.Details.stylename = $scope.SearchRef($scope.ProductRequest.Details.styleid, $scope.StyleList, '');
            $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');

            var description = [];
            if ($scope.ProductRequest.Details.printingname != '')
                description.push($scope.ProductRequest.Details.printingname);
            if ($scope.ProductRequest.Details.brandname != '')
                description.push($scope.ProductRequest.Details.brandname);
            if ($scope.ProductRequest.Details.stylename != '')
                description.push($scope.ProductRequest.Details.stylename);
            if ($scope.ProductRequest.Details.materialname != '')
                description.push($scope.ProductRequest.Details.materialname);
            if ($scope.ProductRequest.Details.colourname != '')
                description.push($scope.ProductRequest.Details.colourname);
            if ($scope.ProductRequest.Details.patternname != '')
                description.push($scope.ProductRequest.Details.patternname);
            if ($scope.ProductRequest.Details.typename != '')
                description.push($scope.ProductRequest.Details.typename);
            
            if ($scope.ProductRequest.Details.sizeid == -2) {
                if ($scope.JumpList.length == 0) {
                    $scope.Toaster.Info("Please enter jump information");
                    return;
                }
                item = {
                    'productid': $scope.ProductRequest.Details.productid,
                    'itemid': $scope.ProductRequest.Details.id,
                    'qty': $scope.ProductRequest.Details.qty,
                    'designid': $scope.ProductRequest.Details.designid,
                    'cqty': 0,
                    'rqty': 0,
                    'isactive': true,
                    'version': 1,
                    'supplierid': $scope.supplierid,
                    'productrequestformid': 0,
                    'id': 0,
                    'designid': $scope.ProductRequest.Details.designid,
                    'printingname': $scope.ProductRequest.Details.printingname,
                    'brandname': $scope.ProductRequest.Details.brandname,
                    'typename': $scope.ProductRequest.Details.typename,
                    'patternname': $scope.ProductRequest.Details.patternname,
                    'stylename': $scope.ProductRequest.Details.stylename,
                    'materialname': $scope.ProductRequest.Details.materialname,
                    'colourname': $scope.ProductRequest.Details.colourname,
                    'sizename': $scope.ProductRequest.Details.sizename,
                    'description': description.join('/'),
                    'pricerange': $scope.ProductRequest.Details.pricerange,
                    'salerange': $scope.ProductRequest.Details.salerange
                };

                var detail = {
                    'brandid': $scope.ProductRequest.Details.brandid,
                    'typeid': $scope.ProductRequest.Details.typeid,
                    'patternid': $scope.ProductRequest.Details.patternid,
                    'styleid': $scope.ProductRequest.Details.styleid,
                    'materialid': $scope.ProductRequest.Details.materialid,
                    'colourid': $scope.ProductRequest.Details.colourid,
                    'sizeid': $scope.ProductRequest.Details.sizeid,
                    'jumplist': $scope.JumpList
                };

                item.detail = (JSON.parse(JSON.stringify(detail)));
                $scope.ProductRequest.Productrequestformitems.push(item);
                $scope.JumpList = [];
                $scope.ProductRequest.qty = 0;
                $scope.ProductRequest.productrequestformid = 0;
                $scope.ProductRequest.Actual = null;
                return;
            }
            else {
                if ($scope.ProductRequest.Details.qty == null || $scope.ProductRequest.Details.qty <= 0) {
                    $scope.Toaster.Info("Please enter order qty");
                    return;
                }

                //if ($scope.ProductRequest.Details.id == null || $scope.ProductRequest.Details.id == 0) {
                //    $scope.Toaster.Info("Please select the item to be requested by Clicking Go button.");
                //    return;
                //}
            }



            var item = {};
            var hasitemdiffer = $scope.HasDiffer($scope.ProductRequest.Actual, $scope.ProductRequest.Details);

            if ($scope.ProductRequest.productrequestformid > 0) {
                item = $scope.ProductRequest.Productrequestformitems.find(function (itm) {
                    return itm.productrequestformid == $scope.ProductRequest.productrequestformid;
                });
                if ($scope.ProductRequest.qty - (item.cqty + item.rqty)) {
                    return;
                }
                if (hasitemdiffer) {
                    $scope.ValidateAndAddItem(item);
                }
            }
            else {
                item = {
                    'productid': $scope.ProductRequest.Details.productid,
                    'itemid': $scope.ProductRequest.Details.id,
                    'qty': $scope.ProductRequest.Details.qty,
                    'cqty': 0,
                    'rqty': 0,
                    'isactive': true,
                    'version': 1,
                    'designid': $scope.ProductRequest.Details.designid,
                    'printingname': $scope.ProductRequest.Details.printingname,
                    'brandname': $scope.ProductRequest.Details.brandname,
                    'typename': $scope.ProductRequest.Details.typename,
                    'patternname': $scope.ProductRequest.Details.patternname,
                    'stylename': $scope.ProductRequest.Details.stylename,
                    'materialname': $scope.ProductRequest.Details.materialname,
                    'colourname': $scope.ProductRequest.Details.colourname,
                    'sizename': $scope.ProductRequest.Details.sizename,
                    'supplierid': $scope.supplierid,
                    'productrequestformid': 0,
                    'id': 0,
                    'detail': (JSON.parse(JSON.stringify($scope.ProductRequest.Details))),
                    'description': description.join('/'),
                    'pricerange': $scope.ProductRequest.Details.pricerange,
                    'salerange': $scope.ProductRequest.Details.salerange
                };
                if (hasitemdiffer) {
                    $scope.ValidateAndAddItem(item);
                }
                else {
                    $scope.ProductRequest.Productrequestformitems.push(item);
                    $scope.ProductRequest.qty = 0;
                    $scope.ProductRequest.productrequestformid = 0;
                    $scope.ProductRequest.Actual = null;
                }
            }
        };

        $scope.POResult = (JSON.parse(JSON.stringify($scope.POResultEntity)));
        $scope.ErrorStyle = "success";
        $scope.ErrorMessage = "";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Agent = (JSON.parse(JSON.stringify($scope.Entity)));
        }


        $scope.qtyChanged = function () {
            $scope.ProductRequest.Details.qty = 0;
            $.each($scope.JumpList, function (k, v) {
                $scope.ProductRequest.Details.qty += $rootScope.GetNumber(v.qty);
            });
        }

        //$scope.SearchPO = function () {

        //    $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandList, '');
        //    $scope.ProductRequest.Details.printingname = $scope.SearchRef($scope.ProductRequest.Details.productid, $scope.ProductList, '');
        //    $scope.ProductRequest.Details.patternname = $scope.SearchRef($scope.ProductRequest.Details.patternid, $scope.PatternList, '');
        //    $scope.ProductRequest.Details.materialname = $scope.SearchRef($scope.ProductRequest.Details.materialid, $scope.MaterialList, '');
        //    $scope.ProductRequest.Details.colourname = $scope.SearchRef($scope.ProductRequest.Details.colourid, $scope.ColorList, '');
        //    $scope.ProductRequest.Details.sizename = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.SizeList, '');
        //    $scope.ProductRequest.Details.stylename = $scope.SearchRef($scope.ProductRequest.Details.styleid, $scope.StyleList, '');
        //    $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');

        //    var request = {
        //        "Item": $scope.ProductRequest.Details,
        //        "securitytoken": "String content"
        //    }
        //    var srequest = JSON.stringify(request);
        //    $http({
        //        method: 'POST',
        //        url: "ProductrequestformService/SearchPO",
        //        data: srequest
        //    }).success(function (result) {
        //        $scope.POResult = result;
        //        $scope.ProductRequest.Details.id = result.itemid;
        //    });
        //};

        $scope.SearchItem = function () {

            if ($scope.ProductRequest.Details.sizeid == -2) {
                $rootScope.Prompt('Jump', 'Jump (start-increment-end)', function (count) {
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

                        $scope.$apply(function () {
                            $scope.JumpList = idList;
                            $scope.ProductRequest.Details.qty = 0;
                            $('#jumplist INPUT').first().focus();
                        });
                    }
                });
                return;
            }
            if ($scope.ProductRequest.Details.productid == null || $scope.ProductRequest.Details.productid == 0) {
                $scope.Toaster.Info("Please select product");
                return;
            }

            $scope.ProductRequest.Details.id = 0;
            $scope.ProductRequest.Details.sellingname = "";
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
                if (result.length == 1) {
                    var design = $scope.ProductRequest.Details.designid;
                    var pricerange = $scope.ProductRequest.Details.pricerange;
                    var salerange = $scope.ProductRequest.Details.salerange;
                    var qty = $scope.ProductRequest.Details.qty;
                    $scope.ProductRequest.Details = result[0];
                    $scope.ProductRequest.Details.designid = design;
                    $scope.ProductRequest.Details.pricerange = pricerange;
                    $scope.ProductRequest.Details.salerange = salerange;
                    $scope.ProductRequest.Details.qty = qty;
                    $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify($scope.ProductRequest.Details)));
                    $scope.SearchPO();
                }
                else
                    $scope.ShowProductSearch(result);
            });
        };

        $scope.SearchPO = function () {

            $scope.ProductRequest.Details.glndesignid = $scope.ProductRequest.Details.designid;
            var request = {
                "Item": $scope.ProductRequest.Details,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ProductrequestformService/SearchPO",
                data: srequest
            }).success(function (result) {
                $scope.POResult = result;
                $scope.ProductRequest.Details.id = result.itemid;
                $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify($scope.ProductRequest.Details)));
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

        $scope.Clear = function () {
            $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.supplierid = 0;
        };

        $scope.Save = function () {
            if ($scope.ProductRequest.requestedon == null || $scope.ProductRequest.requestedon == 0) {
                $rootScope.Toaster.Error("Please select Date!");
                return;
            }
            if ($scope.ProductRequest.requestedby == null || $scope.ProductRequest.requestedby == 0) {
                $rootScope.Toaster.Error("Please select requested person!");
                return;
            }
            if ($scope.ProductRequest.companyid == null || $scope.ProductRequest.companyid == 0) {
                $rootScope.Toaster.Error("Please select company!");
                return;
            }

            request = {
                "item": $scope.ProductRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ProductrequestformService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with Request No : " + result.prfno);
                    $scope.ProductRequest.prfno = result.prfno;
                    //$scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.Clear();
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
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
                    confirm: function () {

                        request = {
                            "Item": item,
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "ProductrequestformitemsService/Delete",
                            data: srequest
                        }).success(function (result) {

                            if (result) {
                                $scope.ErrorMessage = "Successfully deleted";
                                $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                            }
                        }).error(function (result) {

                        });
                    }
                });
            }
            else if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.ProductRequest = item;
            }
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
            });

            var dialog = ngDialog.open({
                template: 'ProductSearch',
                data: { 'scope': $scope, 'list': list, 'iteminfo': iteminfo, 'callback': callback },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.PopupList = $scope.ngDialogData.list;
                    $scope.ShowCreate = $scope.PopupList.length <= 1;
                    var coreScope = $scope.ngDialogData.scope;
                    var item = coreScope.ProductRequest.Details;
                    $scope.printingname = coreScope.SearchRef(item.productid, coreScope.ProductList, '')
                    $scope.Create = function () {
                        item.brandname = coreScope.SearchRef(item.brandid, coreScope.BrandList, '');
                        item.productname = coreScope.SearchRef(item.productid, coreScope.ProductList, '');
                        item.patternname = coreScope.SearchRef(item.patternid, coreScope.PatternList, '');
                        item.materialname = coreScope.SearchRef(item.materialid, coreScope.MaterialList, '');
                        item.colourname = coreScope.SearchRef(item.colourid, coreScope.ColorList, '');
                        item.sizename = coreScope.SearchRef(item.sizeid, coreScope.SizeList, '');
                        item.stylename = coreScope.SearchRef(item.styleid, coreScope.StyleList, '');
                        item.typename = coreScope.SearchRef(item.typeid, coreScope.TypeList, '');

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
                            data.value.callback(data.value.iteminfo, data.value.result.id);
                        }
                        else {
                            data.value.result.pricerange = $scope.ProductRequest.Details.pricerange;
                            data.value.result.salerange = $scope.ProductRequest.Details.salerange;
                            data.value.result.qty = $scope.ProductRequest.Details.qty;
                            data.value.result.designid = $scope.ProductRequest.Details.designid;
                            $scope.ProductRequest.Details = data.value.result;
                            $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify(data.value.result)));
                            $scope.SearchPO();
                        }
                    }
                }
            });
        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);