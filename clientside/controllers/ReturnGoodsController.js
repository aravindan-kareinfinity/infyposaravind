
var app = angular.module('InfinityLab');

app.controller('ReturnGoodsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchConditionItems = [{ 'id': 5, 'name': 'Barcode' }, { 'id': 8, 'name': 'From File' }];
        $scope.Permission = UserDetails.Info;
        $scope.immediatetransfer = false;
        $scope.intercompanytransfer = false;
        $scope.HasLocationSelected = (typeof ($rootScope.LocationInfo) != 'undefined' && $rootScope.LocationInfo != null
            && typeof ($rootScope.LocationInfo.location) != 'undefined' && $rootScope.LocationInfo.location!=null)

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.DesignOutQty = [];
        $scope.SourceTypeList = [{ 'id': 0, 'name': 'Barcode' },
            { 'id': 1, 'name': 'Design' },
            { 'id': 2, 'name': 'LR Entry No' }];

        $scope.SourceType = 0;
        $scope.TotalCounts = 0;
        $scope.TotalSaleValue = 0;
        $scope.Entity = {
            "id": 0,
            "items_json": null,
            "Items": [],
            "selectAll": false,
            "code": null,
            "companyid": 0,
            "fromlocationid": 0,
            "tolocationid": 0,
            "packedby": 0,
            "receivedby": 0,
            "packedon": new Date(),
            "notes": null,
            "approvedby": 0,
            "isactive": true,
            "createdon": "",
            "modifiedon": "",
            "createdby": 0,
            "modifiedby": 0,
            "status": 0,
            "version": 1,
            "autotransfer": $scope.immediatetransfer,
            "salablegoods": [],
            "checkList": true,
            "inward2warehouse": false,
            "tocompanyid": ""
        };
        $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.SearchRef = restService.SearchReference;

        $scope.LoadSetting = function () {
            if (typeof ($rootScope.UserInfo) == 'undefined' ||
                typeof ($rootScope.UserInfo.appsettings) == 'undefined' ||
                $rootScope.UserInfo == null ||
                $rootScope.UserInfo.appsettings == null) {
                $timeout(function () {
                    $scope.LoadSetting();
                }, 50);
                return;
            }
            $scope.immediatetransfer = $rootScope.UserInfo.appsettings.system_immediatetransfer;
            $scope.intercompanytransfer = $rootScope.UserInfo.appsettings.system_interstatetransfer;
            $scope.showdesigninfo = false;
            $scope.system_possizelist = $rootScope.UserInfo.appsettings.system_possizelist;

            if ($scope.system_possizelist && $scope.system_possizelist != null && $scope.system_possizelist != '') {
                var sizelist = $scope.system_possizelist.split(',');
                $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
                    $scope.SizeList = results;
                    $.each(sizelist, function (k, v) {
                        var sizeitem = results.find(function (item) {
                            return item.name == v;
                        });
                        if (sizeitem && sizeitem != null) {
                            $scope.DesignOutQty.push({ 'name': v, 'id': sizeitem.id, 'qty': 0 });
                        }
                    });
                });
                $scope.showdesigninfo = true;
            }
        }

        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
        //    $rootScope.BindCompany($scope, 'OutWards.companyid', $scope.FromCompanyChanged);
        //    $rootScope.BindCompany($scope, 'OutWards.tocompanyid', $scope.ToCompanyChanged);
        });

        $scope.RetailLocation = restService.Get('ReferencelistService/MYRETAILLOCATION', true).then(function (results) {
            $scope.RetailLocation = results;
            $scope.AllRetailLocation = results;
            $rootScope.BindLocation($scope, 'SearchCondition.locationid');
            if ($scope.HasLocationSelected && $scope.OutWards != 'undefined' && $scope.OutWards != null) {
                $scope.OutWards.fromlocationid = $rootScope.LocationInfo.location.id;
            }
        });

        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
            $scope.ToLocations = results;
            $scope.ALLLocation = results;
        //    $rootScope.BindLocation($scope, 'OutWards.fromlocationid', $scope.FromLocationChanged);
        //    $rootScope.BindLocation($scope, 'OutWards.tolocationid', $scope.LocationChanged);
        });

        $scope.FromCompanyChanged = function () {
            if ($scope.intercompanytransfer) {
                $scope.RetailLocation = [];
                $.each($scope.AllRetailLocation, function (k, v) {
                    if (v.additionalinfo.parentid <= 0 || v.additionalinfo.parentid == $scope.OutWards.companyid) {
                        $scope.RetailLocation.push(v);
                    }
                });
            }
        }

        $scope.ToCompanyChanged = function () {
            if ($scope.intercompanytransfer) {
                $scope.Location = [];
                $.each($scope.ALLLocation, function (k, v) {
                    if (v.additionalinfo.parentid <= 0 || v.additionalinfo.parentid == $scope.OutWards.tocompanyid) {
                        $scope.Location.push(v);
                    }
                });
            }
        }

        $scope.BrandList = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });

        $scope.selectall = false;
        $scope.SelectAll = function () {
            $.each($scope.OutWards.items, function (k, v) {
                v.selected = $scope.selectall;
            });
        }

        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfilename = null;
            if ($scope.OutWards.fromlocationid == null || $scope.OutWards.fromlocationid == 0) {
                $rootScope.Toaster.Error("Please select transfer location");
                return;
            }
            var barcodelist = [];
            var barcodecount = [];
            $.each(value.rows, function (k, v) {

                var qty = 1;
                if (v.values.length > 1) {
                    qty = parseFloat(v.values[1]);
                    if (typeof (qty) == 'undefined' || qty <= 0 || isNaN(qty))
                        qty = 1;
                }

                for (var c = 0; c < qty;c++)
                    barcodelist.push(v.values[0]);

                var loc = barcodecount.find(function (item) {
                    return item.barcode == v.values[0];
                });
                if (typeof (loc) != 'undefined' && loc != null)
                    loc.qty = loc.qty + qty;
                else
                    barcodecount.push({ 'barcode': v.values[0], 'qty': qty, 'iqty':0 });
            });

            $scope.SearchCondition.barcode = barcodelist.join(',');
            $scope.SearchStock(true, barcodelist, barcodecount);
        }

        $scope.FromLocationChanged = function () {
            $scope.OutWards.items = [];
            $scope.TotalCounts = 0;
            if (!$scope.intercompanytransfer) {
                var loc = $scope.ALLLocation.find(function (item) {
                    return item.id == $scope.OutWards.fromlocationid;
                });
                var hascompanyid = true;
                if (loc.additionalinfo == undefined ||
                    loc.additionalinfo == null ||
                    loc.additionalinfo.parentid == undefined ||
                    loc.additionalinfo.parentid == null)
                    hascompanyid = false;
                else
                    hascompanyid = loc.additionalinfo.parentid > 0;

                var ToLocations = [];
                $.each($scope.ALLLocation, function (k, v) {
                    if (!hascompanyid ||
                        v.additionalinfo == undefined ||
                        v.additionalinfo == null ||
                        v.additionalinfo.parentid == undefined ||
                        v.additionalinfo.parentid == null) {
                        ToLocations.push(v);
                    }
                    else if (v.additionalinfo.parentid == loc.additionalinfo.parentid) {
                        ToLocations.push(v);
                    }
                });
                $scope.ToLocations = ToLocations;
            }
        }

        $scope.LocationChanged = function () {
            var loc = $scope.Location.find(function (item) {
                return item.id == $scope.OutWards.tolocationid;
            });
            if (typeof (loc) != 'undefined' && loc != null && loc.additionalinfo != null)
                $scope.OutWards.inward2warehouse = loc.additionalinfo.iswarehouse;
            else
                $scope.OutWards.inward2warehouse = false;
        }



        $scope.PopItems = function () {
            var removeitem = [];


            $.each($scope.igrid.GetSelectedRows(), function (key, value) {
                removeitem.push(value);
            });

            $.each(removeitem, function (k, v) {
                $scope.OutWards.items.splice($scope.OutWards.items.indexOf(v), 1);
            });

            $scope.TotalCounts = 0;
            $scope.TotalSaleValue = 0;
            $.each($scope.OutWards.items, function (k, v) {
                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);
                $scope.TotalSaleValue += v.iqty * v.price;
            });

            if ($scope.OutWards.items && $scope.OutWards.items != null)
                $scope.igrid.UpdateData($scope.OutWards.items);
        };

        $scope.SearchCondition = {
            id: 0,
            date: null,
            addedfrom: null,
            addedto: null,
            productid: null,
            brandid: null,
            barcode: null,
            companyid: null,
            searchnewbarcodes: false,
            searchtype: 4,
            supplierid: [],
            suppliername: '',
            lrentryno: null,
            selected: false
        }

        $scope.currentcompanyid = 0;

        $scope.QtyChanged = function (qty, oqty) {
            //$scope.TotalCounts = $scope.TotalCounts - $rootScope.GetNumber(oqty);
            //$scope.TotalCounts = $scope.TotalCounts + $rootScope.GetNumber(qty);

            var iserror = false;
            var wrongbarcode = '';
            $scope.TotalCounts = 0;
            $.each($scope.OutWards.items, function (k, v) {
                if (v.qty < v.iqty || v.iqty <= 0) {
                    wrongbarcode += (wrongbarcode.length > 0 ? ',' : '') + v.barcode;
                    iserror = true;
                }
            });

            if (iserror) {
                $rootScope.ShowAlert('No data found', 'Wrong transfer qty for - ' + wrongbarcode);
                return;
            }

            $scope.TotalCounts = 0;
            $scope.TotalSaleValue = 0;
            $.each($scope.OutWards.items, function (k, v) {
                if (v.iqty > v.qty) v.iqty = v.qty;
                $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);
                $scope.TotalSaleValue += v.iqty * v.price;
            });
        };

        $scope.SearchDesignStock = function () {
            var request = {
                "Item": {
                    'designcode': $scope.SearchCondition.designcode,
                    'requiredesignqty': $scope.DesignOutQty,
                    'companyid': $scope.OutWards.companyid,
                    'locationid': $scope.OutWards.fromlocationid
                },
                "securitytoken": "String content"
            }

            if (!request.Item.locationid || request.Item.locationid == null || request.Item.locationid <= 0) {
                $rootScope.Toaster.Error("Please select From Location!");
                return;
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/SearchDesignStock",
                data: srequest
            }).success(function (result) {
                $scope.OutWards.availableitems = result;
                if (typeof ($scope.OutWards.items) == 'undefined' ||
                    $scope.OutWards.items == null)
                    $scope.OutWards.items = [];
                if (result && result.length > 0) {
                    $.each(result, function (k, v) {
                        $scope.OutWards.items.push(v);
                    })
                    if ($scope.igrid == null)
                        $scope.ConfigGrid($scope.OutWards.items);
                    else
                        $scope.igrid.UpdateData($scope.OutWards.items,'_id');
                }
            });
        }

        $scope.SearchAllStock = function () {

            if (!$scope.intercompanytransfer) {
                $scope.OutWards.companyid = 0;
                $scope.OutWards.tocompanyid = 0;
            }

            var request = {
                "Item": { 'barcode': $scope.SearchCondition.directbarcode, 'companyid': $scope.OutWards.companyid, 'searchtype': 5, 'locationid': $scope.OutWards.fromlocationid },
                "securitytoken": "String content"
            }

            if (!request.Item.locationid || request.Item.locationid == null || request.Item.locationid <= 0) {
                $rootScope.Toaster.Error("Please select From Location!");
                return;
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/SearchGoodsStock",
                data: srequest
            }).success(function (result) {
                $scope.OutWards.availableitems = result;
            });
        }
        $scope.SearchDirectStock = function (barcode) {
            if ($scope.OutWards.items && $scope.OutWards.items.length > 0) {
                var existitem = $scope.OutWards.items.find(function (item) {
                    return (item.barcode == barcode || item.serialno == barcode);
                });
                if (existitem && existitem != null) {
                    if (existitem.qty > existitem.iqty) {
                        existitem.iqty = $rootScope.GetNumber(existitem.iqty) + 1;
                        if ($scope.igrid == null)
                            $scope.ConfigGrid($scope.OutWards.items);
                        else
                            $scope.igrid.UpdateData($scope.OutWards.items);
                        $scope.SearchCondition.directbarcode = '';
                        return;
                    }
                    else if (existitem.serialno == barcode) {
                        var existitem = $scope.OutWards.items.find(function (item) {
                            return (item.serialno == barcode && item.qty > item.iqty);
                        });
                        if (existitem && existitem != null) {
                            existitem.iqty = $rootScope.GetNumber(existitem.iqty) + 1;
                            if ($scope.igrid == null)
                                $scope.ConfigGrid($scope.OutWards.items);
                            else
                                $scope.igrid.UpdateData($scope.OutWards.items);
                            $scope.SearchCondition.directbarcode = '';
                            return;
                        }
                        else {
                            var exclusionlist = [];
                            $.each($scope.OutWards.items, function (k, v) {
                                if (v.serialno == barcode) {
                                    exclusionlist.push(v)
                                }
                            });
                            $scope.SearchCondition.barcoded = true;
                            $scope.SearchCondition.locationid = $scope.OutWards.fromlocationid;
                            request = {
                                "Item": {
                                    'barcode': barcode,
                                    'companyid': $scope.OutWards.companyid,
                                    'searchtype': 5,
                                    'locationid': $scope.OutWards.fromlocationid
                                },
                                "securitytoken": "String content"
                            }
                            $scope.FindUANBarcodes(exclusionlist, request);
                            $scope.SearchCondition.directbarcode = '';
                            return;
                        }
                    }
                }
            }
            $scope.SearchStock(true);
        }

        $scope.NumbersOnly = /^[0-9]+$/;
        $scope.SearchStock = function (barcoded, barcodelist, barcodecount) {

            if (!$scope.OutWards.fromlocationid || $scope.OutWards.fromlocationid == null || $scope.OutWards.fromlocationid <= 0) {
                $rootScope.Toaster.Error("Please select From Location!");
                return;
            }

            if ($scope.OutWards.items.length > 0 && barcoded && $scope.SearchCondition.directbarcode.length < 4 &&
                $scope.SearchCondition.directbarcode.match($scope.NumbersOnly)) {
                $scope.OutWards.items[0].iqty += parseInt($scope.SearchCondition.directbarcode);
            }

            if ($scope.currentcompanyid == 0) {
                $scope.currentcompanyid = $scope.OutWards.companyid;
            }
            else if ($scope.currentcompanyid != $scope.OutWards.companyid) {
                $scope.currentcompanyid = $scope.OutWards.companyid;
                $scope.OutWards.items = [];
            }

            $scope.OutWards.availableitems = [];

            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            if (barcoded) {

                if ($scope.SearchCondition.lrentryno != 'undefined' && $scope.SearchCondition.lrentryno != null
                    && $scope.SearchCondition.lrentryno != '') {
                    $scope.SearchCondition.barcoded = true;
                    $scope.SearchCondition.locationid = $scope.OutWards.fromlocationid;
                    request = {
                        "Item": {
                            'barcode': $scope.SearchCondition.lrentryno,
                            'companyid': $scope.OutWards.companyid,
                            'searchtype': 6,
                            'locationid': $scope.OutWards.fromlocationid
                        },
                        "securitytoken": "String content"
                    }
                }
                else {
                    $scope.SearchCondition.barcoded = true;
                    $scope.SearchCondition.locationid = $scope.OutWards.fromlocationid;
                    request = {
                        "Item": {
                            'barcode': (barcodelist == 'undefined' || barcodelist == null) ? $scope.SearchCondition.directbarcode : $scope.SearchCondition.barcode,
                            'companyid': $scope.OutWards.companyid,
                            'searchtype': 5,
                            'locationid': $scope.OutWards.fromlocationid
                        },
                        "securitytoken": "String content"
                    }
                }
            }
            else {
                $scope.SearchCondition.barcoded = false;
                $scope.SearchCondition.companyid = $scope.OutWards.companyid;
                $scope.SearchCondition.locationid = $scope.OutWards.fromlocationid;
            }


            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/SearchGoodsStock",
                data: srequest
            }).success(function (result) {

                if (angular.isArray(result)) {

                    if (result.length == 0 && $scope.SearchCondition.barcoded) {
                        $rootScope.Toaster.Error(request.Item.barcode + " not found");
                        return;
                    }
                    var extrabarcodelist = [];
                    var missingbarcodes = [];

                    var fileupload = typeof (barcodelist) != 'undefined';
                    var firstuanitem = result.find(function (titm) {
                        return titm.serialno != null && titm.serialno != '';
                    });
                    if (fileupload && typeof (firstuanitem) != 'undefined' && firstuanitem != null) {
                        $scope.OutWards.items = [];
                        var removal = [];
                        $.each(result, function (k, v) {

                            if (barcodecount != 'undefined' && barcodecount != null) {
                                var itm = barcodecount.find(function (titm) {
                                    return titm.barcode == v.serialno;
                                });
                                if (typeof (itm) != 'undefined' && itm != null) {
                                    if (v.qty >= itm.qty - itm.iqty) {
                                        v.iqty = itm.qty - itm.iqty;
                                    }
                                    else {
                                        v.iqty = v.qty;
                                    }
                                    itm.iqty += v.iqty;
                                    $scope.OutWards.items.push(v);
                                    removal.push(v);
                                }
                            }
                        });

                        $.each(removal, function (k, v) {
                            result.splice(result.indexOf(v), 1);
                        });
                    }

                    $.each(result, function (k, v) {

                        if (barcoded && fileupload) {

                            var itm = $scope.OutWards.items.find(function (titm) {
                                return v.stockid == titm.stockid;
                            });

                            if (typeof (itm) == 'undefined' || itm == null) {

                                if (v.qty == 'undefined' || v.qty == null || v.qty <= 0) {
                                    missingbarcodes.push(v.barcode + ((v.serialno != null && v.serialno != '') ? ':' + v.serialno : ''));
                                    return;
                                }

                                //v.iqty = v.pqty > 0 ? (v.pqty>v.qty?v.pqty:v.qty) : 1;
                                v.iqty = v.pqty > 0 ? v.pqty : 1;
                                if (barcodecount != 'undefined' && barcodecount != null) {
                                    var itm = barcodecount.find(function (itm) {
                                        return itm.barcode == v.barcode;
                                    });
                                    if (typeof (itm) != 'undefined' || itm != null) {
                                        v.iqty = itm.qty;
                                        $scope.OutWards.items.splice(0, 0, v);
                                    }
                                    else {
                                        var itm = $scope.OutWards.items.find(function (itm) {
                                            return itm.barcode == v.barcode;
                                        });
                                        if (typeof (itm) != 'undefined' || itm != null) {
                                            if (v.iqty == 0) {
                                                if (v.qty >= itm.qty) {
                                                    v.iqty = itm.qty;
                                                    itm.qty = 0;
                                                }
                                                else {
                                                    v.iqty = v.qty;
                                                    itm.qty -= itm.qty;
                                                }
                                            }
                                        }
                                        else {
                                            $scope.OutWards.items.splice(0, 0, v);
                                        }
                                    }
                                }
                                else {
                                    $scope.OutWards.items.splice(0, 0, v);
                                }
                            }
                            else {
                                if (itm.iqty < v.qty) {
                                    itm.iqty += (v.pqty > 0 ? v.pqty : 1);
                                    if (v.pqty > 0)
                                        itm.qty += v.pqty;
                                    $scope.OutWards.items.splice($scope.OutWards.items.indexOf(itm), 1);
                                    $scope.OutWards.items.splice(0, 0, itm);
                                }
                            }
                            $scope.SearchCondition.directbarcode = '';
                        }
                        else if (barcoded && !fileupload) {
                            var itm = $scope.OutWards.items.find(function (itm) {
                                return v.barcode == itm.barcode;
                            });

                            if ((v.serialno != null && v.serialno != '') && (typeof (itm) == 'undefined' || itm == null)) {
                                var itm = $scope.OutWards.items.find(function (itm) {
                                    return v.serialno == itm.serialno;
                                });
                            }
                            if (typeof (itm) == 'undefined' || itm == null) {
                                if (v.qty > 0) {
                                    if (request.Item.searchtype == 6)
                                        v.iqty = v.qty;
                                    else
                                        v.iqty = 1;
                                    //$scope.OutWards.items.push(v);
                                    $scope.OutWards.items.splice(0, 0, v);
                                }
                                else
                                    missingbarcodes.push(v.barcode);
                            }
                            else if (itm.qty >= itm.iqty + 1) {
                                if (request.Item.searchtype == 6)
                                    item.iqty = item.qty;
                                else
                                    itm.iqty = itm.iqty + 1;
                            }
                            else {
                                if (itm.serialno != null && itm.serialno != '')
                                    extrabarcodelist.push(itm);
                                else
                                    missingbarcodes.push(v.barcode);
                            }
                        }
                        else {
                            var itm = $scope.OutWards.items.find(function (itm) {
                                return v.stockid == itm.stockid;
                            });
                            if (typeof (itm) == 'undefined' || itm == null) {
                                //$scope.OutWards.items.push(v);
                                $scope.OutWards.items.splice(0, 0, v);
                            }
                        }

                    });

                    $scope.TotalSaleValue = 0;
                    $scope.TotalCounts = 0;
                    $scope.SearchCondition.directbarcode = '';

                    if ($scope.OutWards != null && $scope.OutWards.items != null) {
                        $.each($scope.OutWards.items, function (k, v) {
                            if (v.iqty > v.qty)
                                v.iqty = v.qty;
                            $scope.TotalSaleValue += v.iqty * v.price;
                            $scope.TotalCounts += v.iqty;
                        });
                    }

                    if (missingbarcodes.length > 0) {
                        $rootScope.Toaster.Error("There is no enough stock to transfer for some barcodes " +
                            missingbarcodes.join(','));
                    }

                    if (extrabarcodelist.length > 0) {
                        $scope.FindUANBarcodes(extrabarcodelist, request);
                    }
                    else if ($scope.SearchCondition.barcoded) {
                        $scope.SearchCondition.directbarcode = '';
                    }

                    if ($scope.igrid == null)
                        $scope.ConfigGrid($scope.OutWards.items);
                    else
                        $scope.igrid.UpdateData($scope.OutWards.items);

                    $scope.ShowMissingBarcodes(barcoded, barcodelist, barcodecount);
                }


                $scope['setFocusbarcode']();
            }).error(function (result) {
            });
        }

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.FindUANBarcodes = function (excludebarcode, request) {
            var barcodelist = [];
            var stockidlist = [];
            $.each(excludebarcode, function (k, v) {
                if (barcodelist.indexOf(v.serialno) < 0)
                    barcodelist.push(v.serialno);
                if (stockidlist.indexOf(v.stockid) < 0)
                    stockidlist.push(v.stockid);
            });
            request.Item.barcode = barcodelist.join(',');
            request.Item.exclusionlist = stockidlist;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/SearchGoodsStock",
                data: srequest
            }).success(function (result) {
                $.each(result, function (k, v) {
                    var itm = $scope.OutWards.items.find(function (itm) {
                        return v.stockid == itm.stockid;
                    });
                    if (typeof (itm) == 'undefined' || itm == null) {
                        $scope.OutWards.items.push(v);
                        $scope.igrid.UpdateData($scope.OutWards.items);
                    }
                });

                var missingbarcodes = [];
                $.each(barcodelist, function (k, v) {
                    var exist = result.find(function (item) {
                        return item.serialno == v;
                    });
                    if (typeof (exist) == 'undefined' || exist == null)
                        missingbarcodes.push(v);
                });

                if (missingbarcodes.length > 0) {
                    $rootScope.Toaster.Error('No data found - ' + missingbarcodes.join(','));
                }
                else if ($scope.SearchCondition.barcoded) {
                    $scope.SearchCondition.directbarcode = '';
                }

            });
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'Transfer Details';
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

        $scope.DownloadExcel = function (item) {

            $rootScope.Download("BundlesService/DownloadExcel/" + $scope.UserInfo.OrganizationID + '/' + $scope.OutWards.id, "bundle_" + $scope.OutWards.code + ".xlsx", 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
            return;
        }

        $scope.ShowMissingBarcodes = function (barcoded, barcodelist, barcodecount) {
            if (barcoded && barcodelist && barcodecount) {

                var list = [];
                $.each(barcodecount, function (k, v) {
                    var qty = 0;
                    $.each($scope.OutWards.items, function (kk, vv) {
                        if (vv.serialno == v.barcode || vv.barcode == v.barcode)
                            qty += vv.iqty;
                    });
                    if (qty < v.qty) {
                        list.push({ 'barcode': v.barcode,'iqty':qty, 'rqty': v.qty });
                    }
                })

                if ($scope.migrid == null)
                    $scope.ConfigMGrid(list);
                else
                    $scope.migrid.UpdateData(list);
            }
        }

        $scope.igrid = null;
        $scope.ConfigGrid = function (result) {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddSimpleColumn('serialno', 'UAN', 120);
            if ($scope.showdesigninfo) {
                igrid.AddCustomColumn('printingname', 'Item Detail', 180, function (item, col) {
                    if (item.designid != null && item.designid != '')
                        return item.printingname + '/' + item.designid;
                    return item.printingname;
                });
            }
            else {
                igrid.AddSimpleColumn('printingname', 'Item Detail', 180);
            }

            if (typeof ($scope.Permission.p_SHOWVALUE) != 'undefined' && !$scope.Permission.p_SHOWVALUE.view)
                igrid.AddFormatedColumn('buyingprice', 'Cost Price', 80, 'N2', 2);

            if (typeof ($scope.Permission.p_SHOWSELLINGVALUE) != 'undefined' && !$scope.Permission.p_SHOWSELLINGVALUE.view)
                igrid.AddFormatedColumn('price', 'Sale Price', 80, 'N2', 2);

            igrid.AddFormatedColumn('mrp', 'MRP', 60, 'N2', 2);

            if ($scope.OutWards.status == 0) {
                igrid.AddFormatedColumn('qty', 'Stock', 60, 'N2', 2).total = true;
                var column = igrid.AddColumn(
                    {
                        id: "Inward", name: "Transfer Qty", field: "iqty",
                        width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    }
                );
                column.cssClass = 'rightalign';
                column.total = true;
            }
            else {
                igrid.AddFormatedColumn('iqty', 'Transferred', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('aqty', 'Accepted', 60, 'N2', 2).total = true;
            }

            var column = igrid.AddCustomColumn('price', 'Stock Value', 80, function (item, col) {
                if (typeof (item.price) != 'undefined' && typeof (item.iqty) != 'undefined') {
                    return (item.price * item.iqty).toFixed(2);
                }
            });

            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                if (typeof (item.price) != 'undefined' && typeof (item.iqty) != 'undefined') {
                    return (item.price * item.iqty);
                }
                return 0;
            }
            
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.UpdateData(result);
            $scope.igrid = igrid;
        }


        $scope.migrid = null;
        $scope.ConfigMGrid = function (result) {
            if ($scope.migrid != null) {
                $scope.migrid.grid.destroy();
                $('#missingGridContainer').empty();
            }
            var migrid = new InfyGrid('missingGridContainer', '425px', '100%', true, true, true, 280);
            migrid.AddSimpleColumn('barcode', 'Barcode', 100);
            migrid.AddFormatedColumn('iqty', 'Stock', 60, 'N2', 2).total = true;
            migrid.AddFormatedColumn('rqty', 'Required', 60, 'N2', 2).total = true;
            migrid.EnableTotal();
            migrid.Initialize();
            migrid.UpdateData(result);
            $scope.migrid = migrid;
        }

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.Clear = function () {
            $scope.OutWards = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.OutWards.items = [];
            if (typeof ($scope.igrid) != 'undefined' && $scope.igrid != null)
                $scope.igrid.UpdateData($scope.OutWards.items);

            if ($scope.migrid != null) {
                $scope.migrid.grid.destroy();
                $('#missingGridContainer').empty();
            }
        };

        $scope.Save = function () {

            if ($scope.OutWards.fromlocationid == 0 || $scope.OutWards.fromlocationid == null) {
                $rootScope.Toaster.Error("Please select From Location!");
                return;
            }
            if (($scope.OutWards.inward2warehouse == 'undefined' || $scope.OutWards.inward2warehouse == false)
                && ($scope.OutWards.tolocationid == 0 || $scope.OutWards.tolocationid == null)) {
                $rootScope.Toaster.Error("Please select To Location!");
                return;
            }
            if ($scope.OutWards.fromlocationid == $scope.OutWards.tolocationid) {
                $rootScope.Toaster.Error("Transfer location and receiving locations are same., Please change.");
                return;
            }

            //if ($scope.OutWards.autotransfer &&
            //    $scope.OutWards.fromlocationid != null && $scope.OutWards.fromlocationid > 0 &&
            //    $scope.OutWards.tolocationid != null && $scope.OutWards.tolocationid > 0) {
            //    var fromlocation = $scope.Location.find(function (item) { return item.id == $scope.OutWards.fromlocationid; });
            //    var tolocation = $scope.Location.find(function (item) { return item.id == $scope.OutWards.tolocationid; });
            //    if (fromlocation != null && tolocation != null && fromlocation.additionalinfo != null && tolocation.additionalinfo != null &&
            //        fromlocation.additionalinfo.parentid > 0 && tolocation.additionalinfo.parentid > 0 &&
            //        fromlocation.additionalinfo.parentid != tolocation.additionalinfo.parentid) {
            //        $rootScope.Toaster.Error("Auto transfer will not support transfer between companies");
            //        return;
            //    }
            //    if (fromlocation == null || tolocation == null)
            //        $scope.OutWards.autotransfer = false;
            //}
            if ($scope.OutWards.autotransfer && $scope.intercompanytransfer) {
                $rootScope.Toaster.Error("Auto transfer will not support transfer between companies");
                return;
            }
            if ($scope.intercompanytransfer && ($scope.OutWards.companyid == null || $scope.OutWards.companyid == 0)) {
                $rootScope.Toaster.Error("Please select transfer from company details");
                return;
            }
            if ($scope.intercompanytransfer && ($scope.OutWards.tocompanyid == null || $scope.OutWards.tocompanyid == 0)) {
                $rootScope.Toaster.Error("Please select transfer to company details");
                return;
            }

            if (!$scope.intercompanytransfer) {
                $scope.OutWards.companyid = 0;
                $scope.OutWards.tocompanyid = 0;
            }

            request = {
                "Item": $scope.OutWards,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/OutwardGoods",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved");
                    $scope.OutWards.code = result.code;
                    $scope.OutWards.id = result.id;
                    $scope.OutWards.status = result.status;
                }
                else {
                    $rootScope.Toaster.Error(result.errormessage);
                }

            }).error(function (result) {
            });
        }


        $scope.SearchByCode = function () {
            request = {
                "Item": {
                    'code': $scope.OutWards.code,
                    'validatefromlocation': true
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/SearchGoods",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.OutWards = result[0];

                    switch ($scope.OutWards.status) {
                        case 0:
                            $scope.OutWards.statusdisplay = 'New Bundle';
                            break;
                        case 1:
                            $scope.OutWards.statusdisplay = 'Inwarded Bundle';
                            break;
                        case 2:
                            $scope.OutWards.statusdisplay = 'Inwarded Bundle with changes';
                            break;
                        case 9:
                            $scope.OutWards.statusdisplay = 'Rejected Bundle';
                            break;
                    }

                    $scope.TotalCounts = 0;
                    $scope.TotalSaleValue = 0;
                    $.each($scope.OutWards.items, function (k, v) {
                        $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);
                        $scope.TotalSaleValue += v.iqty * v.price;
                    });
                    $scope.ConfigGrid($scope.OutWards.items);
                }
            }).error(function (result) {
            });
        }

        $scope.ShowSuppliers = function () {
            var dialog = ngDialog.open({
                template: 'SupplierSearch',
                data: [$scope, $scope.SupplierList],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SupplierList = $scope.ngDialogData[1];
                    $.each($scope.SupplierList, function (k, v) {
                        v.selected = true;
                    });

                    $scope.Selected = function () {
                        var ret = {
                            'name': '', supplierid: []
                        };
                        $.each($scope.SupplierList, function (k, v) {
                            if (typeof (v.selected) != 'undefined' && v.selected) {
                                ret.name += (ret.name.length > 0 ? ',' : '') + v.name;
                                ret.supplierid.push(v.id);
                            }
                        });
                        $scope.closeThisDialog(ret);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.SearchCondition.suppliername = data.value.name;
                        $scope.SearchCondition.supplierid = data.value.supplierid;
                    }
                }
            });
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $scope.OutWards.items.splice($scope.OutWards.items.indexOf(item), 1);

                $scope.TotalCounts = 0;
                $scope.TotalSaleValue = 0;
                $.each($scope.OutWards.items, function (k, v) {
                    $scope.TotalCounts += (v.ismeter ? 1 : v.iqty);
                    $scope.TotalSaleValue += v.iqty * v.price;
                });

            }
        }

        $scope.Reject = function () {
            request = {
                "Item": $scope.OutWards,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BundlesService/RejectGoods",
                data: srequest
            }).success(function (result) {

                if (result) {
                    $scope.ErrorMessage = "Successfully Removed";
                    $scope.ErrorStyle = "success";
                }
                else {
                    $scope.ErrorMessage = "Some of the items already sold, can't reject";
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
            });
        }


        $scope.PrintBarcode = function () {
            var ids = [];
            var _selectindex = 1;
            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                if (v.iqty && v.iqty > 0) {
                    ids.push({
                        'barcode': v.barcode,
                        'noofcopy': v.iqty,
                        'selectindex': _selectindex++
                    });
                }
            });

            if (ids.length == 0) {
                $rootScope.Toaster.Info("Please select barcode to be print.");
            }
            $scope.ChoseReport({
                'iswarehouse': false,
                'locationid': $scope.OutWards.fromlocationid,
                'barcodes': ids, 'reportid': 0
            });
        };

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
                    url: "PricechangeService/PrintBarcode",
                    data: srequest
                }).success(function (result) {
                    if (result != null && result.length > 0) {
                        $rootScope.Sent2Printer(result.join(';'), printdata);
                    }
                });
            });
        }

        var pagesize = 20;

        $scope.PrintPage = function () {
            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
                var titlele = angular.element('#apptitle').context.title = "OUTWARDS SLIP";

                var docHead = document.head.outerHTML;

                var table = $('#printdiv');
                var ctable = table.clone();

                var width = 800, height = 900;
                var printContents = ctable[0].outerHTML;
                var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=" + width + ", height=" + height + ", resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

                var newWin = window.open("", "_blank", winAttr);
                var writeDoc = newWin.document;
                writeDoc.open();
                writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
                writeDoc.close();
                newWin.focus();

                angular.element('#apptitle').context.title = otitle;
            }, 100);
        }

        $scope.PrintPDF = function (item) {
            $timeout(function () {
                $rootScope.Download('BundlesService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.OutWards.id, 'bundle.pdf', 'application/pdf');
                return;

                var link = document.createElement('a');
                link.download = "debitnote.pdf";
                link.href = 'BundlesService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.OutWards.id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }
        $scope.LoadSetting();
        $scope.PrintDocumentList = [];
        $scope.Print = function () {
            $scope.totalqty = 0.0;

            request = {
                "Item": { 'id': $scope.OutWards.id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BundlesService/Print",
                data: srequest
            }).success(function (result) {
                $scope.PrintDocument = result;
                $scope.PrintDocumentList = [];
                var items = [];

                $.each(result.items, function (k, v) {
                    var exist = items.find(function (item) {
                        return item.productid == v.productid;
                    });
                    if (exist == null) {
                        var item = (JSON.parse(JSON.stringify(v)));
                        items.push(item);
                    }
                    else {
                        exist.iqty += v.iqty;
                    }
                });

                $.each(items, function (k, v) {
                    $scope.PrintDocumentList.push(v);
                });

                $.each($scope.PrintDocumentList, function (k, v) {
                    $scope.totalqty += v.iqty;
                });
                $scope.PrintPage();

            }).error(function (result) {
            });

        }


        $scope.Clear();
    }]);
