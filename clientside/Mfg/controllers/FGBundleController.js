
var app = angular.module('InfinityLab');

app.controller('FGBundleController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', 'cntxService',
    '$timeout', 'Excel','$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, cntxService, $timeout, Excel, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.PackslipSearchModes = [{ 'id': 1, 'name': 'Billed' }, { 'id': 2, 'name': 'All' }, { 'id': 3, 'name': 'Unbilled' }];
        $scope.SetTotalQty = 0;
        $scope.ActionMode = 3;
        $scope.selectedcount = 0;
        $scope.ShowBillItems = true;
        $scope.ShowComboitems = true;
        $scope.FGBundleSoitems = [];
        $scope.FGBundleDesigns = []
        $scope.SelectOption = function (option) {
            $scope.ActionMode = option;

            $scope.ShowBillItems = option == 3 || option == 1;
            $scope.ShowComboitems = option == 3 || option == 2;

            $timeout(function () {
                if ($scope.ShowBillItems) {
                    $scope.PackItemGridConfig();
                } else {
                    $scope.SetConfig();
                    $scope.SetItemConfig();
                }
            }, 10);

        }
        $scope.intercompanytransfer = false;
        $scope.autosetbarcode = true;
        $scope.mixedbarcode = true;
        $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.Customers = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.ShowSearch = false;
        $scope.SearchResults = [];
        $scope.SearchBOMOutwardResult = [];
        $scope.SearchRef = restService.SearchReference;

        $scope.CompanyList = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.CompanyList = results;
            $rootScope.BindCompany($scope, 'FGBundle.companyid');
            $rootScope.BindCompany($scope, 'Entity.companyid');
        });

        $scope.BrandList = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandList = results;
        });
        $scope.DepartmentList = restService.Get('ReferencelistService/MFRDEPARTMENT', true).then(function (results) {
            $scope.DepartmentList = results;
        });

        $scope.SetProductList = restService.Get('ReferencelistService/SETPRODUCTS', true).then(function (results) {
            $scope.SetProductList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.Location = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Location = results;
            $rootScope.BindLocation($scope, 'FGBundle.locationid');
            $rootScope.BindLocation($scope, 'Entity.locationid');
        });

        $scope.WHLocation = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WHLocation = results;
        });

        $scope.AllLocation = [];
        $scope.RetailLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.RetailLocation = [];
            $.each(results, function (k, v) {
                if (v.additionalinfo == null || !v.additionalinfo.iswarehouse) {
                    $scope.RetailLocation.push(v);
                }
                $scope.AllLocation.push(v);
            });
        });

        $scope.BOMCategories = [];
        restService.Get('ReferencelistService/BOMCATEGORY', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.BOMCategories.push(v);
            });
        });

        $scope.BOMAllList = [];
        restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.BOMAllList.push(v);
            });
        });

        restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });

        $scope.ColorList = [];
        $scope.AllColorList = [];
        restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
            $.each(results, function (k, v) {
                $scope.AllColorList.push(v);
            });
        });

        $scope.BOMTypeList = restService.Get('ReferencelistService/BOMTYPE', true).then(function (results) {
            $scope.BOMTypeList = results;
        });
        $scope.BOMDept = restService.Get('ReferencelistService/BOMDEPARTMENT', true).then(function (results) {
            $scope.BOMDept = results;
        });

        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });

        $scope.LocalContext = cntxService.CreatePageContext('FGBundle');
        $scope.TotalAccepted = 0;
        $scope.TotalCounts = 0;
        $scope.SearchCounts = 0;
        $scope.Entity = {
            "id": 0,
            "items_json": null,
            "selectAll": false,
            "code": null,
            "companyid": 0,
            "locationid": 0,
            "tolocationid": 0,
            "fromdepartmentid": 0,
            "todepartmentid": 0,
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
            "qty": 0,
            "version": 1,
            "cansave": true,
            "candelete": false,
            "items": {},
            "checkList": true,
            tocompanyid: ""
        };

        $scope.LastFGBundle = null;
        $scope.FGBundle = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.FGBundleItems = [];

        $scope.SampleDesignChanged = function () {
            if ($scope.FGBundle.soid &&
                $scope.FGBundle.soid > 0) {
                var sample = $scope.CustomerSOList.find(function (item) {
                    return item.id == $scope.FGBundle.soid;
                });
                if (sample && sample != null) {
                    var JumpList = [];
                    $.each(sample.items, function (k, v) {
                        JumpList.push({
                            'id': v.id, 'name': $scope.SearchRef(v.sizeid, $scope.SizeList, '-'),
                            'uid': v.uid, 'description': v.description , 'oqty':v.qty
                        });
                    });
                    $scope.SampleSizeList = JumpList;
                }
            }
        }


        $scope.SampleQtyChanged = function () {
            var tqty = 0;
            $.each($scope.SampleSizeList, function (k, v) {
                var qty = parseFloat(v.qty);
                if (isNaN(qty)) qty = 0;
                tqty += qty;
            })
            $scope.FGBundle.totalqtyqty = tqty;
        }

        $scope.SampleChanged = function () {
            $scope.FGBundle.sampleimageid = null;
            if ($scope.FGBundle.sampleid &&
                $scope.FGBundle.sampleid > 0) {
                restService.Get('ReferencelistService/MFGPENDINGSO/' + $scope.FGBundle.customerid + '-' + $scope.FGBundle.sampleid, true).then(function (results) {
                    $scope.CustomerSOList = results;
                    $.each(results, function (k, v) {
                        v.name = v.designno + '(' + v.sono+' on ' + $filter('date')(v.sodate, 'dd-MMM-yyyy') + ')';
                    })
                });
            }
        }


        $scope.CustomerChanged = function () {
            $scope.FGBundle.customerpricesource = 0;
            if ($scope.FGBundle.customerid && $scope.FGBundle.customerid != null && $scope.FGBundle.customerid > 0) {
                var customer = $scope.Customers.find(function (item) {
                    return item.id == $scope.FGBundle.customerid;
                });
                if (!$scope.IsSampleBased) {
                    if (customer && customer != null) {
                        $scope.FGBundle.customerpricesource = customer.psource;
                    }
                }
                else {
                    restService.Get('ReferencelistService/MFGSAMPLES/' + $scope.FGBundle.customerid, true).then(function (results) {
                        $scope.CustomerSampleList = results;
                    });
                }
            }
        }

        $scope.CreateDirectCombo = function () {

            var dialog = ngDialog.open({
                template: 'ComboCreatorDirect',
                data: [$scope, $scope.SizeList, $scope.FGBundleSoitems],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.designlist = [];
                    $.each($scope.Parent.FGBundleSetitems, function (k, v) {
                        var exist = $scope.designlist.find(function (item) {
                            return item.uan == v.uan;
                        });
                        if (exist == undefined || exist == null) {
                            $scope.designlist.push({ 'uan': v.uan, 'display': v.designitemcode + '(' + v.uan + ')', 'list': [v] })
                        }
                        else {
                            exist.list.push(v);
                        }
                    });

                    //var newsetitem = {
                    //    'uan': uanitem.uan, 'barcode': barcode, 'itemproductid': comboitemdesign.productid,
                    //    'productid': uanitem.productid, 'config': exist, 'itemsizeid': uanproductitem.sizeid,
                    //    'itemcode': uanproductitem.code,
                    //    'designitemcode': combouan.code, 'sizeid': combouan.sizeid, 'mrp': combouan.mrp, 'qty': 1,
                    //    'sno': $scope.FGBundleSetitems.length + 1, 'rqty': uanitem.qty, 'designid': exist.code
                    //};

                    $scope.Selected = function () {

                        var selected = $scope.designlist.find(function (item) {
                            return item.uan == $scope.selecteduan;
                        });

                        var comboitem = selected.list[0].config.sizegroup.items.find(function (sitem) {
                            return sitem.uid == $scope.selecteduan;
                        });
                        comboitem = (JSON.parse(JSON.stringify(comboitem)));
                        comboitem.productid = selected.list[0].config.productid;
                        comboitem.items = [];
                        comboitem.qty = 1;
                        comboitem.designid = selected.list[0].config.id;
                        comboitem.design = selected.list[0].config.code;
                        $scope.ComboItem = comboitem;

                        $scope.ComboItem.qty = parseFloat($scope.qty);
                        if (isNaN($scope.ComboItem.qty)) $scope.ComboItem.qty = 1;
                        $scope.closeThisDialog($scope.ComboItem);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        comboitem = data.value;
                        comboitem.sno = $scope.FGBundleComboitems.length + 1;

                        $.each($scope.FGBundleSetitems, function (k, v) {
                            comboitem.items.push(v);
                        });

                        $scope.FGBundleSetitems = [];

                        var exist = $scope.FGBundleComboitems.find(function (sitem) {
                            return sitem.uid == comboitem.uid;
                        });
                        if (exist && exist != null) {
                            exist.qty += comboitem.qty;
                            $.each(comboitem.items, function (k, v) {
                                exist.items.push(v);
                            });
                        }
                        else {
                            $scope.FGBundleComboitems.push(comboitem);
                        }
                        $scope.SetConfig();
                        $scope.SetItemConfig();
                        $scope.FGBundleItems = [];
                        $scope.PackItemGridConfig();
                    }
                }
            });

        }

        $scope.SearchComboitem = function (barcode, retry) {

            if (barcode == '*') {
                $scope.FGBundleSetitems = [];;
                $scope.FGBundleComboitems = [];
                $scope.FGBundle.comboitembarcode = '';
                $scope.SetItemConfig();
                $scope.SetConfig();
                $scope.setFocusbomcode();
                return;
            }

            if (barcode == 'P' || barcode == 'p') {
                $scope.PrintBarcode();
                $scope.FGBundle.comboitembarcode = ''
                return;
            }

            if (barcode == 'G' || barcode == 'g') {
                $scope.GenerateCombo();
                $scope.FGBundle.comboitembarcode = ''
                return;
            }

            if (barcode == 'C' || barcode == 'c') {
                $scope.CreateDirectCombo();
                $scope.FGBundle.comboitembarcode = ''
                return;
            }

            if (typeof ($scope.FGBundleSoitems) == 'undefined' || $scope.FGBundleSoitems == null) $scope.FGBundleSoitems = [];

            var existingsetitem = $scope.FGBundleSetitems.find(function (item) {
                return item.barcode == barcode
            });

            if (existingsetitem && existingsetitem != null) {
                existingsetitem.qty++;
                $scope.setitemgrid.UpdateData($scope.FGBundleSetitems, '_id');
                $scope.FGBundle.comboitembarcode = ''
                $scope.setFocusbomcode();
                return;
            }

            var exist = $scope.FGBundleSoitems.find(function (item) {
                if (item.comboconfig.uanlist && item.comboconfig.uanlist.length > 0) {
                    var uanitem = item.comboconfig.uanlist.find(function (uitem) {
                        return uitem.uans.indexOf(barcode) >= 0;
                    })
                    return typeof (uanitem) != 'undefined' && uanitem != null;
                }
                return false;
            })

            if (typeof (exist) != 'undefined' && exist != null) {
                var uanitem = null;

                if ($scope.FGBundleSetitems.length > 0) {
                    uanitem = exist.comboconfig.uanlist.find(function (uitem) {
                        return uitem.uan == $scope.FGBundleSetitems[0].uan &&
                            uitem.uans.indexOf(barcode) >= 0;
                    });
                }

                if (uanitem == undefined || uanitem == null) {
                    uanitem = exist.comboconfig.uanlist.find(function (uitem) {
                        return uitem.uans.indexOf(barcode) >= 0;
                    });
                }

                var combouan = exist.sizegroup.items.find(function (item) {
                    return item.uid == uanitem.uan;
                });

                var comboitemdesign = $scope.FGBundleDesigns.find(function (item) {
                    return item.sizegroup.items.find(function (fitem) {
                        return fitem.uid == barcode;
                    })
                });
                var uanproductitem = null;
                if (comboitemdesign && comboitemdesign != null) {
                    uanproductitem = comboitemdesign.sizegroup.items.find(function (fitem) {
                        return fitem.uid == barcode;
                    })
                    if (uanproductitem == undefined || uanproductitem == null) uanproductitem = {};
                }
                if ($scope.FGBundleSetitems == undefined || $scope.FGBundleSetitems == null) $scope.FGBundleSetitems = [];

                var newsetitem = {
                    'uan': uanitem.uan, 'barcode': barcode, 'itemproductid': comboitemdesign.productid,
                    'productid': uanitem.productid, 'config': exist, 'itemsizeid': uanproductitem.sizeid,
                    'itemcode': uanproductitem.code,
                    'designitemcode': combouan.code, 'sizeid': combouan.sizeid, 'mrp': combouan.mrp, 'qty': 1,
                    'sno': $scope.FGBundleSetitems.length + 1, 'rqty': uanitem.qty, 'designid': exist.code
                };
                $scope.FGBundleSetitems.push(newsetitem);
                $scope.FGBundle.comboitembarcode = "";
                $scope.SetItemConfig();
                $scope.ProcessComboItems(true);
                $scope.setFocusbomcode();
            }
            else {
                if (retry) {

                    var request =
                    {
                        Item:
                        {
                            'customerid': $scope.FGBundle.customerid,
                            'locationid': $scope.FGBundle.locationid,
                            'code': barcode,
                            'sono': $scope.FGBundle.sono,
                            'source': $scope.system_ismfgapplication ? 'UAN' : 'BARCODE'
                        }
                    };
                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "SalablegoodsService/SearchBarcode",
                        data: srequest
                    }).success(function (results) {
                        if (results.length == 1) {
                            //if (results[0].barcode == barcode) {

                            if ($scope.FGBundleSetitems == undefined || $scope.FGBundleSetitems == null) $scope.FGBundleSetitems = [];
                            barcode = results[0].barcode;
                            var exist = $scope.FGBundleSoitems.find(function (item) {
                                if (item.comboconfig.uanlist && item.comboconfig.uanlist.length > 0) {
                                    var uanitem = item.comboconfig.uanlist.find(function (uitem) {
                                        return uitem.uans.indexOf(barcode) >= 0;
                                    })
                                    return typeof (uanitem) != 'undefined' && uanitem != null;
                                }
                                return false;
                            })

                            var uanitem = null;

                            if ($scope.FGBundleSetitems.length > 0) {
                                uanitem = exist.comboconfig.uanlist.find(function (uitem) {
                                    return uitem.uan == $scope.FGBundleSetitems[0].uan &&
                                        uitem.uans.indexOf(barcode) >= 0;
                                });
                            }

                            if (uanitem == undefined || uanitem == null) {
                                uanitem = exist.comboconfig.uanlist.find(function (uitem) {
                                    return uitem.uans.indexOf(barcode) >= 0;
                                });
                            }

                            var combouan = exist.sizegroup.items.find(function (item) {
                                return item.uid == uanitem.uan;
                            });

                            var comboitemdesign = $scope.FGBundleDesigns.find(function (item) {
                                return item.sizegroup.items.find(function (fitem) {
                                    return fitem.uid == barcode;
                                })
                            });
                            var uanproductitem = null;
                            if (comboitemdesign && comboitemdesign != null) {
                                uanproductitem = comboitemdesign.sizegroup.items.find(function (fitem) {
                                    return fitem.uid == barcode;
                                })
                                if (uanproductitem == undefined || uanproductitem == null) uanproductitem = {};
                            }
                            if ($scope.FGBundleSetitems == undefined || $scope.FGBundleSetitems == null) $scope.FGBundleSetitems = [];

                            var newsetitem = {
                                'uan': uanitem.uan, 'barcode': barcode, 'itemproductid': comboitemdesign.productid,
                                'productid': uanitem.productid, 'config': exist, 'itemsizeid': uanproductitem.sizeid,
                                'itemcode': uanproductitem.code, 'soboxid': results.fgboxid, 'fgboxid': results.fgboxid,
                                'designitemcode': combouan.code, 'sizeid': combouan.sizeid, 'mrp': combouan.mrp, 'qty': 1,
                                'sno': $scope.FGBundleSetitems.length + 1, 'rqty': uanitem.qty, 'designid': exist.code
                            };


                            var existingsetitem = $scope.FGBundleSetitems.find(function (item) {
                                return item.barcode == barcode
                            });

                            if (existingsetitem && existingsetitem != null) {
                                existingsetitem.rqty++;
                            }
                            else {
                                //$scope.SearchByDesign(results[0].designid);
                                $scope.FGBundleSetitems.push(newsetitem);
                            }
                            $scope.FGBundle.comboitembarcode = "";
                            $scope.SetItemConfig();
                            $scope.ProcessComboItems(true);
                            $timeout(function () {
                                $scope.setFocusbomcode();
                            }, 100);
                        }
                    })
                }
                else {
                    $rootScope.Toaster.Info(" Given barcode not available in this system");
                    $scope.setFocusbomcode();
                }
            }
        }

        $scope.ProcessComboItems = function (retry) {
            var groups = [];
            var addedcombo = 0;
            var designgroup = [];
            $.each($scope.FGBundleSetitems, function (k, v) {
                var grp = groups.find(function (item) {
                    return item.uan == v.uan;
                });

                if (grp && grp != null) {
                    grp.items.push(v);
                }
                else {
                    grp = { 'uan': v.uan, 'config': v.config, 'rqty': v.config.comboconfig.qty, 'items': [v] };
                    groups.push(grp);
                }

                if (grp.items.length == grp.rqty) {
                    var pitem = grp.items.find(function (item) {
                        return item.rqty != item.qty;
                    });
                    if (pitem && pitem != null) {
                        return;
                    }
                    else {
                        $scope.PushComboSet(groups, grp);
                        addedcombo++;
                    }
                }

                var dg = designgroup.find(function (item) {
                    return item.setcode == v.config.code;
                });
                if (dg && dg != null) {
                    if (!dg.optional)
                        dg.optional = v.optional;
                    dg.list.push(v);
                }
                else {
                    dg = { 'setcode': v.config.code, 'qty': v.config.comboconfig.qty, 'optional': v.optional, 'list': [v] };
                    designgroup.push(dg);
                }
            });


            if ($scope.mixedbarcode &&
                $scope.FGBundleSetitems.length > 1 &&
                addedcombo == 0) {
                $.each(designgroup, function (k, v) {
                    if (v.list.length == v.qty && v.optional) {
                        var comboset = $scope.FGBundleSoitems.find(function (item) {
                            return item.code == v.setcode;
                        });
                        var optionalist = [];

                        $.each(v.list, function (kk, vv) {
                            if (vv.optional)
                                optionalist.push(vv);
                        });

                        var nonoptional = v.list.find(function (item) { return !item.optional; });

                        $.each(optionalist, function (kk, optional) {
                            var optional = v.list.find(function (item) { return item.optional; })
                            if (nonoptional && optional && nonoptional != null && optional != null) {
                                var uanset = comboset.comboconfig.uanlist.find(function (item) {
                                    return item.productid == optional.productid && item.uan == nonoptional.uan;
                                });

                                var combouan = comboset.sizegroup.items.find(function (item) {
                                    return item.uid == nonoptional.uan;
                                });

                                optional.designitemcode = combouan.code;
                                optional.sizeid = combouan.sizeid;
                                optional.designid = v.setcode;
                                optional.uan = nonoptional.uan;
                                optional.optional = false;
                            }
                        });

                        grp = { 'uan': nonoptional.uan, 'config': nonoptional.config, 'rqty': nonoptional.config.comboconfig.qty, 'items': v.list };
                        $scope.PushComboSet([grp], grp);
                    }
                })
            }

            $scope.SetTotalQty = 0;
            $.each($scope.FGBundleComboitems, function (k, v) {
                $scope.SetTotalQty += v.qty;
            })
        }

        $scope.PushComboSet = function (list, item) {
            list.splice(list.indexOf(item));
            if ($scope.FGBundleComboitems == undefined || $scope.FGBundleComboitems == null) $scope.FGBundleComboitems = [];

            var comboitem = item.config.sizegroup.items.find(function (sitem) {
                return sitem.uid == item.uan;
            });

            comboitem = (JSON.parse(JSON.stringify(comboitem)));
            comboitem.productid = item.config.productid;
            comboitem.items = [];
            comboitem.qty = 1;
            comboitem.sno = $scope.FGBundleComboitems.length + 1;
            comboitem.designid = item.id;
            comboitem.design = item.code;
            comboitem.soboxid = item.config.boxid;
            $.each(item.items, function (k, v) {
                $scope.FGBundleSetitems.splice($scope.FGBundleSetitems.indexOf(v));
                comboitem.items.push(v);
            });

            var exist = $scope.FGBundleComboitems.find(function (sitem) {
                return sitem.uid == comboitem.uid;
            });
            if (exist && exist != null) {
                exist.qty++;
                $.each(comboitem.items, function (k, v) {
                    exist.items.push(v);
                });
            }
            else {
                $scope.FGBundleComboitems.splice(0, 0, comboitem);
            }
            $scope.SetConfig();
            $scope.SetItemConfig();
        }

        $scope.ProductChanged = function () {

            var request = {
                "Item": { 'productid': $scope.FGBundle.productid, 'includedetail': false, 'onlycombo': true },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgdesignService/Search",
                data: srequest
            }).success(function (result) {
                $scope.Designs = result;
            }).error(function (result) {
            });
        }

        $scope.Add2Combo = function () {
            var design = $scope.Designs.find(function (item) {
                return item.id == $scope.FGBundle.designid;
            });
            if (design && design != null) {
                $scope.SearchByDesign(design.code);
            }
        }

        $scope.SearchByDesign = function (design, designlist, callback) {

            if (design && design == '*') {
                $scope.FGBundleSoitems = [];
                $scope.FGBundleDesigns = [];
                $scope.SOConfig();
                $scope.sogrid.UpdateData($scope.FGBundleSoitems, '_id');
                $scope.FGBundle.combodesign = "";
                return;
            }

            var request = {
                "Item": { 'code': design, 'codes': designlist },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgdesignService/SearchCombo",
                data: srequest
            }).success(function (result) {

                if (typeof ($scope.FGBundleSoitems) == 'undefined' || $scope.FGBundleSoitems == null) $scope.FGBundleSoitems = [];

                var combodesigns = [];
                $.each(result, function (k, v) {
                    if (v.comboconfig && v.comboconfig != null && v.comboconfig.qty > 0) {
                        combodesigns.push(v);
                    }
                    else {
                        $scope.FGBundleDesigns.push(v);
                    }
                });

                $.each(combodesigns, function (k, v) {
                    var exist = $scope.FGBundleSoitems.find(function (item) {
                        return item.code == v.code;
                    })
                    if (exist && exist != null)
                        exist.setqty = exist.setqty + 1;
                    else {
                        v.sno = $scope.FGBundleSoitems.length + 1;
                        v.setqty = 1;
                        $scope.FGBundleSoitems.push(v);
                    }
                })
                $scope.SOConfig();
                $scope.sogrid.UpdateData($scope.FGBundleSoitems, '_id');
                if (callback && callback != null)
                    callback($scope.FGBundleSoitems);

            }).error(function (result) {
            });
        }

        $scope.SOConfig = function () {
            if ($scope.sogrid != null)
                $scope.sogrid.destroy();

            var igrid = new InfyGrid('soitemsContainer', '100px', '100%', true, true, true, 300);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddReferenceColumn('productid', 'Product', 130, $scope.ProductList);
            igrid.AddSimpleColumn('code', 'Design', 100);
            igrid.AddColumn(
                {
                    id: "setqty", name: "Set", field: "setqty",
                    width: 50, cssClass: "cell-title", 'sortable': true
                }
            );
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command != 'Delete') return;
                $scope.$apply(function () {
                    $scope.FGBundleSoitems.splice($scope.FGBundleSoitems.indexOf(item), 1);
                    $scope.sogrid.UpdateData($scope.FGBundleSoitems, '_id');
                });
            });

            $scope.sogrid = igrid;
            if (typeof ($scope.FGBundleSoitems) == 'undefined' || $scope.FGBundleSoitems == null) $scope.FGBundleSoitems = [];
            $scope.sogrid.UpdateData($scope.FGBundleSoitems, '_id');


        }

        $scope.PackItemGridConfig = function (isprint) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridItemContainer', '100px', '100%', true, true, true, 40);
            igrid.options.showHeaderRow = true;
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddSimpleColumn('sno', 'S.No', 50);
            if ($scope.system_ismfgapplication)
                igrid.AddSimpleColumn('itemdesigncode', 'Design', 100);

            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            igrid.AddReferenceColumn('productid', 'Product', $scope.ShowComboitems ? 130 : 150, $scope.ProductList);
            igrid.AddReferenceColumn('colourid', 'Colour', $scope.ShowComboitems ? 80 : 120, $scope.AllColorList);
            igrid.AddReferenceColumn('sizeid', 'Size', $scope.ShowComboitems ? 60 : 120, $scope.SizeList);

            if (isprint && !$scope.IsUANBased) {
                var column = igrid.AddColumn(
                    {
                        id: "qty", name: "Print", field: "qty",
                        width: 100, cssClass: "cell-title", editor: Slick.Editors.Text,
                        'sortable': true
                    });
                column.total = true;
                column.cssClass = 'rightalign';
            }
            else {
                igrid.AddFormatedColumn('oqty', 'Order', 60, 'N2', 2).total = true;
                var column = igrid.AddColumn({
                    id: "qty",
                    name: "Scanned",
                    field: "qty",
                    width: 80,
                    cssClass: "cell-title",
                    editor: Slick.Editors.Text,
                    'sortable': true
                });
                column.total = true;
                column.cssClass = 'rightalign';
            }

            if (!$scope.ShowComboitems) {

                column = igrid.AddCustomColumn('price', 'Price', 60, function (item, col) {
                    var price = item.price;
                    if (item.fgboxid != item.soboxid) {
                        var fgbox = col.dataSource.find(function (bitem) {
                            return bitem.id == item.fgboxid;
                        });

                        var sobox = col.dataSource.find(function (bitem) {
                            return bitem.id == item.soboxid;
                        });

                        if (fgbox && fgbox != null) {
                            price -= fgbox.remove;
                        }

                        if (sobox && sobox != null) {
                            price += sobox.add;
                        }
                        item.fprice = price;
                    }
                    return price;
                });
                column.dataSource = $scope.BoxList;
                column.cssClass = 'rightalign';

                var column = igrid.AddFormatedColumn('mrp', 'MRP', 60, 'N2', 2);
                column.editor = Slick.Editors.Text;
                
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
            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command != 'Delete') return;
                $scope.$apply(function () {
                    $scope.FGBundleItems.splice($scope.FGBundleItems.indexOf(item), 1);
                    $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
                    $scope.LocalContext.Post('LastSavedFGBundleItems', $scope.FGBundleItems, false);
                });

            });
            $scope.igrid = igrid;
            if ($scope.IsUANBased || typeof (isprint) == 'undefined' || !isprint) {
                $scope.igrid.setCellCSS('igridmediumrows');
                $scope.igrid.dataView.getItemMetadata = function (row) {
                    if ($scope.SaleOrderItems && $scope.SaleOrderItems != null && $scope.SaleOrderItems.length > 0) {
                        var rowdata = igrid.grid.getData().getItem(row);
                        if (rowdata.oqty == 0) {
                            rowdata.style = 'igridbigrowswithred';
                        }
                        else {
                            if (rowdata.oqty == rowdata.qty) {
                                rowdata.style = 'igridbigrowswithdarkgreen';
                            }
                            else {
                                rowdata.style = 'igridbigrowswithgreen';
                            }
                        }
                        return { 'cssClasses': rowdata.style };
                    }
                    return { 'cssClasses': '' };
                }
            }
            $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
        }


        $scope.CreateComboItem = function () {
            var dialog = ngDialog.open({
                template: 'ComboCreator',
                data: [$scope, $scope.SizeList, $scope.FGBundleSoitems],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.AllSizeList = $scope.ngDialogData[1];
                    $scope.DesignList = $scope.ngDialogData[2];
                    $scope.SizeList = [];
                    $scope.DesignItemList = [];
                    $scope.designid = 0;
                    $scope.sizeid = 0;
                    $scope.ComboItem = null;
                    $scope.SelectedDesign = null;
                    $scope.designitemcode = "";
                    $scope.designchanged = function () {
                        $scope.ComboItem = null;
                        $scope.DesignItemList = [];
                        $scope.SelectedDesign = null;
                        $scope.SelectedDesign = $scope.DesignList.find(function (item) {
                            return item.id == $scope.designid;
                        });
                        if ($scope.SelectedDesign && $scope.SelectedDesign != null) {
                            var designItemlist = [];
                            $.each($scope.SelectedDesign.sizegroup.items, function (k, v) {
                                var existingdesign = designItemlist.find(function (item) {
                                    return item.code == v.code
                                });
                                if (existingdesign == undefined || existingdesign == null) {
                                    designItemlist.push(v);
                                }
                            });
                            $scope.DesignItemList = designItemlist;
                        }
                    }

                    $scope.sizeselected = function () {
                        var comboitem = $scope.SelectedDesign.sizegroup.items.find(function (item) {
                            return item.code == $scope.designitemcode && item.sizeid == $scope.sizeid;
                        });
                        comboitem = (JSON.parse(JSON.stringify(comboitem)));
                        comboitem.productid = $scope.SelectedDesign.productid;
                        comboitem.items = [];
                        comboitem.qty = 1;
                        comboitem.designid = $scope.SelectedDesign.id;
                        comboitem.design = $scope.SelectedDesign.code;
                        comboitem.soboxid = $scope.SelectedDesign.boxid;
                        $scope.ComboItem = comboitem;
                    }

                    $scope.designitemchanged = function () {
                        $scope.SizeList = [];
                        $scope.ComboItem = null;
                        if ($scope.SelectedDesign && $scope.SelectedDesign != null) {

                            var sizelist = [];
                            $.each($scope.SelectedDesign.sizegroup.items, function (k, v) {

                                if (v.code == $scope.designitemcode) {
                                    var existingdesign = sizelist.find(function (item) {
                                        return item.sizeid == v.sizeid
                                    });
                                    if (existingdesign == undefined || existingdesign == null) {
                                        sizelist.push(v);
                                    }
                                }
                            });
                            $.each(sizelist, function (k, v) {
                                var sizeitem = $scope.AllSizeList.find(function (item) {
                                    return item.id == v.sizeid;
                                });
                                if (sizeitem && sizeitem != null)
                                    $scope.SizeList.push(sizeitem);
                            });
                        }
                    }

                    $scope.Selected = function () {
                        $scope.ComboItem.qty = parseFloat($scope.qty);
                        if (isNaN($scope.ComboItem.qty)) $scope.ComboItem.qty = 1;
                        $scope.closeThisDialog($scope.ComboItem);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        comboitem = data.value;
                        comboitem.sno = $scope.FGBundleComboitems.length + 1;

                        $.each($scope.FGBundleSetitems, function (k, v) {
                            comboitem.items.push(v);
                        });

                        $scope.FGBundleSetitems = [];

                        var exist = $scope.FGBundleComboitems.find(function (sitem) {
                            return sitem.uid == comboitem.uid;
                        });
                        if (exist && exist != null) {
                            exist.qty += comboitem.qty;
                            $.each(comboitem.items, function (k, v) {
                                exist.items.push(v);
                            });
                        }
                        else {
                            $scope.FGBundleComboitems.push(comboitem);
                        }
                        $scope.SetConfig();
                        $scope.SetItemConfig();
                        $scope.FGBundleItems = [];
                        $scope.PackItemGridConfig();
                    }
                }
            });
        }

        $scope.pgrid = null;
        $scope.SearchPendingBills = function () {


            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PackingslipService/Search",
                data: srequest
            }).success(function (result) {

                

                if ($scope.pgrid != null)
                    $scope.pgrid.destroy();

                var igrid = new InfyGrid('pendingpackslipsContainer', '100px', '100%', true, true, true, 320);
                igrid.options.showHeaderRow = true;
                igrid.AddSimpleColumn('packingno', 'P.No', 80);
                igrid.AddFormatedColumn('packedon', 'Packed On', 80, 'dd-MM-yyyy');
                igrid.AddReferenceColumn('packedby', 'Packed By', 130, $scope.Employee);
                if ($scope.SearchCondition.searchmode == 3)
                    igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                else
                    igrid.AddSimpleColumn('billno', 'Bill No', 100);
                igrid.Initialize();
                igrid.currentdata = result;
                igrid.grid.onClick.subscribe(function (e) {
                    var cell = igrid.grid.getCellFromEvent(e);
                    var item = igrid.grid.getDataItem(cell.row);
                    var command = igrid.grid.getColumns()[cell.cell].id;
                    if (command != 'Delete') return;

                    request = {
                        "Item": item,
                        "securitytoken": "String content"
                    }

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "PackingslipService/Delete",
                        data: srequest
                    }).success(function (result) {
                        $scope.pgrid.currentdata.splice($scope.pgrid.currentdata.indexOf(item), 1);
                        $scope.pgrid.UpdateData(igrid.currentdata, '_id');
                    });
                });

                $scope.pgrid = igrid;
                $scope.pgrid.UpdateData(result, '_id');

            }).error(function (result) {
            });

        }


        $scope.exporttoExcel = function (infygrid, filename, subtitle) {
            var title = $rootScope.UserInfo.CompanyName;

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

        $scope.PrintItems = function () {

            var requestitem = {
                'list': [],
                'reportid': 0
            }

            $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                requestitem.list.push({ 'designid': v.designid, 'uid': v.barcode, 'pqty': v.qty, 'mrp': v.mrp });
            });

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

        $scope.PrintBarcode = function () {

            var requestitem = {
                'list': [],
                'reportid': 0
            }

            $.each($scope.setgrid.GetSelectedRows(), function (k, v) {
                requestitem.list.push({ 'designid': v.designid, 'uid': v.barcode, 'pqty': v.qty ,'mrp':v.mrp });
            });

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



        $scope.SetConfig = function () {
            if ($scope.setgrid != null)
                $scope.setgrid.destroy();

            var igrid = new InfyGrid('gridSetContainer', '100px', '100%', true, true, true, 320);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 50);
            igrid.AddSimpleColumn('code', 'Combo Design', $scope.ShowBillItems ? 100 : 200);
            igrid.AddSimpleColumn('uid', 'Combo Barcode', 80);
            igrid.AddReferenceColumn('productid', 'Product', $scope.ShowBillItems ? 100 : 200, $scope.ProductList);
            igrid.AddReferenceColumn('soboxid', 'Box', 100, $scope.BoxList);
            igrid.AddReferenceColumn('sizeid', 'Size', $scope.ShowBillItems ? 60 : 120, $scope.SizeList);
            igrid.AddFormatedColumn('qty', 'Qty', 50, 'N2', 2);

            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command != 'Delete') return;
                $scope.$apply(function () {
                    $scope.FGBundleComboitems.splice($scope.FGBundleComboitems.indexOf(item), 1);
                    $scope.setgrid.UpdateData($scope.FGBundleComboitems, '_id');
                });

            });

            $scope.setgrid = igrid;
            if ($scope.FGBundleComboitems == undefined || $scope.FGBundleComboitems == null) $scope.FGBundleComboitems = [];
            $scope.setgrid.UpdateData($scope.FGBundleComboitems, '_id');


        }

        $scope.SetItemConfig = function () {

            if ($scope.setitemgrid != null)
                $scope.setitemgrid.destroy();

            var igrid = new InfyGrid('gridSetItemContainer', '170px', '100%', true, false, true);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 50);
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddSimpleColumn('designitemcode', 'Design', 120);
            igrid.AddReferenceColumn('productid', 'Combo Product', 100, $scope.ProductList);
            igrid.AddSimpleColumn('uan', 'Combo Barcode', 90);
            igrid.AddReferenceColumn('sizeid', 'Combo Size', 120, $scope.SizeList);

            igrid.AddSimpleColumn('barcode', 'Barcode', 90);
            igrid.AddSimpleColumn('itemcode', 'Code', 100);
            igrid.AddReferenceColumn('itemproductid', 'Product', 60, $scope.ProductList);
            igrid.AddReferenceColumn('itemsizeid', 'Size', 60, $scope.SizeList);
            igrid.AddFormatedColumn('qty', 'Qty', 60, 'N2', 2);


            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command != 'Delete') return;
                $scope.$apply(function () {
                    $scope.FGBundleSetitems.splice($scope.FGBundleSetitems.indexOf(item), 1);
                    $scope.setitemgrid.UpdateData($scope.FGBundleSetitems, '_id');
                });
            });

            $scope.setitemgrid = igrid;
            $scope.setitemgrid.setCellCSS('igridmediumrows');
            $scope.setitemgrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData().getItem(row);
                rowdata.style = '';
                if (!rowdata.optional) {
                    rowdata.style = 'igridbigrowswithred';
                }
                return { 'cssClasses': rowdata.style };
            }


            if ($scope.FGBundleSetitems == undefined || $scope.FGBundleSetitems == null) $scope.FGBundleSetitems = [];
            $scope.setitemgrid.UpdateData($scope.FGBundleSetitems, '_id');
        }

        $scope.ManageList = function (item) {
            $scope.FGBundleItems.splice($scope.FGBundleItems.indexOf(item), 1);
            $.each($scope.FGBundleItems, function (k, v) {
                v.sno = k + 1;
            });
            $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
            $scope.LocalContext.Post('LastSavedFGBundleItems', $scope.FGBundleItems, false);
        };

        $scope.PrintList = [];
        $scope.ShowPrint = true;
        $scope.totalqty = 0;



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

        $scope.LastSaved = function () {

            if ($scope.FGBundle.companyid == null || $scope.FGBundle.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select company name");
                return;
            }

            if ($scope.FGBundle.locationid == null || $scope.FGBundle.locationid == 0) {
                $scope['setFocuslocationid']();
                $rootScope.Toaster.Info("Please select from location");
                return;
            }

            if ($scope.SaleOrderItems == undefined || $scope.SaleOrderItems == null || $scope.SaleOrderItems.length == 0) {
                $rootScope.Toaster.Info("Please select the salesorder no");
                return;
            }
            var value = $scope.LocalContext.Get('LastSavedFGBundleItems', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.FGBundleItems = value;
            $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
        };

        $scope.currentcompanyid = 0;

        $scope.qEncodeURIComponent = function (value) {
            if (value.indexOf('.') >= 0)
                value = value.replaceAll('.', '~dot~');
            if (value.indexOf('/') >= 0)
                value = value.replaceAll('/', '~sls~');
            return encodeURIComponent(value);
        }

        $scope.SaleOrderItems = [];
        $scope.ComboDesigns = [];
        $scope.SearchSaleOrder = function (order, loadasitems) {
            $scope.FGBundle.customerid = 0;
            if (order && order != null && order != '' && $scope.FGBundle.locationid && $scope.FGBundle.locationid > 0) {
                restService.Get('SalesorderService/GetSaleOrder/' + $scope.FGBundle.locationid + '/' + $scope.qEncodeURIComponent(order), true).then(function (results) {
                    if (results.length == 0) {
                        $rootScope.ShowAlert('Sales Order', "Can't find the given sales order - " + order);
                    }
                    else {
                        $scope.SaleOrderItems = results;
                        $scope.FGBundle.customerid = results[0].customerid;
                        $scope.CustomerChanged();
                        var designcodes = [];
                        $.each(results, function (k, v) {
                            if (v.pendingqty > 0) {
                                if (designcodes.indexOf(v.designid) < 0) {
                                    designcodes.push(v.designid);
                                }
                            }
                        });
                        $scope.SearchByDesign('', designcodes, function (items) {
                            if (typeof (callback) != 'undefined')
                                callback();
                            $scope.setFocusbillbarcode();
                        });
                        if (loadasitems) {
                            $scope.LoadSalesOrderItem2Pack();
                        }
                    }
                });
            }
            else {
                $rootScope.ShowAlert('Sales Order', "Please select location,company & Sales order No");
            }
        }

        $scope.LoadSalesOrderItem2Pack = function () {
            $scope.FGBundleItems = [];
            $.each($scope.SaleOrderItems, function (k, v) {
                if (!$scope.IsUANBased || (v.pendingqty && v.pendingqty > 0)) {
                    var scanneditem = {
                        'itemdesigncode': v.designitemcode, 'barcode': v.uid, 'stockid': 0,
                        'productid': v.productid,
                        'colourid': v.colourid, 'sizeid': v.sizeid,
                        'price': v.price,
                        'qty': v.qty,
                        'oqty': v.qty,
                        'mrp': v.mrp,
                        'pqty': v.pqty
                    }
                    scanneditem.sno = $scope.FGBundleItems.length + 1;
                    $scope.FGBundleItems.push(scanneditem);
                }
            });
            $scope.ActionMode == 1;
            $scope.ShowBillItems = $scope.ActionMode == 3 || $scope.ActionMode == 1;
            $scope.ShowComboitems = $scope.ActionMode == 3 || $scope.ActionMode == 2;

            $timeout(function () {
                $scope.PackItemGridConfig(true);
                $scope.FGBundle.cansave = false;
                if ($scope.FGBundleItems.length == 0) {
                    $rootScope.Toaster.Info("No Pending SO Available");
                }
            }, 10);
        }


        $scope.GenerateCombo = function () {
            if ($scope.FGBundle == undefined || $scope.FGBundle.id == undefined ||
                $scope.FGBundle.id == null || $scope.FGBundle.id == '' || $scope.FGBundle.id < 0) {
                $rootScope.Toaster.Info(" Please save the packing slip against a customer before generating combo set");
                return;
            }

            var request =
            {
                Item: {
                    'companyid': $scope.FGBundle.companyid,
                    'locationid': $scope.FGBundle.locationid,
                    'poid': $scope.FGBundle.id,
                    'pono': $scope.FGBundle.packingno,
                    'comboitems': []
                }
            };

            $.each($scope.FGBundleComboitems, function (ck, cv) {
                var citem = {
                    'uid': cv.uid, 'qty': cv.qty, 'created': cv.created,
                    'id': cv.id, 'items': []
                };
                request.Item.comboitems.push(citem);
                $.each(cv.items, function (ik, iv) {
                    citem.items.push({
                        'barcode': iv.barcode,
                        'qty': iv.qty,
                        'id': iv.id
                    });
                })
            });

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PackingslipService/GenerateCombo",
                data: srequest
            }).success(function (allresults) {

                $.each(allresults, function (k, v) {
                    var barcode = v.barcode;
                    var results = [v];
                    var scanneditem = $scope.FGBundleItems.find(function (item) {
                        return item.barcode == barcode
                    })

                    var createdqty = $scope.FGBundleComboitems.find(function (item) {
                        return item.uid == v.barcode;
                    });

                    if (scanneditem && scanneditem != null) {
                        scanneditem.qty += createdqty.qty;
                    }
                    else {


                        var scanneditem = {
                            'itemdesigncode': results[0].groupkey, 'barcode': barcode, 'stockid': results[0].stockid,
                            'productid': results[0].productid, 'colourid': results[0].colourid, 'sizeid': results[0].sizeid,
                            'price': results[0].wsrateextax, 'qty': createdqty.qty, 'oqty': 0, 'soboxid': results[0].soboxid
                        }

                        var exist = $scope.FGBundleSoitems.find(function (item) {
                            if (item.sizegroup.items && item.sizegroup.items.length > 0) {
                                var uanitem = item.sizegroup.items.find(function (uitem) {
                                    return uitem.uid == barcode;
                                })
                                return typeof (uanitem) != 'undefined' && uanitem != null;
                            }
                            return false;
                        })

                        if (exist && exist != null) {
                            var uanitem = exist.sizegroup.items.find(function (uitem) {
                                return uitem.uid == barcode;
                            })
                            scanneditem.mrp = uanitem.mrp;
                            scanneditem.soboxid = exist.boxid;
                            if ($scope.FGBundle.customerpricesource == 1) {
                                scanneditem.price = uanitem.wsrate;
                            } else if ($scope.FGBundle.customerpricesource == 2) {
                                scanneditem.price = uanitem.dealerprice;
                            }
                            else {
                                scanneditem.price = uanitem.salerate;
                            }
                        }

                        if ($scope.SaleOrderItems && $scope.SaleOrderItems.length > 0) {
                            var salesorderitem = $scope.SaleOrderItems.find(function (item) {
                                return item.uid == barcode;
                            });
                            if (salesorderitem && salesorderitem != null) {
                                $.each($scope.SaleOrderItems, function (k, v) {
                                    if (v.uid == barcode) {
                                        scanneditem.oqty += v.pendingqty;
                                    }
                                })
                            }
                        }

                        scanneditem.fprice = scanneditem.price;
                        scanneditem.sno = $scope.FGBundleItems.length + 1;
                        $scope.FGBundleItems.splice(0, 0, scanneditem);
                    }
                });

                $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
                $scope.FGBundle.billbarcode = "";
                $scope.FGBundleComboitems = [];
                $scope.setgrid.UpdateData($scope.FGBundleComboitems, '_id');
                $scope.LocalContext.Post('LastSavedFGBundleItems', $scope.FGBundleItems, false);
                $scope.setFocusbomcode();
                $scope.SetTotalQty = 0;
            });
        }

        $scope.system_ismfgapplication = false;
        $scope.SearchBarcode = function (barcode) {
            if (barcode == '*') {
                $scope.FGBundleItems = [];
                $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
                $scope.LocalContext.Post('LastSavedFGBundleItems', $scope.FGBundleItems, false);
                $scope.FGBundle.billbarcode = "";
                return;
            }


            if (typeof ($scope.FGBundle.locationid) == 'undefined' ||
                $scope.FGBundle.locationid == null ||
                $scope.FGBundle.locationid == '') {
                return;
            }


            if ($scope.FGBundleItems.length > 0) {
                var scanneditem = $scope.FGBundleItems.find(function (item) {
                    return item.barcode == barcode
                })
                if (scanneditem && scanneditem != null) {
                    if (scanneditem.oqty && scanneditem.oqty > 0 &&
                        scanneditem.oqty < scanneditem.qty + 1) {
                        $.alert(barcode + " can't exceed the order qty of " + scanneditem.qty);
                    }
                    else {
                        scanneditem.qty += 1;
                    }
                    $scope.FGBundle.billbarcode = "";
                    $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
                    $scope.LocalContext.Post('LastSavedFGBundleItems', $scope.FGBundleItems, false);
                    return;
                }
            }

            var request =
            {
                Item:
                {
                    'customerid': $scope.FGBundle.customerid,
                    'locationid': $scope.FGBundle.locationid,
                    'code': barcode,
                    'sono': $scope.FGBundle.sono,
                    'source': $scope.system_ismfgapplication ? 'UAN' : 'BARCODE'
                }
            };
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SalablegoodsService/SearchBarcode",
                data: srequest
            }).success(function (results) {
                if (results.length == 1) {
                    if ($scope.FGBundleItems.length == 0) {
                        $scope.PackItemGridConfig();
                    }
                    var scanneditem = $scope.FGBundleItems.find(function (item) {
                        return item.barcode == barcode
                    })
                    if (scanneditem && scanneditem != null) {
                        if (scanneditem.oqty && scanneditem.oqty > 0 &&
                            scanneditem.oqty < scanneditem.qty + 1) {
                            $.alert(barcode + " can't exceed the order qty of " + scanneditem.qty);
                            return;
                        }
                        else {
                            scanneditem.qty += 1;
                        }
                        $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
                        $scope.LocalContext.Post('LastSavedFGBundleItems', $scope.FGBundleItems, false);
                    }
                    else {
                        var scanneditem = {
                            'itemdesigncode': results[0].groupkey, 'barcode': barcode, 'stockid': results[0].stockid,
                            'productid': results[0].productid, 'colourid': results[0].colourid, 'sizeid': results[0].sizeid,
                            'price': results[0].wsrate, 'qty': 1, 'oqty': 0, 'mrp': results[0].mrp, 'soboxid': results[0].fgboxid,
                            'fgboxid': results[0].fgboxid
                        }

                        if ($scope.FGBundle.customerpricesource == 1) {
                            scanneditem.price = results[0].wsrate;
                        } else if ($scope.FGBundle.customerpricesource == 2) {
                            scanneditem.price = results[0].dealerprice;
                        }
                        else {
                            scanneditem.price = results[0].price;
                        }

                        if ($scope.SaleOrderItems && $scope.SaleOrderItems.length > 0) {
                            var salesorderitem = $scope.SaleOrderItems.find(function (item) {
                                return item.uid == barcode;
                            });
                            if (salesorderitem && salesorderitem != null) {
                                $.each($scope.SaleOrderItems, function (k, v) {
                                    if (v.uid == barcode) {
                                        scanneditem.oqty += v.pendingqty;
                                    }
                                })
                                if (scanneditem.oqty < scanneditem.qty) {
                                    $.alert(barcode + " can't exceed the order qty of " + scanneditem.oqty);
                                    return;
                                }
                            }
                            else {
                                $.alert(barcode + " doesn't have sales orders");
                                return;
                            }
                        }
                        scanneditem.sno = $scope.FGBundleItems.length + 1;
                        $scope.FGBundleItems.splice(0, 0, scanneditem);
                    }
                    $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
                    $scope.LocalContext.Post('LastSavedFGBundleItems', $scope.FGBundleItems, false);
                    $scope.FGBundle.billbarcode = "";
                    $scope.setFocusbillbarcode();
                }
            }).error(function (result) {
            });
        }

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.FGBundle = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.Clear = function () {
            $scope.FGBundle = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchResults = [];
            $scope.FGBundleItems = [];
            $scope.TotalAccepted = 0;
            $scope.TotalCounts = 0;
            $scope.PackItemGridConfig();
            $scope.SetConfig();
            $scope.SOConfig();
            $scope.FGBundle.cansave = true;
        };

        $scope.Save = function () {
            var iserror = false;

            if ($scope.FGBundle.packedon == null) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info(" Please select date");
                return;
            }
            if ($scope.FGBundle.companyid == null || $scope.FGBundle.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select company name");
                return;
            }

            if ($scope.FGBundle.packedby == null || $scope.FGBundle.packedby == 0) {
                $scope['setFocuspackedby']();
                $rootScope.Toaster.Info("Please select packed by");
                return;
            }
            if ($scope.FGBundle.locationid == null || $scope.FGBundle.locationid == 0) {
                $scope['setFocuslocationid']();
                $rootScope.Toaster.Info("Please select from location");
                return;
            }

            $scope.FGBundle.barcodes = [];
            $.each($scope.FGBundleItems, function (k, v) {
                $scope.FGBundle.barcodes.push(v);
            })

            request = {
                "Item": $scope.FGBundle,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PackingslipService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $scope.LocalContext.Post('LastSavedFGBundleItems', null, false);
                    $rootScope.Toaster.Info("Packing Slip - " + result.packingno + " Saved Successfully.!");
                    if ($scope.ActionMode == 2) {
                        $scope.LoadPacking(result);
                        $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
                    }
                    else {
                        $scope.LastFGBundle = result;
                        $scope.FGBundle = (JSON.parse(JSON.stringify($scope.Entity)));
                        $scope.FGBundleItems = [];
                        $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
                    }
                }
                else {
                    $rootScope.Toaster.Error(result.errormessage);
                }

            }).error(function (result) {
            });
        }


        $scope.LoadPacking = function (pack) {
            $scope.selectedcount = 0;
            $scope.FGBundleItems = [];
            $.each(pack.barcodes, function (k, v) {
                $scope.FGBundleItems.push(v);
                v.sno = k + 1;
            })
            pack.barcodes = [];
            $scope.FGBundle = pack;
            $scope.FGBundle.code = pack.packingno;
            $scope.LastFGBundle = null;
            $scope.PackItemGridConfig(false);
        }

        $scope.UpdateMrp = function () {
            $rootScope.Prompt('M.R.P', 'MRP', function (code) {
                
                if (typeof (code) != 'undefined' && code.length > 0) {
                    var mrp = parseFloat(code.toString());
                    if (isNaN(mrp)) return;
                    $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                        v.mrp = mrp;
                    })
                    $scope.igrid.UpdateData($scope.FGBundleItems, '_id');
                    $scope.LocalContext.Post('LastSavedFGBundleItems', $scope.FGBundleItems, false);
                }

            })
        }

        $scope.SearchByCode = function (id) {

            //if (typeof ($scope.FGBundle.locationid) == 'undefined' ||
            //    $scope.FGBundle.locationid == null ||
            //    $scope.FGBundle.locationid == '') {
            //    return;
            //}

            //if (typeof ($scope.FGBundle.code) == 'undefined' ||
            //    $scope.FGBundle.code == null ||
            //    $scope.FGBundle.code == '') {
            //    if (typeof (id) == 'undefined')
            //        return;
            //}

            request = {
                "Item": {
                    'code': $scope.FGBundle.code,
                    'id': id,
                    'fromwarehouseoutward': true
                },
                "securitytoken": "String content"
            }
            var locationid = '0';
            if ($scope.FGBundle.locationid && $scope.FGBundle.locationid>0)
                locationid = $scope.FGBundle.locationid;
            var srequest = JSON.stringify(request);
            $http({
                method: 'GET',
                url: "PackingslipService/SearchPackingNo/" + locationid + '/' + $scope.qEncodeURIComponent($scope.FGBundle.code)
            }).success(function (result) {
                if (result && result != null && result.id > 0) {
                    $scope.LoadPacking(result);
                    $scope.CustomerChanged();
                    $scope.LastFGBundle = result;
                }
                else {
                    $scope.FGBundle.statusdisplay = 'Not Found!';
                }
            }).error(function (result) {
            });
        }

        $scope.qEncodeURIComponent = function (value) {
            if (value.indexOf('.') >= 0)
                value = value.replaceAll('.', '~dot~');
            if (value.indexOf('/') >= 0)
                value = value.replaceAll('/', '~sls~');
            return encodeURIComponent(value);
        }


        $scope.UpdateBox = function (id) {

            $http({
                method: 'GET',
                url: "PackingslipService/UpdateBox/" + id
            }).success(function (result) {
                if (result && result != null && result.id > 0) {
                    $scope.LoadPacking(result);
                    $scope.CustomerChanged();
                }
            }).error(function (result) {
            });
        }


        $scope.Clear();
        $scope.BoxList = [];
        $scope.PrintList = [];
        restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
            $.each(results, function (k, v) {
                if (v.additionalinfo && v.additionalinfo.paytype == 'BOX') {
                    $scope.BoxList.push({ 'id': v.id, 'name': v.name, 'add': v.additionalinfo.maxlimit, 'remove': v.additionalinfo.billdiscount });
                }
            })
        });

        restService.Get('ReferencelistService/PRINTTEMPLATE/PACKINGSLIP', true).then(function (results) {
            $scope.PrintList = results;
        });

        $scope.ShowBoxChargesDialog = function () {
            var seletedRows = $scope.igrid.GetSelectedRows();
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
                            $scope.PackItemGridConfig(false);
                        });
                    }
                }
            });
        }

        $scope.Appsetting_Company = false;
        $scope.IsUANBased = false;
        $scope.LoadAppsetting = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadAppsetting, 100);
            }
            else {
                $scope.system_ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication;
                $scope.IsUANBased = $rootScope.UserInfo.appsettings.mfg_mode == 5;
                $scope.IsSampleBased = $rootScope.UserInfo.appsettings.mfg_mode == 6;
                if (!$scope.IsSampleBased) {
                    if ($scope.IsUANBased)
                        $scope.SelectOption(1);
                    else
                        $scope.PackItemGridConfig();
                }
            }
        }

        $scope.PrintSlip = function (printtemplate,id) {
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "packingslip.pdf";
                link.href = 'PackingslipService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + printtemplate + '/' + id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }


        $timeout(function () {
            $scope.LoadAppsetting();
            $scope.SOConfig();
            $scope.SetConfig();
            $scope.SetItemConfig();
        }, 10);
        $scope.CreateNew = function () {
            $scope.FGBundle.packedon = new Date();
        }();
    }]);
