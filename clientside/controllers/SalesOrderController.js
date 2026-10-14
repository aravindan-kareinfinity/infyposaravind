
var app = angular.module('InfinityLab');

app.controller('SalesOrderController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', '$state', 'cntxService', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, $state, cntxService, $filter) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.orderqty = 0;
        $scope.InputMode = 'SizeGroupColour';
        $scope.AdditionalCharges = [];
        $scope.BoxList = [];
        $scope.AvailableColorList = [];
        $scope.EmptyStockLocations = [];
        $scope.SupportAdditionalCharges = false;
        $scope.SupportBoxCharges = false;
        $scope.system_possizelist = null;
        $scope.system_sizegrouplayout = false;
        $scope.SizeIndexList = null;
        $scope.mfg_sovalidate = false;
        $scope.mfg_so_po = false;
        $scope.mfg_sosizeratio = false;
        $scope.mfg_sosizeratiorule = '';
        $scope.mfg_sosizeratioList = [];
        $scope.selectedcount = 0;
        $scope.allowchangestatus = false;
        $scope.panelheight = { 'height': '500px' };
        restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
            $scope.BoxList.push({ 'id': -1, 'name': 'NO Box', 'add': 0, 'remove': 0 });
            $.each(results, function (k, v) {
                if (v.additionalinfo && v.additionalinfo.paytype == 'PACKING') {
                    $scope.AdditionalCharges.push({ 'id': v.id, 'name': v.name, 'value': v.additionalinfo.billdiscount });
                }
                if (v.additionalinfo && v.additionalinfo.paytype == 'SERVICE') {
                    $scope.AdditionalCharges.push({ 'id': v.id, 'name': v.name, 'value': v.additionalinfo.billdiscount });
                }
                if (v.additionalinfo && v.additionalinfo.paytype == 'BOX') {
                    $scope.BoxList.push({ 'id': v.id, 'name': v.name, 'add': v.additionalinfo.maxlimit, 'remove': v.additionalinfo.billdiscount });
                }
            })
            $scope.SupportAdditionalCharges = $scope.AdditionalCharges.length > 0;
            $scope.SupportBoxCharges = $scope.BoxList.length > 1;
            if ($scope.egrid != null)
                $scope.ConfigEditor();
        });
        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.Employees = restService.Get('ReferencelistService/EMPLOYONLY', true).then(function (results) {
            $scope.Employees = results;
        });

        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });

        $scope.TaxList = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.TaxList = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
            $rootScope.BindCompany($scope, 'ProductRequest.companyid');
        });

        $scope.LocationList = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.LocationList = [];
            $.each(results, function (k, v) {
                if (v.additionalinfo == undefined ||
                    (!v.additionalinfo.disabled && !v.additionalinfo.iswarehouse)) {
                    $scope.LocationList.push(v);
                    $scope.EmptyStockLocations.push(v.id);
                }
            });
            $rootScope.BindLocation($scope, 'ProductRequest.deliverylocationid', function () {
                $scope.StockLocationChanged();
            });
        });

        $scope.StyleList = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.StyleList = results;
        });
        $scope.Customers = restService.Get('ReferencelistService/CUSTOMERLEDGERUNAPPROVED', true).then(function (results) {
            $scope.Customers = results;
            //$timeout(function () {
            //    $scope.setFocuscustomer();
            //    $scope.$digest();
            //}, 50);
        });
        $scope.LocalContext = cntxService.CreatePageContext('salesorder');
        $scope.JumpList = [];
        $scope.FullColorList = [];
        $scope.FullSizeList = [];
        $scope.FullSizeGroupList = [];
        $scope.FullSizeNumbers = [];
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
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
                $scope.mfg_sosizeratioList = [];
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

                        if (v.additionalinfo.mfgconfig && v.additionalinfo.mfgconfig.SizeRatioList) {
                            $.each(v.additionalinfo.mfgconfig.SizeRatioList, function (kk, vv) {
                                var size = $scope.SizeList.find(function (item) {
                                    return item.id == vv.sizeid
                                });
                                if (size && size != null) {
                                    vv.sizename = size.name;
                                }
                            });
                        }

                        if (v.additionalinfo.mfgconfig && v.additionalinfo.mfgconfig.sizeratio) {
                            $scope.mfg_sosizeratioList.push({
                                'id': v.id,
                                'name': v.name,
                                'type': v.additionalinfo.mfgconfig.sizeratiotype,
                                'possizelist': v.possizelist,
                                'sizegroups': v.additionalinfo.mfgconfig.SizeRatioList
                            });
                        }
                        v.possizelist = v.possizelist.join(',');
                    }
                })
            });
        });

        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
            $scope.FullColorList = results;
            $.each(results, function (k, v) {
                v.displaycode = v.name;
                v.colourid = v.id;
            });
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
        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });
        $scope.CityList = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.CityList = results;
        });
        $scope.District = restService.Get('ReferencelistService/DISTRICT', true).then(function (results) {
            $scope.District = results;
        });

        $scope.ShowStockView = false;

        $scope.UpdatePriceFromConfig = function (result) {
            if ($scope.ProductRequest && $scope.ProductRequest.marginprice
                && $scope.ProductRequest.marginprice > 0) {
                if ($scope.ProductRequest.marginmode == 'price') {
                    $scope.ProductRequest.Details.price = result.salerate * (1 - ($scope.ProductRequest.marginprice / 100));
                    //SaleRate*(1-Markdown/100)
                } else if ($scope.ProductRequest.marginmode == 'mrp') {
                    $scope.ProductRequest.Details.price = result.mrp * (1 - ($scope.ProductRequest.marginprice / 100));
                }
            }
        }

        $scope.SampleDesignChanged = function () {
            $scope.JumpListStyle = 'col-md-2';
            $scope.ProductRequest.Details.imageid = null;
            if ($scope.ProductRequest.Details.sampledesignid &&
                $scope.ProductRequest.Details.sampledesignid > 0) {
                var sample = $scope.CustomerSampleDesignList.find(function (item) {
                    return item.id == $scope.ProductRequest.Details.sampledesignid;
                });
                if (sample && sample != null) {
                    $scope.ProductRequest.Details.colourid = sample.colourid;
                    $scope.ProductRequest.Details.designimageid = sample.imageid;
                    $scope.ProductRequest.Details.pricerange = sample.requiredprice;
                    var JumpList = [];
                    $.each(sample.sizes, function (k, v) {
                        JumpList.push({
                            'id': v.id, 'name': $scope.SearchRef(v.id, $scope.SizeList, '-'), 'uid': v.uid
                        });
                    });
                    $scope.SampleSizeList = JumpList;
                    //$scope.JumpListStyle = 'col-md-2';
                    //if (JumpList.length<=1)
                    //    $scope.JumpListStyle = 'col-md-6';
                    //else if (JumpList.length <= 2)
                    //    $scope.JumpListStyle = 'col-md-4';
                    //else if (JumpList.length <= 3)
                    //    $scope.JumpListStyle = 'col-md-3';
                }
            }
        }

        $scope.SampleChanged = function () {
            $scope.ProductRequest.Details.imageid = null;
            if ($scope.ProductRequest.Details.sampleid &&
                $scope.ProductRequest.Details.sampleid > 0) {
                var sample = $scope.CustomerSampleList.find(function (item) {
                    return item.id == $scope.ProductRequest.Details.sampleid;
                });
                if (sample && sample != null) {
                    $scope.ProductRequest.Details.sampleimageid = sample.imageid;
                    $scope.CustomerSampleDesignList = sample.designs;
                    $scope.ProductRequest.Details.productid = sample.prdid;
                    $scope.ProductRequest.Details.prddivisionid = sample.prddivid;
                }
            }
        }

        $scope.CustomerChanged = function () {
            $scope.ProductRequest.hasshipto = false;
            $scope.ProductRequest.marginmode = 0;
            $scope.ProductRequest.marginprice = 0;
            $scope.ProductRequest.billdiscount = 0;
            $scope.ProductRequest.pricesource = 0;
            if (typeof ($scope.ProductRequest.id) == 'undefined' || $scope.ProductRequest.id == 0) {
                $scope.ProductRequest.discountpercentage = 0;
            }
            if ($scope.ProductRequest.customerid &&
                $scope.ProductRequest.customerid > 0) {
                var customer = $scope.Customers.find(function (item) {
                    return item.id == $scope.ProductRequest.customerid;
                });
                if (customer && customer != null) {
                    if ($scope.ProductRequest.id == undefined || $scope.ProductRequest.id <= 0)
                        $scope.ProductRequest.agentid = customer.agentid;
                    $scope.ProductRequest.marginmode = customer.marginmode;
                    $scope.ProductRequest.marginprice = customer.marginprice;
                    $scope.ProductRequest.pricesource = customer.psource;
                    if (typeof ($scope.ProductRequest.id) == 'undefined' || $scope.ProductRequest.id == 0) {
                        $scope.ProductRequest.hasshipto = customer.type == 7;
                        if (customer.billdiscount > 0)
                            $scope.ProductRequest.discountpercentage = customer.billdiscount;
                    }
                    else if (!$scope.ProductRequest.hasshipto)
                        $scope.ProductRequest.hasshipto = customer.type == 7;

                    if ($scope.IsSampleBased) {
                        restService.Get('ReferencelistService/MFGSAMPLES/' + $scope.ProductRequest.customerid, true).then(function (results) {
                            $scope.CustomerSampleList = results;
                        });
                    }

                }
            }
        }

        $scope.ItemOQtyChange = function (item) {
            if (typeof (item.jumpqty) != 'undefined' && item.jumpqty != null) {
                var oqty = parseFloat(item.oqty);
                if (isNaN(oqty))
                    oqty = 0;
                $.each(item.jumpqty, function (k, v) {
                    v.oqty = oqty;
                });
                $scope.CalculateOrderQty();
            }
        }

        $scope.ItemPriceChange = function (item) {
            if (typeof (item.jumpqty) != 'undefined' && item.jumpqty != null) {
                $.each(item.jumpqty, function (k, v) {
                    v.pricerange = item.pricerange;
                    v.price = item.pricerange;
                });
            }
        }

        //$scope.FilterDesignCode = function (item) {
        //    return typeof ($scope.ProductRequest.Details.fshade) == 'undefined' ||
        //        $scope.ProductRequest.Details.fshade == null ||
        //        $scope.ProductRequest.Details.fshade == '' ||
        //        $scope.ProductRequest.Details.fshade == item.code;
        //};

        $scope.SelectSizeGroup = function (shade) {
            //$scope.ProductRequest.Details.fshade = '';
            if (shade && shade != null && (shade.indexOf(',') < 0 && shade.indexOf('-') < 0)) {
                var designitemcode = $scope.AvailableColorList.find(function (item) {
                    return shade == item.code;
                });
                if (designitemcode) {
                    var sizegroup = $scope.SizeGroupList.find(function (item) {
                        return item.designcode == designitemcode.designcode;
                    });
                    if (sizegroup) {
                        $scope.ProductRequest.Details.sizegroupid = sizegroup.id;
                        $scope.SetSizeGroup();
                        $scope.setFocusshade_size();
                    }
                } else {
                    designitemcode = $scope.AvailableColorList.find(function (item) {
                        return shade == item.displaycode;
                    });
                    if (designitemcode) {
                        var sizegroup = $scope.SizeGroupList.find(function (item) {
                            return item.designcode == designitemcode.designcode;
                        });
                        if (sizegroup) {
                            $scope.ProductRequest.Details.sizegroupid = sizegroup.id;
                            $scope.SetSizeGroup();
                            $scope.setFocusshade_size();
                        }
                    }
                }
            }
            else {
                var shadelist = shade.split(',');
                if (shade.indexOf('-') > 0) {
                    var slist = [];
                    $.each(shadelist, function (k, v) {
                        if (v.indexOf('-') > 0) {
                            var from = parseInt(v.split('-')[0]);
                            var to = parseInt(v.split('-')[1]);
                            for (var i = from; i <= to; i++) {
                                slist.push(i.toString());
                            }
                        }
                        else {
                            slist.push(v);
                        }
                    })
                    $scope.ProductRequest.Details.shades = slist.join(',');
                    shadelist = slist;
                }

                var tsizegroup = $scope.SizeGroupList.find(function (item) {
                    return item.colourcode == shadelist[0];
                });

                var sizegroup = (JSON.parse(JSON.stringify(tsizegroup)));
                var idList = [];
                $.each(sizegroup.list, function (k, v) {
                    idList.push({
                        'sizeid': v.id,
                        'name': v.name,
                        'uid': v.uid,
                        'oqty': $scope.ProductRequest.Details.groupqty,
                        'wqty': v.wip,
                        'pqty': v.order,
                        'sqty': v.stock,
                        'colourid': v.colourid,
                        'index': $scope.SizeIndexList == null ? 100 : $scope.SizeIndexList.indexOf(v.name)
                    });
                });

                for (var i = 1; i < shadelist.length; i++) {
                    var tsizegroup = $scope.SizeGroupList.find(function (item) {
                        return item.colourcode == shadelist[i];
                    });
                    $.each(tsizegroup.list, function (k, v) {
                        var existid = idList.find(function (item) {
                            return item.sizeid == v.id;
                        });
                        if (existid && existid != null) {
                            existid.wqty += v.wip;
                            existid.pqty += v.order;
                            existid.sqty += v.stock;
                        }
                        //else {
                        //    idList.push({
                        //        'sizeid': v.id,
                        //        'name': v.name,
                        //        'uid': v.uid,
                        //        'oqty': $scope.ProductRequest.Details.groupqty,
                        //        'wqty': v.wip,
                        //        'pqty': v.order,
                        //        'sqty': v.stock,
                        //        'colourid': v.colourid,
                        //        'index': idList.length+1
                        //    });
                        //}
                    });
                }

                idList.sort(function (a, b) {
                    return a.index - b.index;
                });

                $scope.JumpList = idList;
                var lengthysize = $scope.JumpList.find(function (sitem) {
                    return sitem.name != null &&
                        sitem.name.length > 5;
                });
                $scope.JumpListStyle = lengthysize && lengthysize != null ? 'col-md-6' : 'col-md-2';
                $scope.StockList = idList;
                $scope.setFocusshade_size();
            }
        }

        $scope.productChange = function () {

            if ($scope.ismfgapplication &&
                !$scope.fabricbasedorder) {
                var request = {
                    "Item": {
                        'productid': $scope.ProductRequest.Details.productid
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "MfgdesignService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length > 0) {
                        $scope.ProductRequest.Details.designid = result[0].name;
                        $scope.ProductRequest.Details.brandid = result[0].brandid;
                    }
                });
            }
        }

        $scope.GetStockLocations = function () {

            var stocklocations = [];
            if (typeof ($scope.ProductRequest.deliverylocationid) != 'undefined' && $scope.ProductRequest.deliverylocationid != null &&
                $scope.ProductRequest.deliverylocationid > 0) {
                var locationitem = $scope.LocationList.find(function (item) {
                    return item.id == $scope.ProductRequest.deliverylocationid
                });
                if (locationitem && locationitem.additionalinfo && locationitem.additionalinfo != null && locationitem.additionalinfo.iswarehouse) {
                    $rootScope.Toaster.Error("Warehouse can't be selected as stock location");
                    return;
                }
                if (locationitem && locationitem.additionalinfo && locationitem.additionalinfo != null && locationitem.additionalinfo.disabled) {
                    $rootScope.Toaster.Error("marketing/office location can't be selected as stock location");
                    return;
                }
                stocklocations = [$scope.ProductRequest.deliverylocationid];
            }
            else {
                $rootScope.Toaster.Error("Please select the stock location");
                //stocklocations = $scope.EmptyStockLocations;
            }
            return stocklocations;
        }
        $scope.LastDesignID = '';
        $scope.SearchDesign = function (callback, shade) {

            if ($scope.IsSampleBased) return;

            if (typeof ($scope.ProductRequest.customerid) == 'undefined' ||
                $scope.ProductRequest.customerid == null ||
                $scope.ProductRequest.customerid <= 0) {
                $rootScope.Toaster.Error("Please select the customer before search the design!");
                return;
            }
            if ($scope.ProductRequest.Details.designid == '0') {
                $scope.Save();
                return;
            }

            //if ($scope.ProductRequest.mode == '3') {
            //    $scope.setFocusproduct();
            //    return;
            //}

            if ($scope.ismfgapplication || $scope.system_isb2bapplication) {
                var searchdesign = $scope.ProductRequest.Details.designid + (shade ? ('/' + shade) : '');
                //if ($scope.LastDesignID == $scope.searchdesign) {
                //    if ($scope.InputMode == 'SizeGroupColourShade') {
                //        $scope.setFocusshades();
                //    }
                //    else {
                //        if ($scope.ProductRequest.mode == 1) {
                //            if ($scope.SizeGroupList.length == 1) {
                //                if ($scope.InputMode == 'SizeGroupColour') {
                //                    $scope.setFocusallqty();
                //                }
                //            }
                //            else {
                //                $scope.setFocussizegroup();
                //            }
                //        }
                //        else {
                //            $scope.setFocusproduct();
                //        }
                //    }
                //}

                if (typeof (searchdesign) == 'undefined' || searchdesign == null || searchdesign == '') {
                    $rootScope.Toaster.Error("Please enter the design!");
                    return;
                }

                var stocklocations = $scope.GetStockLocations();
                if (stocklocations.length == 0) return;

                var request = {
                    "Item": {
                        'stocklocations': stocklocations,
                        'name': searchdesign.toUpperCase(),
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
                    //$scope.ProductRequest.Details.fshade = '';
                    if (result != null && typeof (result.productid) != 'undefined') {
                        $scope.StockList = [];
                        $scope.LastDesignID = $scope.searchdesign;
                        $scope.ProductRequest.Details.iscoredesign = result.iscoredesign;
                        $scope.ProductRequest.Details.productid = result.productid;
                        $scope.ProductRequest.Details.brandid = result.brandid;
                        $scope.ProductRequest.Details.materialid = result.materialid;
                        $scope.ProductRequest.Details.styleid = result.styleid;
                        $scope.ProductRequest.Details.patternid = result.patternid;
                        $scope.ProductRequest.Details.typeid = result.typeid;
                        $scope.ProductRequest.Details.designuid = result.designid;
                        $scope.ProductRequest.Details.mrp = result.mrp;
                        $scope.ProductRequest.Details.boxid = result.boxid;
                        $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');
                        if (typeof (shade) == 'undefined')
                            $scope.ProductRequest.Details.shades = '';
                        switch ($scope.ProductRequest.pricesource) {
                            case 2:
                                $scope.ProductRequest.Details.price = result.rate;
                                $scope.ProductRequest.Details.pricerange = result.rate;
                                break;
                            case 1:
                                $scope.ProductRequest.Details.price = result.wsrate;
                                $scope.ProductRequest.Details.pricerange = result.wsrate;
                                break;
                            case 0:
                                $scope.ProductRequest.Details.price = result.salerate;
                                $scope.ProductRequest.Details.pricerange = result.salerate;
                                break;
                        }
                        $scope.ProductRequest.Details.designid = result.design;
                        $scope.ProductRequest.Details.colourid = result.colourid;
                        $scope.ProductRequest.Details.groupqty = '';
                        if ($scope.InputMode == 'SizeGroupColourShade') {
                            $scope.ProductRequest.mode = 0;
                            if (result.colourlist != null && result.colourlist.length == 1) {
                                $scope.ProductRequest.Details.shades = result.colourlist[0].displaycode;
                            }
                            if (result.sizeid > 0) {
                                $scope.ProductRequest.Details.shadesize = $scope.SearchRef(result.sizeid, $scope.FullSizeList, '');
                            }
                        }
                        if ($scope.InputMode == 'UAN') {
                            $scope.ProductRequest.mode = 4;
                            if (result.colourlist != null && result.colourlist.length == 1) {
                                $scope.ProductRequest.Details.shades = result.colourlist[0].displaycode;
                            }
                            if (result.sizeid > 0) {
                                $scope.ProductRequest.Details.shadesize = $scope.SearchRef(result.sizeid, $scope.FullSizeList, '');
                            }
                        }
                        //else
                        //    $scope.ProductRequest.mode = 1;
                        $scope.SizeGroupList = result.sizegroup;

                        $scope.UpdatePriceFromConfig(result);

                        if (result.colourlist != null && result.colourlist.length > 0) {

                            $.each(result.colourlist, function (k, v) {
                                v.colourid = v.id;
                            });

                            $scope.ColorList = result.colourlist;
                            $scope.AvailableColorList = result.colourlist;
                        }
                        else {
                            $scope.ColorList = $scope.FullColorList;
                        }

                        if (result.sizegroup != null &&
                            result.sizegroup.length == 1) {
                            $scope.SizeList = result.sizegroup[0].list;
                            $scope.ProductRequest.Details.shades = result.colourlist[0].displaycode;
                            if ($scope.InputMode == 'SizeGroupColourShade') {
                                $scope.ProductRequest.mode = 0;
                            }
                            else {
                                //$scope.ProductRequest.mode = 1;
                                $scope.ProductRequest.Details.colourcode = result.colourlist[0].code;
                                $scope.ProductRequest.Details.colourshade = result.colourlist[0].displaycode;
                            }
                            $scope.ProductRequest.Details.sizegroupid = result.sizegroup[0].id;
                            if (typeof ($scope.ProductRequest.Details.groupqty) == 'undefined')
                                $scope.ProductRequest.Details.groupqty = '';
                            var groupqty = parseInt($scope.ProductRequest.Details.groupqty);
                            if (isNaN(groupqty)) groupqty = 0;
                            $scope.ProductRequest.Details.groupqty = groupqty + 1;
                            $scope.SetSizeGroup();
                            if (result.auto) {
                                var existing = $scope.ProductRequest.Salesorderitems.find(function (item) {
                                    return $scope.ProductRequest.Details.productid == item.productid &&
                                        $scope.ProductRequest.Details.brandid == item.brandid &&
                                        $scope.ProductRequest.Details.designid == item.designid &&
                                        $scope.ProductRequest.Details.sizegroup == item.sizegroup &&
                                        $scope.ProductRequest.Details.colourid == item.colourid;
                                });
                                if (typeof (existing) == 'undefined' || existing == null) {
                                    $scope.AddItem();
                                }
                                else {
                                    $scope.ProductRequest.Details.groupqty = existing.oqty + 1;
                                    existing.oqty = $scope.ProductRequest.Details.groupqty;
                                    $.each(existing.jumpqty, function (k, v) {
                                        v.oqty = $scope.ProductRequest.Details.groupqty;
                                    });
                                }
                                $scope.CalculateOrderQty();
                                $scope.ProductRequest.Details.designid = '';
                                $scope.ProductRequest.Details.designuid = 0;
                                $scope.ProductRequest.Details.groupqty = '';
                                $scope.UpdateEditGrid();
                            }

                        }
                        else if (result.sizegroup == null ||
                            result.sizegroup.length == 0) {
                            if ($scope.InputMode == 'SizeGroupColourShade')
                                $scope.ProductRequest.mode = 0;
                            else
                                $scope.ProductRequest.mode = 2;
                            $scope.SizeList = $scope.FullSizeList;
                            $scope.ProductRequest.Details.sizeid = result.sizeid;
                        }
                        $scope.ProductRequest.Details.colourid = result.colourid;

                        if (result.sizegroup && result.sizegroup.length > 0) {
                            var filteredsizelist = [];
                            $.each(result.sizegroup, function (k, v) {
                                $.each(v.list, function (kk, vv) {
                                    var exist = filteredsizelist.find(function (item) {
                                        return item.id == vv.id;
                                    })
                                    if (exist == undefined || exist == null)
                                        filteredsizelist.push({ 'id': vv.id, 'name': vv.name });
                                })
                            });
                            $scope.SizeList = filteredsizelist;
                        }

                        $timeout(function () {
                            if ($scope.InputMode == 'SizeGroupColourShade') {
                                if (shade)
                                    $scope.setFocusshade_size();
                                else {
                                    if (result.sizegroup.length == 1) {
                                        $scope.ProductRequest.Details.shades = result.colourlist[0].displaycode;
                                        $scope.setFocusshade_size();
                                    }
                                    else {
                                        $scope.setFocusshades();
                                    }
                                }
                            }
                            else {
                                if ($scope.ProductRequest.mode == 1) {
                                    if ($scope.SizeGroupList.length == 1) {
                                        if ($scope.InputMode == 'SizeGroupColour') {
                                            $scope.setFocusallqty();
                                        }
                                    }
                                    else {
                                        $scope.setFocussizegroup();
                                    }
                                }
                                else if ($scope.ProductRequest.mode == 2) {
                                    $scope.setFocuscolour();
                                }
                                else {
                                    $scope.setFocusproduct();
                                }
                            }
                            if (typeof (callback) != 'undefined' && callback != null)
                                callback();
                        }, 10);
                    }
                    else {
                        $rootScope.Toaster.Error("Design not found!");
                        //if ($scope.ProductRequest.mode == 2) {
                        //    $scope.setFocussize();
                        //} else if ($scope.ProductRequest.mode == 1) {
                        //    $scope.setFocussize();
                        //} else if ($scope.ProductRequest.mode == 0) {
                        //    $scope.setFocussizerange();
                        //}
                    }
                });
                //restService.Get('MfgdesignService/SearchByName/' + encodeURIComponent($scope.ProductRequest.Details.designid.replace('/', '~')), true).then(function (result) {
                //});
            }
            else {
                if ($scope.ProductRequest.mode == 2) {
                    $scope.setFocuscolour();
                } else if ($scope.ProductRequest.mode == 1) {
                    $scope.setFocussize();
                } else if ($scope.ProductRequest.mode == 0) {
                    $scope.setFocussizerange();
                }
            }
        }

        $scope.DesignClass = 'col-md-6';
        $scope.IsUANBased = false;
        $scope.IsSampleBased = false;
        $scope.ModeChanged = function () {
            $scope.DesignClass = $scope.ProductRequest.mode == '3' ? 'col-md-12' : 'col-md-6'
            $scope.IsUANBased = $scope.ProductRequest.mode == '4';
            $scope.IsSampleBased = $scope.ProductRequest.mode == '5';
        }

        $scope.POMode = [{ id: 0, name: 'Size Range Based' },
        { id: 1, name: 'Size Set Based' },
        { id: 2, name: 'Single Size' },
        { id: 3, name: 'Design Based' },
        { id: 4, name: 'UAN Based' },
        { id: 5, name: 'Sample Based' },
        ];

        $scope.SourceTypes = [{ id: 1, name: 'Agent' },
        { id: 2, name: 'Salesmen' }];

        $scope.FreightList = [{ id: 1, name: 'To Pay' },
        { id: 2, name: 'Paid' }];

        $scope.POStatus = [{ id: 0, name: 'PENDING' }, { id: 1, name: 'APPROVED' }, { id: 2, name: 'REJECTED' },
        { id: 3, name: 'CANCELLED' }, { id: 4, name: 'COMPLETED' }];

        $scope.POSearchStatus = [{ id: 0, name: 'PENDING APPROVAL' }, { id: 1, name: 'APPROVED' },
        { id: 2, name: 'REJECTED' }, { id: 3, name: 'CANCELLED' }, { id: 4, name: 'COMPLETED' },
        { id: -9, name: 'ALL' }];

        $scope.POList = [];
        $scope.PendingPOList = [];
        $scope.Entity = {
            "id": 0,
            "expectedate": new Date(),
            "sodate": new Date(),
            "customerid": null,
            "sizegroup": '',
            "locationid": 0,
            "deliverylocationid": 0,
            "sono": "",
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
            "mode": 2,
            "Salesorderitems": [],
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
                "id": 0,
                "balance": "",
                "price": "",
                "designid": ""
            }
        };
        $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.SearchCondition = {
            'sono': null,
            'customerid': 0,
            'expectedate': null,
            'statusid': -9
        };

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.ErrorMessage = "";

        $scope.Clear = function () {
            $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.setFocusfirst();
            $scope.CheckPOSLayout(true);
            $scope.UpdateEditGrid();
        };
        $scope.DisableSave = false;
        $scope.Save = function () {
            if ($scope.ProductRequest.companyid == null || $scope.ProductRequest.companyid == 0) {
                $rootScope.Toaster.Error("Please select company!");
                return;
            }
            if ($scope.ProductRequest.customerid == null || $scope.ProductRequest.customerid == 0) {
                $rootScope.Toaster.Error("Please select customer!");
                return;
            }
            if ($scope.ProductRequest.sodate == null || $scope.ProductRequest.sodate == 0) {
                $rootScope.Toaster.Error("Please select so date!");
                return;
            }

            if ($scope.ProductRequest.Salesorderitems == undefined ||
                $scope.ProductRequest.Salesorderitems == null ||
                $scope.ProductRequest.Salesorderitems.length == 0) {
                $rootScope.Toaster.Error("Please add minimum one item!");
                return;
            }
            if ($scope.ProductRequest.agentid == undefined ||
                $scope.ProductRequest.agentid == null ||
                $scope.ProductRequest.agentid == '') {
                if ($scope.Agent && $scope.Agent != null &&
                    $scope.Agent.length > 0) {
                    $rootScope.Toaster.Error("Please select the agent!");
                    return;
                }
            }
            if ($scope.ProductRequest.salesmenid == undefined ||
                $scope.ProductRequest.salesmenid == null ||
                $scope.ProductRequest.salesmenid == '') {
                if ($scope.Employees && $scope.Employees != null &&
                    $scope.Employees.length > 0) {
                    $rootScope.Toaster.Error("Please select the salesmen!");
                    return;
                }
            }
            if ($scope.hasbooking) {
                if ($scope.ProductRequest.sobookno == undefined ||
                    $scope.ProductRequest.sobookno == null ||
                    $scope.ProductRequest.sobookno == '') {
                    $rootScope.Toaster.Error("Please enter booking no!");
                    return;
                }
                if ($scope.ProductRequest.sobookdate == undefined ||
                    $scope.ProductRequest.sobookdate == null ||
                    $scope.ProductRequest.sobookdate == '') {
                    $rootScope.Toaster.Error("Please enter booking date!");
                    return;
                }
                if ($scope.ProductRequest.sobookqty == undefined ||
                    $scope.ProductRequest.sobookqty == null ||
                    $scope.ProductRequest.sobookqty == '' ||
                    isNaN(parseFloat($scope.ProductRequest.sobookqty)) ||
                    parseFloat($scope.ProductRequest.sobookqty) <= 0) {
                    $rootScope.Toaster.Error("Please enter booking qty!");
                    return;
                }
            }

            if ($scope.DisableSave) return;
            $scope.DisableSave = true;
            var request = {
                "item": $scope.ProductRequest,
                "securitytoken": "String content"
            }

            $scope.Entity.sodate = $scope.ProductRequest.sodate;
            $scope.Entity.companyid = $scope.ProductRequest.companyid;
            $scope.Entity.deliverylocationid = $scope.ProductRequest.deliverylocationid;
            $scope.Entity.expectedate = $scope.ProductRequest.expectedate;
            $scope.Entity.tolerance = $scope.ProductRequest.tolerance;
            $scope.Entity.mode = $scope.ProductRequest.mode;
            $scope.Entity.agentid = $scope.ProductRequest.agentid;
            $scope.Entity.freight = $scope.ProductRequest.freight;
            $.each($scope.ProductRequest.Salesorderitems, function (k, v) {
                v.isactive = true;
            })
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SalesorderService/Save",
                data: srequest
            }).success(function (result) {
                $scope.DisableSave = false;
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with so No :" + result.sono);
                    $scope.ProductRequest.sono = result.sono;
                    $scope.ProductRequest.id = result.id;
                    $scope.ProductRequest.Salesorderitems = result.Salesorderitems;
                    $scope.UpdateEditGrid();
                }
                else {
                    $rootScope.Toaster.Error("Attempt Failed." + result.ErrorMessage);
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }
                $scope.CalculateOrderQty();
            }).error(function (result) {
                $scope.DisableSave = false;
            });
        }

        $scope.ChangeOrderStatus = function () {
            if ($scope.ProductRequest.id == null || $scope.ProductRequest.id == 0) {
                $rootScope.Toaster.Error("Please select sales order!");
                return;
            }

            var request = {
                "item": $scope.ProductRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalesorderService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Status updated successfully");

            }).error(function (result) {
            });
        }

        $scope.ShowSMApprovalDialog = function () {
            var ids = [];
            if ($scope.Navigation.save) {
                ids.push($scope.ProductRequest.id);
            }
            else {
                $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                    ids.push(v.id);
                });
            }

            var dialog = ngDialog.open({
                template: 'SMApproveDialog',
                data: [$scope, ids],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.termsandcondition = restService.Get('SalesorderService/GetSMApproveval/' + $scope.ngDialogData[1][0], true).then(function (results) {
                        if (results && results != null) {
                            $scope.pickedby = results.pickedby;
                            $scope.pickedon = results.pickedon;
                        }
                        else {
                            $scope.pickedon = new Date();
                        }
                    });
                    $scope.ChangeStock = function () {

                        //var date = new Date($scope.pickedon).setUTCHours(new Date().getHours(),
                        //    new Date().getMinutes());

                        $scope.closeThisDialog({
                            'ids': $scope.ngDialogData[1],
                            'pickedby': $scope.pickedby,
                            'pickedon': $scope.pickedon
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {

                        var webrequest = {
                            "item": data.value,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "SalesorderService/SetSMApproveval",
                            data: srequest
                        }).success(function (result) {
                            if (result) {
                                $rootScope.Toaster.Error('SM Approval successfully changed.');
                                if (!$scope.Navigation.save) {
                                    $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                                        v.smapprovedby = data.value.pickedby;
                                        v.smapprovedon = data.value.pickedon;
                                    });
                                    $scope.igrid.referesh();
                                }
                            }
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.ShowTermsDialog = function () {
            var ids = [];
            if ($scope.Navigation.save) {
                ids.push($scope.ProductRequest.id);
            }
            else {
                $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                    ids.push(v.id);
                });
            }

            var dialog = ngDialog.open({
                template: 'TermDialog',
                data: [$scope, ids],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.termsandcondition = restService.Get('SalesorderService/GetTermsandcondition/' + $scope.ngDialogData[1][0], true).then(function (results) {

                        if (results && results != null) {
                            $scope.pickedby = results.pickedby;
                            $scope.pickedon = results.pickedon;
                        }
                    });
                    $scope.ChangeStock = function () {

                        var date = new Date($scope.pickedon).setUTCHours(new Date().getHours(),
                            new Date().getMinutes());


                        $scope.closeThisDialog({
                            'ids': $scope.ngDialogData[1],
                            'pickedby': $scope.pickedby,
                            'pickedon': new Date(date)
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {

                        var webrequest = {
                            "item": data.value,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "SalesorderService/SetTermsandcondition",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Error('Pick Info successfully changed.');
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.ShowStatusDialog = function (IsCurrent) {

            var dialog = ngDialog.open({
                template: 'ChangeStatusDialog',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.ChangeStock = function () {
                        $scope.closeThisDialog({
                            'statusid': $scope.statusid,
                            'statuson': $scope.statuson,
                            'approvedby': $scope.approvedby
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {

                        var date = new Date(data.value.statuson).setUTCHours(new Date().getHours(),
                            new Date().getMinutes());

                        var request = {
                            'statusid': data.value.statusid,
                            'statuson': new Date(date),
                            'approvedby': data.value.approvedby,
                            ids: []
                        };

                        if (IsCurrent) {
                            request.ids.push($scope.ProductRequest.id);
                        }
                        else {
                            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                                request.ids.push(v.id);
                            });
                        }

                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "SalesorderService/ChangeStatus",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Error('Attributes successfully changed.');
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.CloneSO = function () {
            $scope.ProductRequest.id = 0;
            $scope.ProductRequest.sono = '';
            $.each($scope.ProductRequest.Salesorderitems, function (k, v) {
                if (v.id > 0) {
                    v.id = 0;
                }
                else if (v.jumpqty && v.jumpqty.length > 0) {
                    $.each(v.jumpqty, function (kk, vv) {
                        vv.id = 0;
                    });
                }
            });
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

        $scope.UpdateTax = function (soitem) {
            var product = $scope.ProductList.find(function (item) {
                return item.id == soitem.productid;
            });
            if (product && product.salestaxid > 0) {
                var stax = $scope.TaxList.find(function (item) {
                    return item.id == product.salestaxid;
                });
                if (stax && stax != null) {
                    if (stax.taxsplit == undefined || stax.taxsplit == null)
                        stax.taxsplit = { 'taxpercentage': stax.taxpercentage };
                    stax.taxsplit.findTax = $scope.findTax;
                    if (soitem.price == 0 && soitem.priceupto > 0)
                        soitem.price = soitem.priceupto;
                    if (soitem.price == 0 && soitem.mrp == 0)
                        soitem.price = soitem.mrp;

                    var taxitemvalue = stax.taxsplit.findTax(soitem.price);
                    soitem.taxpercentage = taxitemvalue.cgst + taxitemvalue.sgst;
                    soitem.taxid = product.salestaxid;
                    var qty = 0;
                    if (soitem.jumpqty && soitem.jumpqty != null) {
                        $.each(soitem.jumpqty, function (k, v) {
                            var oqty = parseFloat(v.oqty);
                            if (isNaN(oqty))
                                oqty = 0;
                            qty += oqty;
                        })
                    }
                    else {
                        var oqty = parseFloat(soitem.oqty);
                        if (isNaN(qty)) oqty = 0;
                        qty += oqty;
                    }
                    soitem.amount = soitem.price * qty;
                    if (soitem.amount > 0 && soitem.taxpercentage > 0) {
                        soitem.tax = (soitem.amount * soitem.taxpercentage / 100).round(2);
                    }
                    else {
                        soitem.tax = 0;
                    }
                }
            }
        }

        $scope.Designselected = function (selectall) {
            if (typeof (selectall) != 'undefined') {
                if (selectall) {
                    $.each($scope.AvailableColorList, function (k, v) {
                        v.selected = true;
                    })
                }
                else {
                    $.each($scope.AvailableColorList, function (k, v) {
                        v.selected = false;
                    })
                }
            }
            var selectedlist = [];
            $.each($scope.AvailableColorList, function (k, v) {
                if (v.selected)
                    selectedlist.push(v.code);
            })
            $scope.ProductRequest.Details.shades = selectedlist.join(',');
            $scope.SelectSizeGroup($scope.ProductRequest.Details.shades);
        }

        $scope.AddItem = function () {
            if ($scope.ProductRequest.companyid == null || $scope.ProductRequest.companyid == 0) {
                $scope.Toaster.Info("Please enter company");
                return false;

            }
            if ($scope.ProductRequest.deliverylocationid == null || $scope.ProductRequest.deliverylocationid == 0) {
                $scope.Toaster.Info("Please enter delivery location");
                return false;

            }
            if ($scope.ProductRequest.customerid == null || $scope.ProductRequest.customerid == 0) {
                $scope.Toaster.Info("Please enter order location");
                return false;
            }

            if (typeof ($scope.ProductRequest.Details) == 'undefined') {
                $scope.Toaster.Info("Please enter product");
                $scope.setFocusproduct();
                return false;
            }
            if ($scope.ProductRequest.Details.productid == null || $scope.ProductRequest.Details.productid == 0) {
                $scope.Toaster.Info("Please enter product");
                $scope.setFocusproduct();
                return false;
            }

            if ($scope.InputMode == 'SizeGroupColour' && $scope.ProductRequest.mode != 2) {
                if ($scope.ProductRequest.Details.groupqty == null || $scope.ProductRequest.Details.groupqty <= 0) {
                    $scope.Toaster.Info("Please enter order qty");
                    return false;
                }
            }
            else if ($scope.InputMode == 'SizeGroupColourShade' && $scope.ProductRequest.mode == 0) {
                if ($scope.ProductRequest.Details.groupqty == null || $scope.ProductRequest.Details.groupqty <= 0) {
                    $scope.Toaster.Info("Please enter order qty");
                    return false;
                }

                if ($scope.ProductRequest.Details.designid == null || $scope.ProductRequest.Details.designid == '') {
                    $scope.Toaster.Info("Please enter design id");
                    return false;
                }

                if ($scope.ProductRequest.Details.shades == null || $scope.ProductRequest.Details.shades == '') {
                    $scope.Toaster.Info("Please enter shade");
                    return false;
                }

                if ($scope.ProductRequest.Details.shadesize == null || $scope.ProductRequest.Details.shadesize == '') {
                    $scope.Toaster.Info("Please enter size");
                    return false;
                }
            }
            else if ($scope.ProductRequest.mode > 1) {
                if ($scope.ProductRequest.Details.oqty == null || $scope.ProductRequest.Details.oqty <= 0) {
                    $scope.Toaster.Info("Please enter order qty");
                    return false;
                }
            }

            if ($scope.ProductRequest.mode <= 1) {
                if (typeof ($scope.JumpList) == 'undefined' || $scope.JumpList == null || $scope.JumpList.length == 0) {
                    $scope.Toaster.Info("allocate size - qty");
                    $scope.SearchDesign(function () {
                        if (typeof ($scope.JumpList) != 'undefined' &&
                            $scope.JumpList != null && $scope.JumpList.length > 0) {
                            $scope.setFocusallqty();
                        }
                    });
                    return false;
                }
            }

            $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.ProductRequest)));
            $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
            $scope.ProductRequest.Details.printingname = $scope.SearchRef($scope.ProductRequest.Details.productid, $scope.ProductList, '');
            $scope.ProductRequest.Details.sizename = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.FullSizeList, '');
            $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
            $scope.ProductRequest.Details.materialname = $scope.SearchRef($scope.ProductRequest.Details.materialid, $scope.MaterialList, '');
            $scope.ProductRequest.Details.stylename = $scope.SearchRef($scope.ProductRequest.Details.styleid, $scope.StyleList, '');

            var scolour = $scope.FullColorList.find(function (item) {
                return item.colourid == $scope.ProductRequest.Details.colourid;
            });
            if (scolour && scolour != null)
                $scope.ProductRequest.Details.colourname = scolour.name;

            $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');
            $scope.ProductRequest.Details.patternname = $scope.SearchRef($scope.ProductRequest.Details.patternid, $scope.PatternList, '');

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

            if (typeof ($scope.ProductRequest.Details.pricerange) == 'undefined' ||
                $scope.ProductRequest.Details.pricerange == null) {
                $scope.ProductRequest.Details.pricerange = '';
            }
            //$scope.JumpList.length == 0 && 
            if ($scope.ProductRequest.mode == 2) {
                $scope.JumpList = [];
                $scope.ProductRequest.Details.tqty = $scope.ProductRequest.Details.oqty;
                var sizegroup = $scope.SizeGroupList.find(function (item) {
                    return $scope.ProductRequest.Details.colourshade == item.colourshade;
                });
                if (sizegroup) {
                    $scope.ProductRequest.Details.colourcode = sizegroup.colourcode;
                    $scope.ProductRequest.Details.designitemcode = sizegroup.designcode;
                    var size = sizegroup.list.find(function (item) {
                        return item.id == $scope.ProductRequest.Details.sizeid;
                    });
                    if (size == undefined || size == null) {
                        var availsize = [];
                        $.each(sizegroup.list, function (k, v) {
                            var size = $scope.FullSizeList.find(function (item) {
                                return item.id == v.id;
                            });
                            if (size)
                                availsize.push(size.name);
                        })
                        $rootScope.Toaster.Error('Please select the correct size/colour, Available - ' + availsize.join(','));
                        return;
                    }
                    var jump = (JSON.parse(JSON.stringify(size)));
                    delete jump.id;
                    jump.oqty = $scope.ProductRequest.Details.oqty;
                    jump.sizeid = $scope.ProductRequest.Details.sizeid;
                    $scope.JumpList.push(jump);
                }
                else {
                    $rootScope.Toaster.Error('Please select the correct size/colour');
                    return;
                }
            } else if ($scope.IsSampleBased) {
                $scope.ProductRequest.Details.size = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.FullSizeList, '');
                var sample = $scope.CustomerSampleList.find(function (item) {
                    return item.id == $scope.ProductRequest.Details.sampleid;
                });
                var design = $scope.CustomerSampleDesignList.find(function (item) {
                    return item.id == $scope.ProductRequest.Details.sampledesignid;
                });
                var size = $scope.SampleSizeList.find(function (item) {
                    return item.id == $scope.ProductRequest.Details.sizeid;
                });
                var colour = $scope.ColorList.find(function (item) {
                    return item.colourid == $scope.ProductRequest.Details.colourid;
                });
                $scope.ProductRequest.Details.uid = size.uid;
                $scope.ProductRequest.Details.designitemcode = design.code;
                $scope.ProductRequest.Details.designid = design.name;
                $scope.ProductRequest.Details.sizegroup = sample.samplename;
                $scope.ProductRequest.Details.mfgsampleordercode = sample.designno;
                $scope.ProductRequest.Details.colourcode = colour && colour != null ? colour.code : '';
                $scope.ProductRequest.Details.colourshade = colour && colour != null ? colour.code : '';
                $scope.ProductRequest.Details.mfgsampleorderid = $scope.ProductRequest.Details.sampleid;
                description = [];
                description.push(sample.samplename);
                description.push(design.name);
                description.push(size.name);
                $scope.ProductRequest.Details.description = sample.name + '/' + design.name + '/' + size.name;
            }

            var priceranges = $scope.ProductRequest.Details.pricerange.toString().split('-');
            var pricefrom = parseFloat(priceranges[0]);
            var priceto = priceranges.length > 1 ? parseFloat(priceranges[1]) : 0;
            var price = isNaN(pricefrom) ? 0 : pricefrom;

            const jumpqty = $scope.JumpList.filter(vv => (isNaN(parseFloat(vv.oqty)) ? 0 : parseFloat(vv.oqty)) > 0);

            item = {
                'productid': $scope.ProductRequest.Details.productid,
                'mfgsampleorderid': $scope.ProductRequest.Details.mfgsampleorderid,
                'mfgsampleordercode': $scope.ProductRequest.Details.mfgsampleordercode,
                'oqty': $scope.ProductRequest.Details.oqty,
                'cqty': 0,
                'rqty': 0,
                'isactive': true,
                'version': 1,
                'imageid': $scope.ProductRequest.Details.imageid,
                'fgboxid': $scope.ProductRequest.Details.boxid,
                'soboxid': $scope.ProductRequest.Details.boxid,
                'styleid': $scope.ProductRequest.Details.styleid,
                'materialid': $scope.ProductRequest.Details.materialid,
                'brandid': $scope.ProductRequest.Details.brandid,
                'typeid': $scope.ProductRequest.Details.typeid,
                'patternid': $scope.ProductRequest.Details.patternid,
                'addnlchargeid': $scope.ProductRequest.Details.addnlchargeid,
                'addnlcharge': $scope.ProductRequest.Details.addnlcharge,
                'colourid': $scope.ProductRequest.Details.colourid,
                'sizeid': $scope.ProductRequest.Details.sizeid,
                'materialname': $scope.ProductRequest.Details.materialname,
                'stylename': $scope.ProductRequest.Details.stylename,
                'printingname': $scope.ProductRequest.Details.printingname,
                'brandname': $scope.ProductRequest.Details.brandname,
                'sizename': $scope.ProductRequest.Details.sizename,
                'typename': $scope.ProductRequest.Details.typename,
                'patternname': $scope.ProductRequest.Details.patternname,
                'colourname': $scope.ProductRequest.Details.colourname,
                'colourcode': $scope.ProductRequest.Details.colourcode,
                'colourshade': $scope.ProductRequest.Details.colourshade,
                'designitemcode': $scope.ProductRequest.Details.designitemcode,
                'size': $scope.ProductRequest.Details.size,
                'sizegroup': $scope.ProductRequest.Details.sizegroup,
                'sizegroupid': $scope.ProductRequest.Details.sizegroupid,
                'pricerange': $scope.ProductRequest.Details.pricerange,
                'designid': $scope.ProductRequest.Details.designid,
                'tqty': $scope.ProductRequest.Details.tqty,
                'uid': $scope.ProductRequest.Details.uid,
                'jumpqty': jumpqty,
                'price': price,
                'mrp': $scope.ProductRequest.Details.mrp,
                'priceupto': isNaN(priceto) || priceto == 0 ? price : 0,
                'description': description.join('/'),
                'id': $scope.ProductRequest.Details.id
            };

            if (typeof (item.imageid) != 'undefined' && item.imageid != null &&
                item.imageid.length > 0)
                $scope.ProductRequest.ShowImage = true;

            if ($scope.ProductRequest.mode == 3)
                item.size = $scope.ProductRequest.Details.sizename;
            else
                $scope.ProductRequest.Details.sizename = item.size;
            if (!$scope.ValidateStock(item)) return;
            $scope.ProductRequest.Salesorderitems.splice(0, 0, item);
            $scope.ProductRequest.Details.oqty = 0;
            $scope.ProductRequest.Details.id = 0;
            $scope.ProductRequest.Details.imageid = '';
            $scope.JumpList = [];
            $scope.CalculateOrderQty();
            $scope.setFocusdesign();
            $scope.UpdateEditGrid();
            $scope.LocalContext.Post('$scope.ProductRequest', $scope.ProductRequest, false);
            $scope.LocalContext.Post('$scope.ProductRequest_Detail', {
                'stocklist': $scope.StockList,
                'jumplist': $scope.JumpList,
            }, false);

            if ($scope.IsSampleBased) {
                $scope.ProductRequest.Details.sizeid = null;
                $timeout(function () {
                    $scope.setFocussamplesinglesize();
                }, 10);
            }
            return true;
        };

        $scope.ValidateSizeRatio = function (item, stockList) {

            if ($scope.ProductRequest &&
                $scope.ProductRequest.Details &&
                $scope.ProductRequest.Details.iscoredesign)
                return true;

            var mfg_so_po = $scope.mfg_so_po;
            var mfg_so_wip = $scope.mfg_so_wip;
            var mfg_so_fg = $scope.mfg_so_fg;

            var errorList = [];
            if ($scope.mfg_sosizeratiorule &&
                $scope.mfg_sosizeratiorule != null &&
                $scope.mfg_sosizeratiorule == '90ML') {
                var orderlist = [];
                $.each(item.jumpqty, function (kk, vv) {
                    var oqty = parseFloat(vv.oqty);
                    if (isNaN(oqty)) oqty = 0;
                    vv.oqty = oqty;
                    var sqty = 0;
                    var stockqty = stockList.find(function (item) {
                        return item.sizeid == vv.sizeid;
                    });
                    if (stockqty && stockqty != null)
                        sqty = stockqty.sqty - (mfg_so_po ? stockqty.pqty : 0) + ((mfg_so_fg || mfg_so_wip) ? stockqty.wqty : 0);
                    orderlist.push({
                        'sizeid': vv.sizeid, 'sizename': vv.name, 'stockqty': sqty,
                        'orderqty': vv.oqty, 'closingstock': sqty - vv.oqty
                    });
                });
                errorList = $scope.ValidateSizeRatioRule90ML(orderlist);
            }
            else {

                $.each($scope.mfg_sosizeratioList, function (k, v) {
                    v.orderlist = [];
                });

                $.each(item.jumpqty, function (kk, vv) {
                    var oqty = parseFloat(vv.oqty);
                    if (isNaN(oqty)) oqty = 0;
                    vv.oqty = oqty;

                    for (var i = 0; i < $scope.mfg_sosizeratioList.length; i++) {
                        var sg = $scope.mfg_sosizeratioList[i];
                        if (sg.possizelist.indexOf(vv.name) >= 0) {
                            var stockqty = stockList.find(function (item) {
                                return item.sizeid == vv.sizeid;
                            });
                            var sqty = 0;
                            if (stockqty && stockqty != null)
                                sqty = stockqty.sqty - (mfg_so_po ? stockqty.pqty : 0) + ((mfg_so_fg || mfg_so_wip) ? stockqty.wqty : 0);
                            sg.orderlist.push({
                                'sizeid': vv.sizeid, 'sizename': vv.name, 'stockqty': sqty,
                                'orderqty': vv.oqty, 'closingstock': sqty - vv.oqty
                            });
                        }
                    }
                    //var sg = $scope.mfg_sosizeratioList.find(function (item) {
                    //    return (item.possizelist.indexOf(vv.name) >= 0);
                    //});
                    //if (sg && sg != null) {
                    //    var stockqty = stockList.find(function (item) {
                    //        return item.sizeid == vv.sizeid;
                    //    });
                    //    var sqty = 0;
                    //    if (stockqty && stockqty != null)
                    //        sqty = stockqty.sqty - (mfg_so_po ? stockqty.pqty : 0) + ((mfg_so_fg || mfg_so_wip) ? stockqty.wqty : 0);
                    //    sg.orderlist.push({
                    //        'sizeid': vv.sizeid, 'sizename': vv.name, 'stockqty': sqty,
                    //        'orderqty': vv.oqty, 'closingstock': sqty - vv.oqty
                    //    });
                    //}
                });

                $.each($scope.mfg_sosizeratioList, function (k, v) {
                    $.each(v.possizelist, function (kk, vv) {
                        var orderitem = v.orderlist.find(function (item) {
                            return item.sizename == vv;
                        });
                        if (orderitem == undefined || orderitem == null) {
                            var stockqty = stockList.find(function (item) {
                                return item.name == vv;
                            });
                            var sqty = 0;
                            if (stockqty && stockqty != null) {
                                sqty = stockqty.sqty - (mfg_so_po ? stockqty.pqty : 0) + ((mfg_so_fg || mfg_so_wip) ? stockqty.wqty : 0);
                                v.orderlist.push({
                                    'sizeid': stockqty.sizeid, 'sizename': stockqty.name, 'stockqty': sqty,
                                    'orderqty': 0, 'closingstock': sqty
                                });
                            }
                        }
                    });
                    if (v.type == 1) {
                        $scope.ValidateSizeRatioEven(v, stockList, errorList);
                    }
                    else if (v.type == 2) {
                        $scope.ValidateSizeRatioHierarchy(v, stockList, errorList);
                    }
                })
            }

            if (errorList.length > 0) {
                $rootScope.Toaster.Error(errorList.join(' <br/> '));
                return false;
            }
            return true;
        }

        $scope.ValidateSizeRatioRule90ML = function (orderList) {

            const sizes = ["S", "M", "L", "XL", "XXL", "3XL", "4XL", "5XL", "6XL"];
            let stock = {}, order = {}, balance = {};
            let errors = [];

            for (let s of sizes) {
                stock[s] = 0;
                order[s] = 0;
                balance[s] = 0;
            }

            for (let s of orderList) {
                stock[s.sizename] = s.stockqty;
                order[s.sizename] = s.orderqty;
                balance[s.sizename] = s.closingstock;
                if (balance[s.sizename] < 0) {
                    return [s.sizename + " Order exceeds stock"];
                }
            }

            const group1 = stock["L"] > 0 && stock["XL"] > 0 && stock["XXL"] > 0;
            const group3 = stock["3XL"] > 0 && stock["4XL"] > 0;
            const group2 = stock["XXL"] > 0 && stock["3XL"] > 0 && stock["4XL"] > 0;

            if (group1) {
                if (balance["L"] < balance["M"]) {
                    errors.push("L < M");
                }
                if (balance["L"] < balance["XL"]) {
                    errors.push("L < XL");
                }
                if (balance["M"] < balance["S"]) {
                    errors.push("M < S");
                }
                if (balance["XL"] < balance["XXL"]) {
                    errors.push("XL < XXL");
                }
                if (stock["L"] > 0 && balance["L"] === 0 && balance["XL"] > 0) {
                    errors.push("L cannot be zero while XL exists");
                }
            }

            if (group3) {
                if (balance["3XL"] < balance["4XL"]) {
                    errors.push("3XL < 4XL");
                }
            }

            if (!group1 && group2) {
                if (balance["XXL"] < balance["3XL"]) {
                    errors.push("XXL < 3XL");
                }
                if (balance["3XL"] < balance["4XL"]) {
                    errors.push("3XL < 4XL");
                }
            }

            if (stock["5XL"] > 0 || stock["6XL"] > 0) {
                if (balance["5XL"] < balance["6XL"]) {
                    errors.push("5XL < 6XL");
                }
            }

            return errors;
        }


        $scope.ValidateSizeRatioRule = function (orderList) {

            const sizes = ["S", "M", "L", "XL", "XXL", "3XL", "4XL", "5XL", "6XL"];
            let stock = {}, order = {}, balance = {};
            let errors = [];

            for (let s of orderList) {
                stock[s.sizename] = s.stockqty;
                order[s.sizename] = s.orderqty;
                balance[s.sizename] = s.closingstock;
                if (balance[s.sizename] < 0) {
                    errors.push(s.sizename+" Order exceeds stock");
                }
            }

            const active = (arr) => arr.filter(s => balance[s] > 0);

            let valid = true;
            

            // -------------------------
            // GROUP 1 (S M L XL XXL)
            // -------------------------
            const hasG1 = order["S"] || order["M"] || order["L"] || order["XL"] || order["XXL"];

            if (hasG1) {

                const g1 = active(["S", "M", "L", "XL", "XXL"]);

                if (g1.includes("L") && g1.includes("M") && balance["L"] < balance["M"]) {
                    valid = false; errors.push("L < M");
                }

                if (g1.includes("L") && g1.includes("XL") && balance["L"] < balance["XL"]) {
                    valid = false; errors.push("L < XL");
                }

                if (g1.includes("M") && g1.includes("S") && balance["M"] < balance["S"]) {
                    valid = false; errors.push("M < S");
                }

                if (g1.includes("XL") && g1.includes("XXL") && balance["XL"] < balance["XXL"]) {
                    valid = false; errors.push("XL < XXL");
                }

                // CONTROL SIZE PROTECTION
                if (stock["L"] > 0 && balance["L"] === 0 && balance["XL"] > 0) {
                    valid = false; errors.push("L cannot be zero while XL exists");
                }
            }

            // -------------------------
            // GROUP 2 (XXL 3XL 4XL)
            // Only if 3XL / 4XL in order
            // -------------------------
            if (order["3XL"] > 0 || order["4XL"] > 0) {

                const g2 = active(["XXL", "3XL", "4XL"]);

                if (g2.includes("XXL") && g2.includes("3XL") && balance["XXL"] < balance["3XL"]) {
                    valid = false; errors.push("XXL < 3XL");
                }

                if (g2.includes("3XL") && g2.includes("4XL") && balance["3XL"] < balance["4XL"]) {
                    valid = false; errors.push("3XL < 4XL");
                }
            }

            // -------------------------
            // GROUP 4 (5XL 6XL)
            // -------------------------
            if (order["5XL"] > 0 || order["6XL"] > 0) {

                const g4 = active(["5XL", "6XL"]);

                if (g4.includes("5XL") && g4.includes("6XL") && balance["5XL"] < balance["6XL"]) {
                    valid = false; errors.push("5XL < 6XL");
                }
            }
            return errors;
        }


        $scope.ValidateSizeRatioHierarchy = function (v, stocklist, errorList) {
            if (v.orderlist.length > 0) {

                v.sizegroups.sort(function (a, b) {
                    return a.index - b.index;
                });

                var sizegrouplist = [];
                $.each(v.sizegroups, function (k, vs) {
                    var sg = sizegrouplist.find(function (item) {
                        return item.index == vs.index;
                    });
                    if (sg == undefined || sg == null) {
                        sg = { 'index': vs.index, 'list': [] };
                        sizegrouplist.push(sg);
                    }

                    var stock = stocklist.find(function (stockitem) {
                        return stockitem.sizeid == vs.sizeid;
                    });

                    vs._orderqty = 0;
                    vs._closingstock = stock.sqty - ($scope.mfg_so_po ? stock.pqty : 0) + (($scope.mfg_so_fg || $scope.mfg_so_wip) ? stock.wqty : 0);
                    var orderqty = v.orderlist.find(function (item) {
                        return item.sizeid == vs.sizeid;
                    });
                    if (orderqty && orderqty != null) {
                        vs._orderqty = orderqty.orderqty;
                    }
                    sg.list.push(vs);
                });


                $.each(sizegrouplist, function (k, v) {
                    if (k == 0) return;
                    sizegrouplist[k - 1].list.find(function (parentitem) {
                        var pitem = parentitem;
                        var expqty = parentitem._closingstock - parentitem._orderqty;
                        for (var index = 0; index < v.list.length; index++) {
                            var currentitem = v.list[index];
                            var parentexpqty = currentitem._closingstock - currentitem._orderqty;
                            if (parentitem._closingstock > 0 && expqty < parentexpqty) {
                                errorList.push('Size Ratio Hierarchy : ' + pitem.sizename + '(' + expqty + ') should be heigher than ' + currentitem.sizename + '(' + parentexpqty + ')');
                            }
                        }
                    })
                })


                //if (stockqty && stockqty != null) {
                //    closingstock = stockqty.sqty - ($scope.mfg_so_po ? stockqty.pqty : 0) + (($scope.mfg_so_fg || $scope.mfg_so_wip) ? stockqty.wqty : 0);
                //}
                //$.each(v.orderlist, function (kk, vv) {
                //    if (vv.orderqty == 0) return;
                //    var sizegroupconfig = v.sizegroups.find(function (item) {
                //        return item.sizeid == vv.sizeid;
                //    });

                //    $.each(v.sizegroups, function (sk, sv) {
                //        if (sv.sizeid == vv.sizeid) return;
                //        var closingstock = 0;
                //        var sizeordered = v.orderlist.find(function (item) {
                //            return item.sizeid == sv.sizeid;
                //        });
                //        if (sizeordered == undefined || sizeordered == null) {
                //            var stockqty = stocklist.find(function (item) {
                //                return item.sizeid == sv.sizeid;
                //            });
                //            if (stockqty && stockqty != null) {
                //                closingstock = stockqty.sqty - ($scope.mfg_so_po ? stockqty.pqty : 0) + (($scope.mfg_so_fg || $scope.mfg_so_wip) ? stockqty.wqty : 0);
                //            }
                //        }
                //        else {
                //            closingstock = sizeordered.closingstock;
                //        }

                //        if (closingstock > 0) {
                //            if (sizegroupconfig.index < sv.index) {
                //                if (vv.closingstock < closingstock) {

                //                    errorList.push('After reduced the stock ' + vv.stockqty
                //                        + ' with order qty ' + vv.orderqty + ' of "'
                //                        + vv.sizename + '" may break the size ratio of "' + sv.sizename + '" - ' + closingstock + ', cant allow the order');

                //                }
                //            }
                //            else if (sizegroupconfig.index > sv.index) {
                //                if (vv.closingstock > closingstock) {

                //                    errorList.push('After reduced the stock ' + vv.stockqty
                //                        + ' with order qty ' + vv.orderqty + ' of "'
                //                        + vv.sizename + '" may break the size ratio of "' + sv.sizename + '" - ' + closingstock + ', cant allow the order');
                //                }
                //            }
                //        }
                //    })
                //})


            }
        }

        $scope.ValidateSizeRatioEven = function (v, errorList) {
            var tstock = 0;
            var tcstock = 0;
            var removeitems = [];
            $.each(v.orderlist, function (kk, vv) {
                tstock += v.stockqty;
                tcstock += v.closingstock;
                if (vv.closingstock <= 0 &&
                    vv.stockqty <= 0) {
                    removeitems.push(vv);
                }
            });

            $.each(removeitems, function (kk, vv) {
                v.orderlist.splice(v.orderlist.indexOf(vv), 1);
            });
            if (v.orderlist.length > 0) {
                v.orderlist.sort(function (a, b) {
                    if (a.stockqty == b.stockqty)
                        return a.sizeid - b.sizeid;
                    return (a.stockqty - b.stockqty);
                });
                var firstStock = v.orderlist[0];
                v.orderlist.sort(function (a, b) {
                    if (a.closingstock == b.closingstock)
                        return a.sizeid - b.sizeid;
                    return (a.closingstock - b.closingstock);
                })
                var firstClosing = v.orderlist[0];
                if (firstStock.sizeid != firstClosing.sizeid &&
                    firstStock.closingstock != firstClosing.closingstock) {
                    errorList.push('After reduced the stock ' + firstClosing.stockqty
                        + ' with order qty ' + firstClosing.orderqty + ' of "'
                        + firstClosing.sizename + '" may break the size ratio of "' + firstStock.sizename + '", cant allow the order');
                }
            }
        }

        $scope.ValidateStock = function (item) {
            $scope.UpdateTax(item);
            if ($scope.mfg_sosizeratio &&
                !$scope.ValidateSizeRatio(item, $scope.StockList)) {
                return false;
            }
            if (!$scope.mfg_sovalidate) return true;
            var errorList = [];
            var stockList = $scope.StockList;
            var mfg_so_po = $scope.mfg_so_po;
            var mfg_so_wip = $scope.mfg_so_wip;
            var mfg_so_fg = $scope.mfg_so_fg;
            $.each(item.jumpqty, function (k, v) {
                if (!$scope.mfg_sosizeratio) {
                    var oqty = parseFloat(v.oqty);
                    if (isNaN(oqty)) oqty = 0;
                    v.oqty = oqty;
                }
                var stockqty = stockList.find(function (vv) {
                    return v.sizeid == vv.sizeid;
                });
                if (stockqty && stockqty != null) {
                    var sqty = stockqty.sqty - (mfg_so_po ? stockqty.pqty : 0) + ((mfg_so_fg || mfg_so_wip) ? stockqty.wqty : 0);
                    if (sqty < v.oqty) {
                        errorList.push(v.name + ' - ' + (v.oqty) + '(' + sqty + ')');
                    }
                } else if (v.oqty > 0) {
                    errorList.push(v.name + ' - ' + (v.oqty) + '(0)');
                }
            });
            if (errorList.length > 0)
                $rootScope.Toaster.Error("Stock not available - " + errorList.join(' '));
            return errorList.length == 0;
        }

        $scope.mfg_solocation = 0;
        $scope.StockLocationChanged = function () {
            if ($scope.mfg_solocation != $scope.ProductRequest.deliverylocationid) {
                $scope.mfg_solocation = $scope.ProductRequest.deliverylocationid;
                $scope.mfg_sovalidate = false;
                $scope.mfg_so_wip = false;
                $scope.mfg_so_po = false;
                $scope.mfg_so_fg = false;
                $scope.mfg_sosizeratio = false;
                var srequest = JSON.stringify({
                    "Item": { 'locationid': $scope.mfg_solocation, 'key': 'APPLICATIONSETTINGS' },
                    "securitytoken": "String content"
                });

                $http({
                    method: 'POST',
                    url: "ConfigurationService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result && result != null && result.length == 1) {
                        $scope.mfg_sovalidate = result[0].appsettings.mfg_sovalidate;
                        $scope.mfg_so_wip = result[0].appsettings.mfg_so_wip;
                        $scope.mfg_so_po = result[0].appsettings.mfg_so_po;
                        $scope.mfg_so_fg = result[0].appsettings.mfg_so_fg;
                        $scope.mfg_sosizeratio = result[0].appsettings.mfg_sosizeratio;
                        $scope.mfg_sosizeratiorule = result[0].appsettings.mfg_sosizeratiorule;

                        //$scope.mfg_sosizeratio = true;
                        //$scope.mfg_sosizeratiorule = '90ML';

                        if ($scope.egrid != null) {
                            $scope.ConfigEditor();
                            if ($scope.ProductRequest && $scope.ProductRequest.Salesorderitems &&
                                $scope.ProductRequest.Salesorderitems != null && $scope.ProductRequest.Salesorderitems.length > 0) {
                                $scope.UpdateEditGrid();
                                $scope.CalculateOrderQty();
                            }
                        }
                    }
                });
            }
        }

        $scope.CalculateOrderQty = function () {
            $scope.orderqty = 0;
            $.each($scope.ProductRequest.Salesorderitems, function (k, v) {
                if ($rootScope.GetNumber(v.oqty) == 'undefined' || $rootScope.GetNumber(v.oqty) == 0)
                    v.oqty = 0;
                if (v.jumpqty && v.jumpqty != null && v.jumpqty.length > 0) {
                    $.each(v.jumpqty, function (kk, vv) {
                        var oqty = $rootScope.GetNumber(vv.oqty);
                        if (oqty > 0)
                            $scope.orderqty += oqty;
                    });
                }
                else {
                    $scope.orderqty += $rootScope.GetNumber(v.oqty);
                }
            });
        };

        $scope.ColourChanged = function () {
            $scope.ProductRequest.showcolourchart = false;

            var selectedcolour = $scope.ColorList.find(function (item) {
                return item.displaycode == $scope.ProductRequest.Details.colourshade;
            })

            if (selectedcolour &&
                selectedcolour != null) {
                $scope.ProductRequest.Details.colourid = selectedcolour.colourid;
                $scope.ProductRequest.Details.colourcode = selectedcolour.code;
            }

            if ($scope.ProductRequest.Details.colourid &&
                $scope.ProductRequest.Details.colourid != null &&
                $scope.ProductRequest.Details.colourid == -1) {
                $scope.ProductRequest.showcolourchart = true;
                $scope.ProductRequest.ColourCharts = [];
                $.each($scope.ColorList, function (k, v) {
                    if (v.id > 0) {
                        $scope.ProductRequest.ColourCharts.push({
                            'id': v.id,
                            'name': v.name,
                            'qty': ''
                        });
                    }
                })
            }

        }

        $scope.ManageItem = function (type, item) {
            if (type == 'Edit') {
                $scope.ProductRequest.Details = $scope.ProductRequest.Details || {};
                $scope.ProductRequest.Details.productid = item.productid;
                $scope.ProductRequest.Details.oqty = item.oqty;
                $scope.ProductRequest.Details.id = item.id;
                $scope.ProductRequest.Details.imageid = item.imageid;
                $scope.ProductRequest.Details.styleid = item.styleid;
                $scope.ProductRequest.Details.materialid = item.materialid;
                $scope.ProductRequest.Details.brandid = item.brandid;
                $scope.ProductRequest.Details.typeid = item.typeid;
                $scope.ProductRequest.Details.patternid = item.patternid;
                $scope.ProductRequest.Details.colourid = item.colourid;
                $scope.ProductRequest.Details.sizeid = item.sizeid;
                $scope.ProductRequest.Details.materialname = item.materialname;
                $scope.ProductRequest.Details.stylename = item.stylename;
                $scope.ProductRequest.Details.printingname = item.printingname;
                $scope.ProductRequest.Details.brandname = item.brandname;
                $scope.ProductRequest.Details.sizename = item.sizename;
                $scope.ProductRequest.Details.sizegroupid = item.sizegroup;
                $scope.ProductRequest.Details.sizegroup = item.sizegroup;
                $scope.ProductRequest.Details.typename = item.typename;
                $scope.ProductRequest.Details.patternname = item.patternname;
                $scope.ProductRequest.Details.colourname = item.colourname;
                $scope.ProductRequest.Details.size = item.size;
                $scope.ProductRequest.Details.colourcode = item.colourcode;
                $scope.ProductRequest.Details.colourshade = item.colourshade;
                $scope.ProductRequest.Details.pricerange = item.pricerange;
                $scope.ProductRequest.Details.designid = item.designid;
                $scope.ProductRequest.Details.designuid = item.designuid;
                $scope.ProductRequest.Details.groupqty = item.oqty;
                $scope.ProductRequest.Details.setqty = item.setqty;
                $scope.ProductRequest.Details.addnlcharge = item.addnlcharge;
                $scope.ProductRequest.Details.addnlchargeid = item.addnlchargeid;
                $scope.ProductRequest.Details.mrp = item.mrp;
                /*if ($scope.InputMode == 'SizeGroupColourShade' &&
                    $scope.mfg_sosizeratio) {
                    $scope.ProductRequest.mode = 1;
                }*/
                $scope.ProductRequest.Salesorderitems.splice($scope.ProductRequest.Salesorderitems.indexOf(item), 1);
                $scope.UpdateEditGrid();
                if ($scope.IsSampleBased) {
                    $scope.ProductRequest.Details.sampleid = item.mfgsampleorderid;
                    $scope.SampleChanged();
                    var designitem = $scope.CustomerSampleDesignList.find(function (sitem) {
                        return sitem.code == item.designitemcode;
                    });
                    if (designitem && designitem != null)
                        $scope.ProductRequest.Details.sampledesignid = designitem.id;
                    $scope.SampleDesignChanged();
                    $scope.ProductRequest.Details.sizeid = item.sizeid;
                    return;
                }

                var stocklocations = $scope.GetStockLocations();
                if (stocklocations.length == 0) return;

                var request = {
                    "Item": {
                        'stocklocations': stocklocations,
                        'name': item.designitemcode.toUpperCase()
                    },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "MfgdesignService/SearchByName",
                    data: srequest
                }).success(function (result) {
                    $scope.ProductRequest.Details.boxid = result.boxid;
                    $scope.ProductRequest.Details.iscoredesign = result.iscoredesign;
                    //$scope.ProductRequest.mode = 1;
                    $scope.SizeGroupList = result.sizegroup;
                    $scope.AvailableColorList = [];
                    if ($scope.InputMode == 'SizeGroupColourShade' && $scope.mfg_sosizeratio)
                        $scope.ProductRequest.Details.colourshade = null;
                    if (result.colourlist != null && result.colourlist.length > 0) {
                        $scope.ColorList = result.colourlist;
                        $scope.AvailableColorList = result.colourlist;
                    }
                    else {
                        $scope.ColorList = $scope.FullColorList;
                    }

                    $timeout(function () {
                        if (result.sizegroup != null &&
                            result.sizegroup.length == 1 &&
                            (result.sizegroup[0].name == null || result.sizegroup[0].name == '')) {
                            $scope.SizeList = result.sizegroup[0].list;
                            $scope.ProductRequest.mode = 2;
                        }
                        else if (result.sizegroup == null ||
                            result.sizegroup.length == 0) {
                            $scope.ProductRequest.mode = 2;
                            $scope.SizeList = $scope.FullSizeList;
                            $scope.ProductRequest.Details.sizeid = result.sizeid;
                        }
                        if ($scope.InputMode == 'SizeGroupColourShade') {
                            //$scope.ProductRequest.mode = $scope.mfg_sosizeratio ? 1 : 0;
                            $scope.ProductRequest.Details.shadesize = item.size;
                            $scope.ProductRequest.Details.shades = item.colourcode;
                            $scope.ProductRequest.Details.colourshade = item.colourcode;
                            var colouritem = $scope.ColorList.find(function (citem) {
                                return citem.designcode == item.designitemcode
                            });
                            if (colouritem) {
                                $scope.ProductRequest.Details.shades = colouritem.displaycode;
                                $scope.SelectSizeGroup($scope.ProductRequest.Details.shades);
                            }
                        }
                        if ($scope.SizeGroupList.length > 0) {
                            var colourid = $scope.ProductRequest.Details.colourid;
                            var sizegroup = $scope.ProductRequest.Details.sizegroup;
                            $scope.ProductRequest.Details.sizegroupid = null;
                            $scope.ProductRequest.Details.colourid = null;
                            $timeout(function () {
                                $scope.ProductRequest.Details.sizegroupid = sizegroup;
                                $scope.ProductRequest.Details.colourid = colourid;

                            }, 10);
                        }
                        $scope.ProductRequest.Details.typeid = result.typeid;
                        $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');
                    }, 20);

                });

                //restService.Get('MfgdesignService/SearchByName/' + item.designid, true).then(function (result) {

                //});
                $scope.JumpList = item.jumpqty;
                $scope.CalculateOrderQty();
                $scope.ProductRequest.Details.tqty = 0;
                $.each($scope.JumpList, function (k, v) {
                    $scope.ProductRequest.Details.tqty += $rootScope.GetNumber(v.oqty);
                });
                var lengthysize = $scope.JumpList.find(function (sitem) {
                    return sitem.name != null &&
                        sitem.name.length > 5;
                });
                $scope.JumpListStyle = lengthysize && lengthysize != null ? 'col-md-6' : 'col-md-2';
            } else if (type == 'Delete') {
                if (item.rqty > 0) {
                    $rootScope.Toaster.Error("Item Can't be deleted!! Item already received from the supplier.")
                    return;
                }
                else {
                    $scope.ProductRequest.Salesorderitems.splice($scope.ProductRequest.Salesorderitems.indexOf(item), 1);
                    $scope.UpdateEditGrid();
                }
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
                    url: "SalesorderitemsService/Update",
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
                $scope.ShowSearch = false;
                $scope.Navigation.SetMode('EDIT');
                $scope.StockLocationChanged();
                $scope.CheckPOSLayout(false);
                $scope.CustomerChanged();
                var request = {
                    "Item": {
                        'Salesorderid': item.id,
                        'groupbysizegroup': !$scope.IsUANBased && !$scope.IsSampleBased
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "SalesorderitemsService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length > 0) {
                        $scope.ProductRequest.Salesorderitems = result;
                        $scope.ConfigEditor();
                        $scope.UpdateEditGrid();
                    }
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
                                    url: "SalesorderService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $rootScope.Toaster.Info("Successfully deleted");
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


        $scope.ChargesChanged = function (selecteditem) {
            selecteditem.addnlcharge = 0;
            if (selecteditem.addnlchargeid && selecteditem.addnlchargeid > 0) {
                var charge = $scope.AdditionalCharges.find(function (item) {
                    return item.id == selecteditem.addnlchargeid;
                });
                if (charge && charge != null) {
                    selecteditem.addnlcharge = charge.value;

                }
            }
        }

        $scope.groupqtyChanged = function () {
            var qty = parseFloat($scope.ProductRequest.Details.groupqty);
            if (isNaN(qty)) return;

            $scope.ProductRequest.Details.oqty = qty;
            $.each($scope.JumpList, function (k, v) {
                v.oqty = qty;
            });
            $scope.ProductRequest.Details.tqty = 0;
            $.each($scope.JumpList, function (k, v) {
                $scope.ProductRequest.Details.tqty += $rootScope.GetNumber(v.oqty);
            });
        }

        $scope.qtyChanged = function () {
            $scope.ProductRequest.Details.tqty = 0;
            $.each($scope.JumpList, function (k, v) {
                $scope.ProductRequest.Details.tqty += $rootScope.GetNumber(v.oqty);
            });
            $scope.ProductRequest.Details.oqty = parseInt($scope.ProductRequest.Details.tqty / $scope.JumpList.length);
            if (isNaN($scope.ProductRequest.Details.oqty)) $scope.ProductRequest.Details.oqty = 1;
            $scope.ProductRequest.Details.groupqty = $scope.ProductRequest.Details.oqty;
        }

        $scope.SetSizeGroup = function () {
            //$scope.ProductRequest.Details.fshade = '';
            if (typeof ($scope.ProductRequest.Details.sizegroupid) != 'undefined' &&
                $scope.ProductRequest.Details.sizegroupid != null) {

                var sizegroup = $scope.SizeGroupList.find(function (item) {
                    return item.id == $scope.ProductRequest.Details.sizegroupid;
                });

                //$scope.ProductRequest.Details.fshade = $scope.AvailableColorList.find(function (item) {
                //    return item.designcode == sizegroup.designcode;
                //}).code;

                $scope.ProductRequest.Details.sizegroup = sizegroup.name;
                $scope.ProductRequest.Details.size = sizegroup.name;
                $scope.ProductRequest.Details.oqty = $scope.ProductRequest.Details.groupqty;
                $scope.ProductRequest.Details.designitemcode = sizegroup.designcode;
                if (sizegroup.list.length > 0 && sizegroup.list[0].colourid > 0) {
                    $scope.ProductRequest.Details.colourid = sizegroup.list[0].colourid;
                    $scope.ProductRequest.Details.price = sizegroup.list[0].wsrate;
                    $scope.ProductRequest.Details.mrp = sizegroup.list[0].mrp;
                    var selectedcolour = $scope.AvailableColorList.find(function (item) {
                        return item.colourid == $scope.ProductRequest.Details.colourid
                    });
                    if (selectedcolour != null) {
                        $scope.ProductRequest.Details.colourcode = selectedcolour.code;
                        $scope.ProductRequest.Details.colourshade = selectedcolour.displaycode;
                    }
                }
                var idList = [];
                $.each(sizegroup.list, function (k, v) {
                    idList.push({
                        'sizeid': v.id,
                        'name': v.name,
                        'uid': v.uid,
                        'oqty': $scope.ProductRequest.Details.groupqty,
                        'wqty': v.wip,
                        'price': v.wsrate,
                        'mrp': v.mrp,
                        'pqty': v.order,
                        'sqty': v.stock,
                        'colourid': v.colourid,
                        'index': $scope.SizeIndexList == null ? 100 : $scope.SizeIndexList.indexOf(v.name)
                    });
                });


                //$.each(sizegroup.list, function (k, v) {
                //    idList.push({
                //        'sizeid': v.id,
                //        'name': v.name,
                //        'uid': v.uid,
                //        'oqty': $scope.ProductRequest.Details.groupqty,
                //        'wqty': v.wip,
                //        'pqty': v.order,
                //        'sqty': v.stock,
                //        'colourid': v.colourid,
                //        'index': k
                //    });
                //});

                idList.sort(function (a, b) {
                    return a.index - b.index;
                });

                $scope.JumpList = idList;
                $scope.StockList = idList;
                var lengthysize = $scope.JumpList.find(function (sitem) {
                    return sitem.name != null &&
                        sitem.name.length > 5;
                });
                $scope.JumpListStyle = lengthysize && lengthysize != null ? 'col-md-6' : 'col-md-2';
            }
        }

        $scope.CheckSize = function () {

            if (typeof ($scope.ProductRequest.Details.size) != 'undefined' &&
                $scope.ProductRequest.Details.size != null) {
                $scope.JumpList = $scope.GetJUMPInformation($scope.ProductRequest.Details.size);
                var lengthysize = $scope.JumpList.find(function (sitem) {
                    return sitem.name != null &&
                        sitem.name.length > 5;
                });
                $scope.JumpListStyle = lengthysize && lengthysize != null ? 'col-md-6' : 'col-md-2';
                //$scope.setFocusallqty();
            }

            //$rootScope.Prompt('Jump', 'Jump (start-increment-end)', function (count) {
            //});
        }

        $scope.GetJUMPInformation = function (count) {

            if (count.length > 0) {

                var sizeidList = count.split(',');
                var sizeList = sizeidList[0].split('-');
                if (sizeList.length == 2 && /^[a-zA-Z2-6]*$/.test(sizeList[0])) {

                    var sizeIndex = '3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL'.split(',');
                    var startsize = sizeIndex.indexOf(sizeList[0]);
                    var endsize = sizeIndex.indexOf(sizeList[1]);


                    var xxl = $scope.FullSizeList.find(function (item) {
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
                } else if (sizeidList.length >= 1) {
                    if (sizeList.length == 2) {
                        if ($scope.system_possizelist != null &&
                            $scope.system_possizelist != '') {
                            var sizeIndex = $scope.system_possizelist.split(',');
                            var startsize = sizeIndex.indexOf(sizeList[0]);
                            var endsize = sizeIndex.indexOf(sizeList[1]);
                            if (startsize >= 0 && endsize >= 0) {
                                sizeList = [];
                                for (var index = startsize; index <= endsize; index++) {
                                    sizeList.push(sizeIndex[index]);
                                }
                            }
                        } else if ($scope.FullSizeNumbers.length > 0) {
                            var sizeIndex = $scope.FullSizeNumbers;
                            var startsize = sizeIndex.indexOf(sizeList[0]);
                            var endsize = sizeIndex.indexOf(sizeList[1]);
                            if (startsize >= 0 && endsize >= 0) {
                                sizeList = [];
                                for (var index = startsize; index <= endsize; index++) {
                                    sizeList.push(sizeIndex[index].toString());
                                }
                            }
                        }
                    }
                    else {
                        $.each(sizeidList, function (k, v) {
                            sizeList.push(v);
                        })
                    }
                }

                var idList = [];
                $.each($scope.SizeList, function (k, v) {
                    $.each(sizeList, function (kk, vv) {
                        if (vv == v.name) {
                            if (idList.find(function (item) {
                                return item.name == v.name;
                            }) == null) {
                                idList.push({
                                    'sizeid': v.id,
                                    'name': v.name, 'qty': 0, 'index': kk
                                });
                            }
                        }
                    });
                });

                idList.sort(function (a, b) {
                    return a.index - b.index;
                });

                return idList;
            }
            return []
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
            if ($scope.IsSampleBased) return;
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
        $scope.Search = function () {

            var hascondition = false;
            if ($scope.SearchCondition.sono && $scope.SearchCondition.sono != null && $scope.SearchCondition.sono != '')
                hascondition = true;

            if ($scope.SearchCondition.fromdate && $scope.SearchCondition.fromdate != null && $scope.SearchCondition.fromdate != '')
                hascondition = true;

            if ($scope.SearchCondition.expectedatefrom && $scope.SearchCondition.expectedatefrom != null && $scope.SearchCondition.expectedatefrom != '')
                hascondition = true;

            if (!hascondition) {
                $rootScope.Toaster.Info("Minimum one condition required(SO NO,REQUESTED,DUE ON).");
                return;
            }
            var request = {
                "Item": {
                    'customerid': $scope.SearchCondition.customerid,
                    'sono': $scope.SearchCondition.sono,
                    'expectedatefrom': $scope.SearchCondition.expectedatefrom,
                    'expectedateto': $scope.SearchCondition.expectedateto,
                    'showsize': $scope.SearchCondition.showsize,
                    'fromdate': $scope.SearchCondition.fromdate,
                    'todate': $scope.SearchCondition.todate,
                    'deliverylocationid': $scope.SearchCondition.deliverylocationid,
                    'locationid': $scope.SearchCondition.deliverylocationid,
                    'statusid': $scope.SearchCondition.statusid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SalesorderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ConfigGrid(result);
            });
        };

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
                    $scope.printingname = '';
                    $scope.Create = function () {

                        var coreScope = $scope.ngDialogData.scope;
                        var item = coreScope.ProductRequest.Details;
                        item.brandname = coreScope.SearchRef(item.brandid, coreScope.BrandName, '');
                        item.productname = coreScope.SearchRef(item.productid, coreScope.Products, '');
                        item.patternname = coreScope.SearchRef(item.patternid, coreScope.Patterns, '');
                        item.materialname = coreScope.SearchRef(item.materialid, coreScope.Material, '');
                        item.colourname = coreScope.SearchRef(item.colourid, coreScope.Colors, '');
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
            restService.Get('SalesorderService/Publish/' + $scope.UserInfo.OrganizationID + '/' + $scope.ProductRequest.id, true).then(function (results) {
                if (results != null && results != 'undefined' && results == 'true')
                    $rootScope.Toaster.Info("Sales Order Sent Successfully.")
                else
                    $rootScope.Toaster.Info("Error in Sending Sales Order.")
            });
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == 'posummary')
                subtitle = 'Sales ORDER - SUMMARY REPORT';
            else if (tableid == 'podetails')
                subtitle = 'Sales ORDER - ITEM DETAILS REPORT';
            //subtitle = 'Sales ORDER - SUMMARY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company) + " ) ";

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

        $scope.Print = function () {
            $rootScope.Download('SalesorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.ProductRequest.id, 'salesorder.pdf', 'application/pdf');
            return;
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "po.pdf";
                link.href = 'SalesorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.ProductRequest.id;
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
                url: "SalesorderService/SearchPOSummary",
                data: srequest
            }).success(function (result) {
                $scope.POList = result;
                $scope.totalrecords = result.length;
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                }

                $scope.POConfig();
            }).error(function (result) {
            });
        }

        $scope.ShowOrderItems = function (action, item) {
            $scope.POItemsList = [];
            if (action == "VIEW") {

                request = {
                    "item": {
                        "Salesorderid": item.sono
                    },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SalesorderitemsService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.POItemsList = result;
                    $scope.POOrderDetailConfig();
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
                "item": $scope.ProductRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalesorderService/SearchPOSummary",
                data: srequest
            }).success(function (result) {
                $scope.POOrderList = result;
                $scope.ShowOrderItems('VIEW', $scope.POOrderList[0]);
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
                url: "SalesorderService/SearchPendingPO",
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


        $scope.POConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sono', 'Order No', 80);
            igrid.AddFormatedColumn('sodate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('customerid', 'Supplier Name', 150, $scope.Suppliers);
            igrid.AddReferenceColumn('locationid', 'Order Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('deliverylocationid', 'Delivery Location', 100, $scope.LocationList);
            igrid.AddFormatedColumn('expectedate', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('tolerance', 'Qty Tolerance %', 100);
            igrid.AddReferenceColumn('agentid', 'Agent Name', 100, $scope.Agent);
            igrid.AddReferenceColumn('status', 'PO Status', 100, $scope.POStatus);

            igrid.AddFormatedColumn('ordered', 'Ordered Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('received', 'Received Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cancelled', 'Cancelled Qty', 100, 'N2', 2).total = true;


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

        $scope.POOrderDetailConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('griddetailContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sono', 'so NUMER', 50);
            igrid.AddReferenceColumn('customerid', 'SUPPLIER NAME', 120, $scope.Suppliers);
            igrid.AddSimpleColumn('productname', 'PRODUCT NAME', 100);
            igrid.AddSimpleColumn('brandname', 'BRAND', 100);
            igrid.AddSimpleColumn('designid', 'DESIGN NO', 100);
            igrid.AddSimpleColumn('colourname', 'COLOUR');
            igrid.AddSimpleColumn('sizename', 'SIZE', 100);
            igrid.AddSimpleColumn('stylename', 'STYLE', 100);
            igrid.AddSimpleColumn('materialname', 'MATERIAL', 100);
            igrid.AddSimpleColumn('patternname', 'PATTERN', 100);
            igrid.AddSimpleColumn('description', 'DESCRIPTION', 100);
            igrid.AddSimpleColumn('pricerange', 'PRICE RANGE');
            igrid.AddSimpleColumn('size', 'SIZE RANGE', 100);
            igrid.AddFormatedColumn('oqty', 'ORDER QTY', 100, 'N2', 2);
            igrid.AddFormatedColumn('rqty', 'RECEIVED QTY', 100, 'N2', 2);
            igrid.AddFormatedColumn('cqty', 'CANCELLED QTY', 100, 'N2', 2);

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

            igrid.AddSimpleColumn('sono', 'Order No', 80);
            igrid.AddFormatedColumn('sodate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expectedate', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('customerid', 'Supplier Name', 150, $scope.Suppliers);
            igrid.AddReferenceColumn('locationid', 'Order Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('deliverylocationid', 'Delivery Location', 100, $scope.LocationList);
            igrid.AddSimpleColumn('tolerance', 'Qty Tolerance %', 100);
            igrid.AddReferenceColumn('agentid', 'Agent Name', 100, $scope.Agent);
            igrid.AddReferenceColumn('status', 'PO Status', 100, $scope.POStatus);
            igrid.AddSimpleColumn('productname', 'PRODUCT NAME', 80);
            igrid.AddSimpleColumn('brandname', 'BRAND', 100);
            igrid.AddSimpleColumn('designid', 'DESIGN NO', 100);
            igrid.AddSimpleColumn('colourname', 'COLOUR');
            igrid.AddSimpleColumn('sizename', 'SIZE', 100);
            igrid.AddSimpleColumn('stylename', 'STYLE', 100);
            igrid.AddSimpleColumn('materialname', 'MATERIAL', 100);
            igrid.AddSimpleColumn('patternname', 'PATTERN', 100);
            igrid.AddSimpleColumn('description', 'DESCRIPTION', 100);
            igrid.AddSimpleColumn('pricerange', 'PRICE RANGE');
            igrid.AddSimpleColumn('size', 'SIZE RANGE', 100);
            igrid.AddFormatedColumn('ordered', 'Ordered Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('received', 'Received Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cancelled', 'Cancelled Qty', 100, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.PendingPOList);
        }


        $scope.UpdatePrice = function () {
            var request = {
                "Item": {
                    'id': $scope.ProductRequest.id,
                    'pricesource': $scope.ProductRequest.pricesource,
                    'groupbysizegroup': !$scope.IsUANBased && !$scope.IsSampleBased
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SalesorderitemsService/UpdatePrice",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.ProductRequest.Salesorderitems = result;
                    $scope.UpdateEditGrid();
                }
                $scope.CalculateOrderQty();
            });
        }


        $scope.ColourShadeAdd = function () {
            if ($scope.ProductRequest.Details.designid &&
                $scope.ProductRequest.Details.designid != null &&
                $scope.ProductRequest.Details.designid != '' &&
                typeof ($scope.ProductRequest.Details.shadesize) != 'undefined' &&
                $scope.ProductRequest.Details.shadesize != null &&
                typeof ($scope.ProductRequest.Details.shades) != 'undefined' &&
                $scope.ProductRequest.Details.shades != null) {

                $scope.ProductRequest.Details.designitemcode = $scope.ProductRequest.Details.designid + '/' + $scope.ProductRequest.Details.shades;

                var exist = $scope.ProductRequest.Salesorderitems.find(function (item) {
                    return (item.designitemcode == $scope.ProductRequest.Details.designitemcode);
                });
                if (exist && exist != null) {

                    $.confirm({
                        title: 'Duplicate Design ',
                        content: 'Do you want add this design ' + $scope.ProductRequest.Details.designitemcode + ' again ?',
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
                                    $timeout(function () {
                                        $scope.ColourShadeAddInternal();
                                    });
                                }
                            },
                            cancel: function () {
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
                else {
                    $scope.ColourShadeAddInternal();
                }
            }
        }

        $scope.ColourShadeAddInternal = function () {
            var diffprice = false;
            var size = $scope.ProductRequest.Details.shadesize;
            var groupqty = parseInt($scope.ProductRequest.Details.groupqty);
            var jumps = $scope.GetJUMPInformation($scope.ProductRequest.Details.shadesize);
            var sizegroupitem = $scope.SizeGroupList.find(function (item) {
                return $scope.ProductRequest.Details.designitemcode == item.designcode;
            });
            if (sizegroupitem) {
                diffprice = sizegroupitem.diffprice;
                sizegroupitem = sizegroupitem.list;
            }
            else {
                $scope.ProductRequest.Details.designitemcode = $scope.ProductRequest.Details.designid + $scope.ProductRequest.Details.shades;
                var sizegroupitem = $scope.SizeGroupList.find(function (item) {
                    return $scope.ProductRequest.Details.designitemcode == item.designcode;
                });
                if (sizegroupitem)
                    sizegroupitem = sizegroupitem.list;
                else
                    return;
            }

            var removejumps = [];
            $.each(jumps, function (k, v) {
                v.oqty = groupqty;
                if (diffprice) {
                    var mfgsizeitem = sizegroupitem.find(function (item) {
                        return item.id == v.sizeid;
                    });
                    if (mfgsizeitem) {
                        v.mrp = mfgsizeitem.mrp;
                        switch ($scope.ProductRequest.pricesource) {
                            case 0:
                                v.price = mfgsizeitem.salerate;
                                v.pricerange = mfgsizeitem.salerate;
                                break;
                            case 1:
                                v.price = mfgsizeitem.wsrate;
                                v.pricerange = mfgsizeitem.wsrate;
                                break;
                            case 2:
                                v.price = mfgsizeitem.rate;
                                v.pricerange = mfgsizeitem.rate;
                                break;
                        }
                    }
                    else {
                        removejumps.push(v);
                    }
                }
                else {
                    var sizegroup = sizegroupitem[0];
                    v.mrp = sizegroup.mrp;
                    switch ($scope.ProductRequest.pricesource) {
                        case 0:
                            v.price = sizegroup.salerate;
                            v.pricerange = sizegroup.salerate;
                            break;
                        case 1:
                            v.price = sizegroup.wsrate;
                            v.pricerange = sizegroup.wsrate;
                            break;
                        case 2:
                            v.price = sizegroup.rate;
                            v.pricerange = sizegroup.rate;
                            break;
                    }
                }
            });

            var jumpList = [];
            if (!diffprice) {
                jumpList.push(jumps);
            }
            else {
                var tjumplist = [];
                $.each(jumps, function (k, v) {
                    if (removejumps.indexOf(v) < 0) {
                        var exist = tjumplist.find(function (item) {
                            return item.mrp == v.mrp &&
                                item.price == v.price;
                        });
                        if (exist && exist != null) {
                            exist.list.push(v);
                        }
                        else {
                            exist = {
                                'mrp': v.mrp, 'price': v.price, 'list': []
                            };
                            tjumplist.push(exist);
                            exist.list.push(v);
                        }
                    }
                });
                $.each(tjumplist, function (k, v) {
                    jumpList.push(v.list);
                })
            }
            var shades = $scope.ProductRequest.Details.shades.split(',');
            var canadd = true;

            $.each(jumpList, function (jk, jv) {
                $.each(shades, function (k, v) {
                    if (canadd) {
                        $scope.JumpList = (JSON.parse(JSON.stringify(jv)));;
                        if (diffprice && jumpList.length > 1) {
                            size = $scope.JumpList.length == 1 ? $scope.JumpList[0].name : ($scope.JumpList[0].name + '-' + $scope.JumpList[$scope.JumpList.length - 1].name);
                        }

                        $scope.ProductRequest.Details.size = size;

                        $scope.ProductRequest.Details.groupqty = groupqty;
                        $scope.ProductRequest.Details.addnlchargeid = $scope.ProductRequest.addnlchargeid;
                        $scope.ProductRequest.Details.addnlcharge = $scope.ProductRequest.addnlcharge;
                        $scope.ProductRequest.Details.oqty = groupqty;
                        $scope.ProductRequest.Details.sizegroup = size;
                        $scope.ProductRequest.Details.sizegroupid = 0;
                        var colour = $scope.ColorList.find(function (item) {
                            return item.code == v || item.displaycode == v;
                        });
                        if (colour && colour != null) {
                            $scope.ProductRequest.Details.colourid = colour.colourid;
                            $scope.ProductRequest.Details.colourcode = colour.code;
                            $scope.ProductRequest.Details.colourshade = colour.displaycode;
                            var sizegroup = $scope.SizeGroupList.find(function (item) {
                                return item.designcode == colour.designcode;
                            });
                            $scope.ProductRequest.Details.designitemcode = sizegroup.designcode;
                            $scope.ProductRequest.Details.mrp = jv[0].mrp;
                            $scope.ProductRequest.Details.price = jv[0].price;
                            $scope.ProductRequest.Details.pricerange = jv[0].pricerange;

                            //switch ($scope.ProductRequest.pricesource) {
                            //    case 0:
                            //        $scope.ProductRequest.Details.price = sizegroup.salerate;
                            //        $scope.ProductRequest.Details.pricerange = sizegroup.salerate;
                            //        break;
                            //    case 1:
                            //        $scope.ProductRequest.Details.price = sizegroup.wsrate;
                            //        $scope.ProductRequest.Details.pricerange = sizegroup.wsrate;
                            //        break;
                            //    case 2:
                            //        $scope.ProductRequest.Details.price = sizegroup.rate;
                            //        $scope.ProductRequest.Details.pricerange = sizegroup.rate;
                            //        break;
                            //}

                            var removeList = [];
                            $.each($scope.JumpList, function (jkey, jvalue) {
                                var sizeitem = sizegroup.list.find(function (item) {
                                    return item.id == jvalue.sizeid;
                                });
                                if (sizeitem)
                                    jvalue.uid = sizeitem.uid;
                                else {
                                    removeList.push(jvalue);
                                }
                            });

                            $.each(removeList, function (jkey, jvalue) {
                                $scope.JumpList.splice($scope.JumpList.indexOf(jvalue), 1);
                            })

                            var exist = $scope.ProductRequest.Salesorderitems.find(function (item) {
                                return (item.designitemcode == $scope.ProductRequest.Details.designitemcode) &&
                                    (item.mrp == $scope.ProductRequest.Details.mrp && item.price == $scope.ProductRequest.Details.price);
                            });

                            if (exist && exist != null) {
                                if (typeof (exist.jumpqty) == 'undefined' || exist.jumpqty == null) {
                                    exist.jumpqty = [];
                                }
                                if ($scope.mfg_sosizeratio || $scope.mfg_sovalidate) {
                                    var copyofexist = (JSON.parse(JSON.stringify(exist)));
                                    $.each(jumpList, function (jkkey, jvvalue) {
                                        $.each(jvvalue, function (jkey, jvalue) {
                                            var existjumpqty = copyofexist.jumpqty.find(function (item) {
                                                return item.sizeid == jvalue.sizeid &&
                                                    (typeof (item.id) == 'undefined' || item.id == null || item.id <= 0);
                                            });
                                            if (existjumpqty && existjumpqty != null) {
                                                existjumpqty.oqty += jvalue.oqty;
                                            } else {
                                                copyofexist.jumpqty.push(jvalue);
                                            }
                                        })
                                    });
                                    if ($scope.mfg_sovalidate) {
                                        if ($scope.ValidateStock(copyofexist)) {
                                            $scope.ProductRequest.Salesorderitems.splice($scope.ProductRequest.Salesorderitems.indexOf(exist), 1);
                                            $scope.ProductRequest.Salesorderitems.splice(0, 0, copyofexist);
                                        }
                                    } else if ($scope.ValidateSizeRatio(copyofexist, $scope.StockList)) {
                                        $scope.ProductRequest.Salesorderitems.splice($scope.ProductRequest.Salesorderitems.indexOf(exist), 1);
                                        $scope.ProductRequest.Salesorderitems.splice(0, 0, copyofexist);
                                    }
                                }
                                else {
                                    $.each(jumpList, function (jkkey, jvvalue) {
                                        $.each(jvvalue, function (jkey, jvalue) {
                                            var existjumpqty = exist.jumpqty.find(function (item) {
                                                return item.sizeid == jvalue.sizeid &&
                                                    (typeof (item.id) == 'undefined' || item.id == null || item.id <= 0);
                                            });
                                            if (existjumpqty && existjumpqty != null) {
                                                existjumpqty.oqty += jvalue.oqty;
                                            } else {
                                                exist.jumpqty.push(jvalue);
                                            }
                                        })
                                    });
                                    $scope.ProductRequest.Salesorderitems.splice($scope.ProductRequest.Salesorderitems.indexOf(exist), 1);
                                    $scope.ProductRequest.Salesorderitems.splice(0, 0, exist);
                                }
                            }
                            else {
                                canadd = $scope.AddItem();
                            }
                        }
                    }
                });
            });
            $scope.ProductRequest.Details.designid = '';
            var lengthysize = $scope.JumpList.find(function (sitem) {
                return sitem.name != null &&
                    sitem.name.length > 5;
            });
            $scope.JumpListStyle = lengthysize && lengthysize != null ? 'col-md-6' : 'col-md-2';
            $scope.setFocusdesign();
            $scope.UpdateEditGrid();
        }

        $scope.JumpListStyle = 'col-md-2';
        $scope.addnlchargechanged = function () {
            $scope.ProductRequest.addnlcharge = 0;
            var charge = $scope.AdditionalCharges.find(function (fitem) {
                return fitem.id == $scope.ProductRequest.addnlchargeid;
            });
            if (charge && charge != null) {
                $scope.ProductRequest.addnlcharge = charge.value;
            }
        }

        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            var qtycolumn = -1;
            var uancolumn = -1;
            $.each(value.columns, function (k, v) {
                if (v.toLowerCase().indexOf('qty') >= 0)
                    qtycolumn = k;
                if (v.toLowerCase().indexOf('ean') >= 0)
                    uancolumn = k;
                if (v.toLowerCase().indexOf('uan') >= 0)
                    uancolumn = k;
            });

            if (qtycolumn == -1) {
                $rootScope.Toaster.Error("Quantity Column Missing");
                return;
            }

            if (uancolumn == -1) {
                $rootScope.Toaster.Error("UAN Column Missing");
                return;
            }

            var uidlist = [];
            $.each(value.rows, function (k, v) {
                if (k == 0) return;
                uidlist.push({ 'uid': v.values[uancolumn], 'qty': v.values[qtycolumn] });
            });

            var request = {
                'Item': { 'items': uidlist },
                "locationid": $scope.ProductRequest.deliverylocationid,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SalesorderService/SearchStockByUID",
                data: srequest
            }).success(function (result) {


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

        $scope.igrid = null;
        $scope.egrid = null;

        $scope.ConfigEditor = function () {
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('gridItems', '400px', '100%', true, true, true, 100);
            igrid.options.showHeaderRow = true;
            igrid.options.autoEdit = true;
            igrid.AddSimpleColumn('sno', 'S.No', 50);
            if ($scope.InputMode != 'SizeGroupColourShade') {
                igrid.AddSimpleColumn('description', 'Detail', 150);
            }

            if ($scope.IsSampleBased) {
                igrid.AddSimpleColumn('mfgsampleordercode', 'Sample Code', 100);
                igrid.AddSimpleColumn('sizegroup', 'Sample Name', 100);
            }

            if ($scope.InputMode == 'SizeGroupColourShade') {
                var column = igrid.AddCustomColumn('designitemcode', 'Design', 120, function (item, col) {
                    if (item.designitemcode && item.designitemcode != '')
                        return item.designitemcode;
                    return item.designid + '/' + item.colourcode;
                });
                column.getValue = function (item, col) {
                    if (item.designitemcode && item.designitemcode != '')
                        return item.designitemcode;
                    return item.designid + '/' + item.colourcode;
                }
                column.enablefilter = true;
            }
            else {
                igrid.AddSimpleColumn('designid', 'Design', 100);
            }

            if ($scope.InputMode == 'SizeGroupColour') {
                var column = igrid.AddCustomColumn('size', 'Size', 200, function (item, col) {
                    if (item.jumpqty && item.jumpqty.length > 0) {
                        var jqty = [];
                        $.each(item.jumpqty, function (k, v) {
                            if (v.oqty > 0) {
                                jqty.push(v.name + '(' + v.oqty + ')');
                            }
                        })
                        return jqty.join(',');
                    }
                    return item.sizename;
                });
                column.sortable = true;
                column.getValue = function (item, col) {
                    if (item.jumpqty && item.jumpqty.length > 0) {
                        var jqty = [];
                        $.each(item.jumpqty, function (k, v) {
                            if (v.oqty > 0) {
                                jqty.push(v.name + '(' + v.oqty + ')');
                            }
                        })
                        return jqty.join(',');
                    }
                    return item.sizename;
                };
            }
            else {
                igrid.AddSimpleColumn('size', 'Size', 60);
                if ($scope.IsUANBased) {
                    var column = igrid.AddColumn(
                        {
                            id: "uid", name: "UAN", field: "uid",
                            width: 100, cssClass: "cell-title",
                            'sortable': true
                        }
                    );
                }
            }
            if ($scope.SupportAdditionalCharges) {
                var column = igrid.AddCustomColumn('pricerange', 'Price', 70, function (item, col) {
                    if (item.addnlcharge && item.addnlcharge != 0)
                        return item.price + item.addnlcharge;
                    return item.price;
                });
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.addnlcharge && item.addnlcharge != 0)
                        return item.price + item.addnlcharge;
                    return item.price;
                };
            }
            else {
                var column = igrid.AddColumn(
                    {
                        id: "pricerange", name: "Price", field: "pricerange",
                        width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
            }
            column.cssClass = 'rightalign';
            if (!$scope.IsSampleBased) {
                var column = igrid.AddColumn(
                    {
                        id: "mrp", name: "MRP", field: "mrp",
                        width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.cssClass = 'rightalign';
            }

            if ($scope.SupportAdditionalCharges) {
                var column = igrid.AddColumn(
                    {
                        id: "addnlchargeid", name: "Addnl", field: "addnlchargeid",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Select2Editor,
                        'sortable': true, formatter: function (row, cell, searchValue, columnDef, dataContext) {
                            if (typeof (searchValue) != 'undefined') {
                                var found = columnDef.dataSource.find(function (arritem) {
                                    return arritem.id == searchValue;
                                });
                                if (typeof (found) != 'undefined' && found != null)
                                    return found.name;
                            }
                            return 'None';
                        }
                    }
                );
                column.dataSource = $scope.AdditionalCharges;
            }

            if ($scope.SupportBoxCharges) {
                var column = igrid.AddColumn(
                    {
                        id: "soboxid", name: "Box", field: "soboxid",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Select2Editor,
                        'sortable': true, formatter: function (row, cell, searchValue, columnDef, dataContext) {
                            if (typeof (searchValue) != 'undefined') {
                                var found = columnDef.dataSource.find(function (arritem) {
                                    return arritem.id == searchValue;
                                });
                                if (typeof (found) != 'undefined' && found != null)
                                    return found.name;
                            }
                            return 'None';
                        }
                    }
                );
                column.dataSource = $scope.BoxList;
            }

            if ($scope.IsUANBased) {

                var column = igrid.AddColumn(
                    {
                        id: "oqty", name: "Qty", field: "oqty",
                        width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
            }
            else if ($scope.system_possizelist != null &&
                $scope.system_possizelist != '') {
                var slist = $scope.system_possizelist.split(',');
                enabletotal = true;
                $.each(slist, function (k, v) {
                    var column = igrid.AddCustomColumn('size_' + k, v, 30, function (item, col) {
                        if (item.jumpqty && item.jumpqty.length > 0) {
                            var jqty = 0;
                            $.each(item.jumpqty, function (k, v) {
                                if (v.name == col.size)
                                    jqty += v.oqty;
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
                        if (item.jumpqty && item.jumpqty.length > 0) {
                            var jqty = 0;
                            $.each(item.jumpqty, function (k, v) {
                                if (v.name == col.size)
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
                            if (lastValue > newvalue) {
                                var reduce = lastValue - newvalue;
                                $.each(item.jumpqty, function (k, v) {
                                    if (reduce > 0 && v.name == col.size) {
                                        if (v.oqty >= reduce) {
                                            v.oqty -= reduce;
                                            reduce = 0;
                                        }
                                        else {
                                            reduce -= v.oqty;
                                            v.oqty = 0;
                                        }
                                    }
                                })
                            }
                            else {
                                var increase = newvalue - lastValue;
                                $.each(item.jumpqty, function (k, v) {
                                    if (increase > 0 && v.name == col.size) {
                                        v.oqty += increase;
                                        increase = 0;
                                    }
                                })
                                if (increase > 0) {
                                    $scope.AddDirectItem(item, col, increase, function () {
                                        $scope.CalculateOrderQty();
                                        $scope.UpdateEditGrid();
                                        callback();
                                    });
                                }
                            }
                            $scope.UpdateEditGrid();
                            $scope.CalculateOrderQty();
                        }
                    }
                    if (!$scope.mfg_sosizeratio)
                        column.editor = Slick.Editors.Text;
                })

                var column = igrid.AddCustomColumn('qty', 'Total', 40, function (item, col) {
                    var qty = 0;
                    if (item.jumpqty && item.jumpqty.length > 0) {
                        $.each(item.jumpqty, function (k, v) {
                            qty += v.oqty;
                        })
                    }
                    return qty;
                });
                column.cssClass = 'rightalign';
                column.sortable = true;
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    var qty = 0;
                    if (item.jumpqty && item.jumpqty.length > 0) {
                        $.each(item.jumpqty, function (k, v) {
                            qty += v.oqty;
                        })
                    }
                    return qty;
                }
            }
            else if ($scope.InputMode == 'SizeGroupColour') {
                var column = igrid.AddCustomColumn('oqty', 'Qty', 60, function (item, col) {
                    if (item.jumpqty && item.jumpqty.length > 0) {
                        var jqty = 0;
                        $.each(item.jumpqty, function (k, v) {
                            jqty += parseFloat(v.oqty);
                        })
                        return jqty;
                    }
                    return item.oqty;
                });
                column.sortable = true;
                column.getValue = function (item, col) {
                    if (item.jumpqty && item.jumpqty.length > 0) {
                        var jqty = 0;
                        $.each(item.jumpqty, function (k, v) {
                            jqty += parseFloat(v.oqty);
                        })
                        return jqty;
                    }
                    return item.oqty;
                };
            }
            else {
                var column = igrid.AddColumn(
                    {
                        id: "oqty", name: "Qty", field: "oqty",
                        width: 70, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
            }
            column.cssClass = 'rightalign';
            //if (result.sizelist && $scope.SearchCondition.showsize && $scope.SearchCondition.customerid > 0) {
            //    $.each(result.sizelist, function (k, v) {
            //        var column = igrid.AddCustomColumn('size_' + k, v, 40,
            //            function (item, col) {
            //                if (item.sz[col.Index] > 0)
            //                    return item.sz[col.Index];
            //                return '';
            //            });

            //        column.Index = k;
            //        column.enablefilter = true;
            //        column.sortable = true;
            //        column.cssClass = 'rightalign';
            //        column.total = true;
            //        column.filtertype = 'number';
            //        column.getValue = function (item, col) {
            //            if (item.sz[col.Index] > 0)
            //                return item.sz[col.Index];
            //            return 0;
            //        }
            //    });
            //    result = result.items;
            //}

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageItem('Delete', args.item);
                });
            });

            igrid.EnableTotal();
            igrid.Initialize();

            igrid.grid.onDblClick.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var field = args.grid.getColumns()[args.cell].id;
                if (field == 'designid') {

                }
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'addnlchargeid') {
                    item.addnlcharge = 0;
                    var charge = $scope.AdditionalCharges.find(function (fitem) {
                        return fitem.id == item.addnlchargeid;
                    });
                    if (charge && charge != null) {
                        item.addnlcharge = charge.value;
                    }
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                    return;
                }
                $timeout(function () {
                    args.grid.invalidateRow(args.row);
                    args.grid.render();
                }, 10);
            });
            $scope.egrid = igrid;
            $scope.egrid.UpdateData([]);
        }

        $scope.AddDirectItem = function (item, col, qty, callback) {
            var request = {
                "Item": {
                    'name': item.designid,
                    'designcode': item.designitemcode && item.designitemcode != '' ?
                        item.designitemcode : (item.designid + '/' + item.colourcode),
                    'sizeid': col.size
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgdesignService/SearchUANByName",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    item.jumpqty.push({
                        'sizeid': result.sizeid,
                        'name': result.sizegroup,
                        'uid': result.uid,
                        'oqty': qty
                    });
                    callback();
                }
            });
        }
        $scope.UpdateEditGrid = function () {
            if ($scope.ProductRequest.Salesorderitems &&
                $scope.ProductRequest.Salesorderitems != null) {
                var index = $scope.ProductRequest.Salesorderitems.length;
                $.each($scope.ProductRequest.Salesorderitems, function (k, v) {
                    v.sno = index--;
                })
                $scope.egrid.UpdateData($scope.ProductRequest.Salesorderitems, '_id');
            }
        }

        $scope.ConfigGrid = function (result) {
            $scope.selectedcount = 0;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            if ($scope.ismulticompany)
                igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
            if ($scope.ismultilocationentry)
                igrid.AddReferenceColumn('locationid', 'LocationList', 100, $scope.Location);

            igrid.AddSimpleColumn('sono', 'S.No', 80);
            igrid.AddSimpleColumn('agentname', 'Agent', 80);
            igrid.AddSimpleColumn('customername', 'Customer', 100);
            igrid.AddSimpleColumn('cityname', 'City', 100);
            igrid.AddReferenceColumn('employeeid', 'Created', 100, $scope.Employees);
            var column = igrid.AddCustomColumn('pickedby', 'Pick By', 120,
                function (item, col) {
                    if (item.termsandcondition != null &&
                        item.termsandcondition.pickedby > 0) {
                        var emp = $scope.Employees.find(function (sitem) {
                            return sitem.id == item.termsandcondition.pickedby
                        });
                        if (emp && emp != null)
                            return emp.name + ' - ' + $filter('date')(item.termsandcondition.pickedon, 'dd-MM-yyyy');
                    }
                    return '';
                });
            //var column = igrid.AddCustomColumn('pickedon', 'Pick On', 100,
            //    function (item, col) {
            //        if (item.termsandcondition != null) {

            //            var date = $filter('date')(item.termsandcondition.pickedon, 'dd-MMM-yyyy');
            //            if (date.endsWith('0001'))
            //                return '';
            //            return date;
            //        }
            //        return '';
            //    });

            igrid.AddReferenceColumn('status', 'Status', 100, $scope.POStatus);
            var column = igrid.AddCustomColumn('smstatus', 'SM Status', 120, function (item, col) {
                if (item.smapprovedby && item.smapprovedby > 0)
                    return 'Approved - ' + $filter('date')(item.smapprovedon, 'dd-MM-yyyy hh:mm a');
                return 'Pending'
            });
            igrid.AddFormatedColumn('sodate', 'SO Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expectedate', 'Exp Date', 80, 'dd-MM-yyyy');

            if ($scope.IsSampleBased) {
                var column = igrid.AddCustomColumn('sampleentryno', 'S.Entry No', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.entryno;

                });
                var column = igrid.AddCustomColumn('designno', 'S.Design No', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.designno;

                });
                var column = igrid.AddCustomColumn('samplename', 'S.Label Name', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.samplename;
                });
                var column = igrid.AddCustomColumn('division', 'S.Division', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.division;
                });
                var column = igrid.AddCustomColumn('merchant', 'Merchant', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.merchant;

                });
                var column = igrid.AddCustomColumn('marketing', 'Marketing', 100, function (item, col) {
                    if (item.mfglabel == undefined || item.mfglabel == null) return '';
                    return item.mfglabel.marketing;
                });
            }
            else {

                igrid.AddFormatedColumn('totalqty', 'Qty', 80, 'N2', 2);
                igrid.AddFormatedColumn('totalsetqty', 'Set', 80, 'N2', 2);
            }

            if (result.sizelist && $scope.SearchCondition.showsize && $scope.SearchCondition.customerid > 0) {
                $.each(result.sizelist, function (k, v) {
                    var column = igrid.AddCustomColumn('size_' + k, v, 40,
                        function (item, col) {
                            if (item.sz[col.Index] > 0)
                                return item.sz[col.Index];
                            return '';
                        });

                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.sz[col.Index] > 0)
                            return item.sz[col.Index];
                        return 0;
                    }
                });
                result = result.items;
            }

            igrid.AddSimpleColumn('sobookno', 'Book No', 50);
            igrid.AddSimpleColumn('sobookqty', 'Book Qty', 50);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Print'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Slip'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Plan'/>"

            igrid.AddButtonColumn('action', 'ACTION', 220, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Delete', args.item);
                    else if ($(e.target).val() == 'Rack No')
                        $scope.SetRackNo(args.item);
                    else if ($(e.target).val() == 'Print')
                        $rootScope.Download('SalesorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + args.item.id, 'salesorder.pdf', 'application/pdf');
                    else if ($(e.target).val() == 'Slip')
                        $rootScope.Download('SalesorderService/PrintSLIP/' + $scope.UserInfo.OrganizationID + '/' + args.item.id, 'salesorder.pdf', 'application/pdf');
                    else if ($(e.target).val() == 'Plan')
                        $rootScope.Download('SalesorderService/PrintPLAN/' + $scope.UserInfo.OrganizationID + '/' + args.item.id, 'salesorder.pdf', 'application/pdf');
                });
            });

            igrid.EnableTotal();
            igrid.Initialize();
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                    $scope.$apply();
                }, 10);
            };
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result);
        }


        $scope.SetRackNo = function (item) {
            $rootScope.Prompt('Rack Number', 'No', function (code) {
                code = code.toString();
                if (typeof (code) != 'undefined' && code.length > 0) {
                    item.soracknumber = code;
                    var request = {
                        "item": { 'id': item.id, 'soracknumber': code },
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "SalesorderService/SaveRackNumber",
                        data: srequest
                    }).success(function (result) {
                        $scope.igrid.referesh();
                    });
                }
            })
        }

        $scope.ShowBoxChargesDialog = function () {
            var seletedRows = this.egrid.GetSelectedRows();
            if (seletedRows.length == 0) return;
            var dialog = ngDialog.open({
                template: 'BoxCharges',
                data: { 'parentscope': $scope },
                width: '250px',

                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', function ($scope, $timeout, $http, hotkeys, $rootScope) {
                    $scope.Parent = $scope.ngDialogData.parentscope;
                    $scope.Change = function () {
                        if ($scope.boxid && $scope.boxid > 0)
                            $scope.closeThisDialog($scope.boxid);
                        else
                            $scope.closeThisDialog(0);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {

                        var charge = $scope.BoxList.find(function (item) {
                            return item.id == data.value;
                        });
                        if (charge && charge != null) {
                            $.each(seletedRows, function (k, v) {
                                v.soboxid = data.value;
                            })
                        }
                        $timeout(function () {
                            $scope.UpdateEditGrid();
                        });
                    }
                }
            });
        }

        $scope.ShowAdditionalChargesDialog = function () {
            var seletedRows = this.egrid.GetSelectedRows();
            if (seletedRows.length == 0) return;
            var dialog = ngDialog.open({
                template: 'AdditionalCharges',
                data: { 'parentscope': $scope },
                width: '250px',

                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', function ($scope, $timeout, $http, hotkeys, $rootScope) {
                    $scope.Parent = $scope.ngDialogData.parentscope;
                    $scope.Change = function () {
                        if ($scope.additionalchargeid && $scope.additionalchargeid > 0)
                            $scope.closeThisDialog($scope.additionalchargeid);
                        else
                            $scope.closeThisDialog(0);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {

                        var charge = $scope.AdditionalCharges.find(function (item) {
                            return item.id == data.value;
                        });
                        if (charge && charge != null) {
                            $.each(seletedRows, function (k, v) {
                                v.addnlchargeid = data.value;
                                v.addnlcharge = charge.value;
                            })
                        }
                        $timeout(function () {
                            $scope.UpdateEditGrid();
                        });
                    }
                }
            });
        }

        $scope.ShowCustomerDialog = function () {
            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: { 'parentscope': $scope },
                width: '800px',

                controller: ['$scope', '$timeout', '$http', 'hotkeys', '$rootScope', function ($scope, $timeout, $http, hotkeys, $rootScope) {

                    $scope.PopupList = [];
                    $scope.Customer = {};
                    $scope.CloseIfOne = false;
                    $scope.Parent = $scope.ngDialogData.parentscope;
                    $scope.ManageAddress = function () {

                        restService.Get('SupplierService/GSTNo/' + $scope.Customer.gstno, true).then(function (result) {
                            result.pradr.id = -1;
                            result.pradr.name = 'Present';
                            result.adadrlist = [];
                            result.adadrlist.push(result.pradr);
                            $.each(result.adadr, function (k, v) {
                                v.id = k + 1;
                                v.name = 'Branch - ' + (k + 1);
                                result.adadrlist.push(v);
                            });
                            $scope.Customer.name = result.tradeNam;
                            $scope.Customer.area = result.stj;
                            $scope.Customer.address = result.pradr.addr.address;
                            $scope.Customer.city = result.pradr.addr.city;
                            $scope.Customer.area = result.pradr.addr.loc;
                            $scope.Customer.district = result.pradr.addr.dst;
                            $scope.Customer.pincode = result.pradr.addr.pncd;
                        });
                    }

                    $scope.Create = function () {
                        if (!$scope.Customer.name || $scope.Customer.name == null || $scope.Customer.name == '') {
                            $rootScope.Toaster.Notice('Please enter the customer name');
                            return;
                        }
                        if (!$scope.Customer.contactno || $scope.Customer.contactno == null || $scope.Customer.contactno == '') {
                            $rootScope.Toaster.Notice('Please enter the customer contact no');
                            return;
                        }
                        var request = {
                            "Item": {
                                'name': $scope.Customer.name,
                                'area': $scope.Customer.area,
                                'mobileno': $scope.Customer.contactno,
                                'gstno': $scope.Customer.gstno,
                                'address': $scope.Customer.address,
                                'cityid': $scope.Customer.cityid,
                                'districtid': $scope.Customer.districtid,
                                'pincode': $scope.Customer.pincode,
                                'hasgst': $scope.Customer.gstno && $scope.Customer.gstno != '',
                                'registredlocationid': $rootScope.LocationInfo.location.id,
                                'locationid': $rootScope.LocationInfo.location.id,
                                'customertype': 2
                            },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };

                    $scope.hasvalue = function (val) {
                        return typeof (val) != 'undefined' && val != null &&
                            val != '' && val.length > 0;
                    }


                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape' && data.value.id > 0) {
                        var customers = $scope.Customers;
                        var existingCustomer = customers.find(function (item) {
                            return item.id == data.value.id;
                        });
                        if (typeof (existingCustomer) == 'undefined' || existingCustomer == null)
                            customers.push(data.value);
                        $scope.Customers = null;
                        $timeout(function () {
                            $scope.Customers = customers;
                            $scope.ProductRequest.customerid = data.value.id;
                            $scope.CustomerChanged();
                        });
                    }
                }
            });
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.ProductRequest', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }

            var dvalue = $scope.LocalContext.Get('$scope.ProductRequest_Detail', {
                'stocklist': [],
                'jumplist': [],
            }, false);
            if (dvalue != null) {
                $scope.StockList = dvalue.stocklist;
                $scope.JumpList = dvalue.jumplist;
            }
            var lengthysize = $scope.JumpList.find(function (sitem) {
                return sitem.name != null &&
                    sitem.name.length > 5;
            });
            $scope.JumpListStyle = lengthysize && lengthysize != null ? 'col-md-6' : 'col-md-2';
            $scope.ProductRequest = value;
            $scope.UpdateEditGrid();
            $scope.ShowSearch = false;
            $scope.CalculateOrderQty();
        };

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
                        $scope.Entity.sizegroup = data.value.name;
                        $scope.ProductRequest.sizegroup = data.value.name;
                        $scope.ConfigEditor();
                        $scope.UpdateEditGrid();
                    }
                }
            });
        }

        $scope.CheckPOSLayout = function (isnew) {
            if (!$scope.system_sizegrouplayout) return;

            if (!isnew &&
                $scope.ProductRequest.sizegroup != $scope.Entity.sizegroup) {
                $scope.ShowSearch = false;
                $timeout(function () {
                    var sizegroup = $scope.SizeGroupList.find(function (item) {
                        return item.name == $scope.ProductRequest.sizegroup;
                    })
                    if (sizegroup && sizegroup != null) {
                        $scope.system_possizelist = sizegroup.possizelist;
                        $scope.SizeIndexList = $scope.system_possizelist.split(',');
                        $scope.ConfigEditor();
                        $scope.UpdateEditGrid();
                    }
                }, 20);
            }

            if (isnew) {
                if ($scope.Entity.sizegroup &&
                    $scope.Entity.sizegroup != null) {
                    var sizegroup = $scope.SizeGroupList.find(function (item) {
                        return item.name == $scope.$scope.Entity.sizegroup;
                    })
                    if (sizegroup && sizegroup != null) {
                        if (sizegroup && $scope.system_possizelist != sizegroup.possizelist) {
                            $scope.system_possizelist = sizegroup.possizelist;
                            $scope.SizeIndexList = $scope.system_possizelist.split(',');
                            $scope.ConfigEditor();
                            $scope.UpdateEditGrid();
                        }
                    }
                }
                else if ($scope.system_possizelist != $scope.ApplicationSettings.appsettings.system_possizelist) {
                    $scope.system_possizelist = $scope.ApplicationSettings.appsettings.system_possizelist;
                    $scope.SizeIndexList = $scope.system_possizelist.split(',');
                    $scope.GridConfig();
                    $scope.UpdateEditGrid();
                }
            }
        }


        $scope.LoadAppSettings = function () {
            if ($rootScope.UserInfo.appsettings) {
                $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
                $scope.ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
                $scope.system_isb2bapplication = $rootScope.UserInfo.appsettings.system_isb2bapplication;
                $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;
                $scope.system_sizegrouplayout = $rootScope.UserInfo.appsettings.system_sizegrouplayout;
                $scope.system_possizelist = $rootScope.UserInfo.appsettings.system_possizelist;
                $scope.mfg_customerpo = $rootScope.UserInfo.appsettings.mfg_customerpo;
                $scope.canchangestatus = $rootScope.UserInfo.p_STATUSCHANGESO && $rootScope.UserInfo.p_STATUSCHANGESO.view;
                $scope.canchangeprice = $rootScope.UserInfo.p_PRICEEDITONSO && $rootScope.UserInfo.p_PRICEEDITONSO.view;
                $scope.fabricbasedorder = $rootScope.UserInfo.appsettings.mfg_fabricbasedorder;
                $scope.hasbooking = false || $rootScope.UserInfo.appsettings.saleorder &&
                    $rootScope.UserInfo.appsettings.saleorder.booking;

                if ($scope.system_possizelist && $scope.system_possizelist != null) {
                    $scope.SizeIndexList = $scope.system_possizelist.split(',');
                }
                switch ($rootScope.UserInfo.appsettings.mfg_mode) {
                    case 0:
                        $scope.InputMode = '';
                    case 1:
                        $scope.InputMode = 'Size';
                        break;
                    case 2:
                        $scope.InputMode = 'SizeGroup';
                        break;
                    case 3:
                        $scope.InputMode = 'SizeGroupColour';
                        break;
                    case 4:
                        $scope.InputMode = 'SizeGroupColourShade';
                        $scope.ProductRequest.mode = 0;
                        break;
                    case 5:
                        $scope.InputMode = 'UAN';
                        $scope.ProductRequest.mode = 4;
                        $scope.IsUANBased = true;
                        break;
                    case 6:
                        $scope.InputMode = 'SAMPLE';
                        $scope.ProductRequest.mode = 5;
                        $scope.IsSampleBased = true;
                        break;
                }

                $scope.ConfigEditor();
            }
            else {
                $timeout(function () {
                    $scope.LoadAppSettings();
                }, 100)
            }
        }

        $timeout(function () {
            $scope.allowchangestatus = $location.search().intray && $location.search().intray == 'true';
            $scope.LoadAppSettings();
            var available = (typeof ($rootScope.windowSize) != 'undefined' &&
                typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 75) + 'px';
            }
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "SalesorderService/Search",
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
        }, 100);
    }]);
