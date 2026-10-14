
var app = angular.module('InfinityLab');

app.controller('BOMPurchaseReturnController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', 'restService', '$timeout', 'Excel', 'ngDialog', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, restService, $timeout, Excel, ngDialog, $stateParams) {

        $scope.Navigation = $rootScope.GetNavigation();
        Number.prototype.round = function (places) {
            return +(Math.round(this + "e+" + places) + "e-" + places);
        }

        $scope.GetNumber = function (value) {
            value = parseFloat(value);
            if (typeof (value) == 'undefined' || value == null || isNaN(value))
                return 0;
            return value;
        };

        $scope.discountchanged = function (item, current) {
            if (current) {
                item.discountperitem = (item.discount / $scope.GetNumber(item.qty)).round(2);
                item.tax = ($scope.GetNumber(item.rate - (item.discount / $scope.GetNumber(item.qty)))
                    * $scope.GetNumber(item.qty) * item.taxpercentage / 100).round(2);
                item.amount = ((item.rate) * $scope.GetNumber(item.qty)).round(2);
                item.discountpercentage = (item.discount / item.amount * 100).round(2);
            }
            else {
                item.newdiscountperitem = (item.newdiscount / $scope.GetNumber(item.newqty)).round(2);
                item.newtax = ($scope.GetNumber(item.newrate - (item.newdiscount / $scope.GetNumber(item.newqty)))
                    * $scope.GetNumber(item.newqty) * item.newtaxpercentage / 100).round(2);
                item.newamount = ((item.newrate) * $scope.GetNumber(item.newqty)).round(2);
                item.newdiscountpercentage = (item.newdiscount / item.newamount * 100).round(2);
            }
        }

        $scope.taxpercentagechanged = function (item, current) {
            if (current) {
                item.discountperitem = (item.rate * item.discountpercentage / 100).round(2);
                item.discount = (item.discountperitem * $scope.GetNumber(item.qty)).round(2);
                item.tax = ($scope.GetNumber(item.rate - (item.discount / $scope.GetNumber(item.qty)))
                    * $scope.GetNumber(item.qty) * item.taxpercentage / 100).round(2);
                item.amount = ((item.rate) * $scope.GetNumber(item.qty)).round(2);
            }
            else {
                item.newdiscountperitem = (item.newrate * item.newdiscountpercentage / 100).round(2);
                item.newdiscount = (item.newdiscountperitem * $scope.GetNumber(item.newqty)).round(2);
                item.newtax = ($scope.GetNumber(item.newrate - (item.newdiscount / $scope.GetNumber(item.newqty)))
                    * $scope.GetNumber(item.newqty) * item.newtaxpercentage / 100).round(2);
                item.newamount = ((item.newrate) * $scope.GetNumber(item.newqty)).round(2);
            }
        }

        $scope.BOMCategories = restService.Get('ReferencelistService/FULL-BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategories = results;
        });
        $scope.BOMMaster = restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMMaster = results;
        });
        $scope.AllColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.AllColorList = results;
        });
        $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transports = results;
        });
        $scope.Employ = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employ = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Permission = UserDetails.Info;
        $scope.SearchRef = restService.SearchReference;

        $scope.SearchConditionItems = [{ 'id': 1, 'name': 'Barcode' }, { 'id': 2, 'name': 'Damaged Invoice' },
        { 'id': 3, 'name': 'Invoice' },
        { 'id': 4, 'name': 'Entry No' }];

        $scope.DiscountModes = [{ 'id': 1, 'name': 'Value' }, { 'id': 2, 'name': '%' }];


        $scope.RetrunType = [{ 'id': 0, 'name': 'Returns - Debit Note' },
        { 'id': 5, 'name': 'Discount Note' },
        { 'id': 6, 'name': 'Supplier Credit Note' }];

        $scope.ProductList = [];
        restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $.each(results, function (k, v) {
                $scope.ProductList.push(v);
            })
        });
        $scope.FreightList = [{ id: 1, name: 'To Pay' }, { id: 2, name: 'Paid' }];

        $scope.suppliercompany = null;
        $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
        });
        $scope.Agents = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agents = results;
        });

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });
        $scope.returnqty = 0;
        $scope.PrintDocument = { 'pages': [] };
        $scope.UploadedBarcodes = [];
        $scope.MissingBarcodes = null;
        $scope.FileUpload = false;

        $scope.Entity = {
            "mode": 0,
            "returntype": 0,
            "productid": 0,
            "companyid": 0,
            "id": 0,
            "isactive": false,
            "createdby": 0,
            "modifiedby": 0,
            "version": 0,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "itemid": 0,
            "qty": 0.0,
            "piece": 0.0,
            "barcode": null,
            "stockid": 0,
            "lrinvoiceitemsid": 0,
            "taxid": 0,
            "rate": 0.0,
            "tax": 0.0,
            "total": 0,
            "returnno": null,
            "returndate": new Date(),
            "lrinvoiceid": 0,
            "lrid": 0,
            "discount": 0.0,
            "amount": 0.0,
            "lrinvoicereturnid": 0,
            "BOMinvoicereturnitems": [],
            "Lrinvoicereturngroups": [],
            "transportid": "0",
            "bundles": "0",
            "freightamt": "0.0",
            "freight": 0,
            "freighttaxamt": 0,
            "freighttaxid": 0,
            "weight": "0.0",
            "packingcharge": "0.0",
            "remarks": ""
        };

        $scope.CreteriaList = [{ 'id': 1, 'name': 'LR Number' },
        { 'id': 1, 'name': 'Invoice No' }];
        $scope.SearchInvoiceCondition = { 'mode': 1, 'barcode': '', 'invoiceno': '' };

        $scope.New = function () {
            $scope.LoadReturnList = [];
            $scope.PurchaseReturn = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.PurchaseReturn.returndate = new Date();
        }
        $scope.New();

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.Locations = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        //$scope.Locations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
        //    $scope.Locations = results;
        //    results.push({ 'id': 0, 'name': 'Warehouse', 'parentgroup': 0 });

        //    if (!$stateParams.directlaunch) {
        //        $timeout(function () {
        //            $scope.LoadDefaultItems();
        //        }, 50);
        //    }
        //    else {
        //        $scope.PurchaseReturn.locationid = 0;
        //    }

        //});
        $scope.PurchaseTax = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.PurchaseTax = results;
        });
        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
        });

        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.ErrorStyle = "success";

        $scope.ShowSearch = false;
        $scope.Debitnote = false;

        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Debitnote = false;
            $scope.PrintDocument = { 'pages': [] };
            //$scope.PurchaseReturn = (JSON.parse(JSON.stringify($scope.Entity)));
        }


        $scope.SearchRequest = {
            returndate: ""
        };

        $scope.FilterSupplierCompany = function () {

            var suppliercompany = $scope.SupplierCompany.find(function (item) {
                return item.supplierid == $scope.PurchaseReturn.supplierid;
            });

            if (typeof (suppliercompany) != 'undefined' && suppliercompany != null)
                $scope.PurchaseReturn.suppliercompanyid = suppliercompany.id;


            /*$scope.suppliercompany = [];
            if ($scope.SupplierCompany != null && $scope.PurchaseReturn.supplierid > 0) {
                $.each($scope.SupplierCompany, function (k, v) {
                    if (v.supplierid == $scope.PurchaseReturn.supplierid)
                        $scope.suppliercompany.push(v);
                });
            }*/
        };

        $scope.categoryChange = function () {
            if (typeof ($scope.ItemDetails.bomcategoryid) != 'undefined' && $scope.ItemDetails.bomcategoryid != null) {
                var BOMList = [];
                $.each($scope.BOMMaster, function (k, v) {
                    if (v.categoryid == $scope.ItemDetails.bomcategoryid)
                        BOMList.push(v);
                });
                $scope.BOMList = BOMList;
            }
        }

        $scope.setColourFocus = function () {
            if ($scope.ColorList.length > 0)
                $scope.setFocuscolourid();
            else
                $scope.setFocuswidth();
        }

        $scope.productChange = function () {
            if ($scope.ItemDetails.bomid && $scope.ItemDetails.bomid != null) {
                var prditem = $scope.BOMList.find(function (item) {
                    return (item.id == $scope.ItemDetails.bomid);
                });
                $scope.ItemDetails.fgproductid = 0;
                $scope.ItemDetails.fgskuid = '';
                if (prditem || prditem != null) {
                    $scope.ItemDetails.taxid = prditem.taxid;
                    if (prditem.coretypeid && prditem.coretypeid == 3) {
                        $scope.ItemDetails.fgproductid = prditem.tproductid;
                        $scope.ItemDetails.fgskuid = prditem.tskuid;
                    }
                    $scope.SetColourList(prditem);
                }
            };
        };


        $scope.SetColourList = function (prditem) {
            $scope.ItemDetails.colourid = null;
            $scope.ItemDetails.bominwardcode = '';
            if (prditem.colourmode == 2) {
                $scope.ColorList = [];
                $.each(prditem.colourlist, function (k, v) {
                    var colour = $scope.AllColorList.find(function (item) {
                        return item.id == v.id;
                    });
                    if (colour && colour.id > 0) {
                        colour.bominwardcode = v.name;
                        colour.imageid = v.imageid;
                        $scope.ColorList.push(colour);
                    }
                });
            } else if (prditem.colourmode == 1) {
                $scope.ColorList = [];
                var colour = $scope.AllColorList.find(function (item) {
                    return item.id == prditem.colorid;
                });
                if (colour && colour.id > 0) {
                    colour.bominwardcode = v.name;
                    colour.imageid = v.imageid;
                    $scope.ColorList.push(colour);
                }
            }
            else {
                $scope.ColorList = [];
                $scope.ItemDetails.bominwardcode = prditem.code;
                $scope.ItemDetails.imageid = prditem.imageid;
            }
        }
        $scope.LocationChanged = function () {
            var location = $scope.Locations.find(function (item) {
                return item.id == $scope.PurchaseReturn.locationid;
            });
            if (location && location != null && typeof (location.productgroup) != 'undefined') {
                $scope.PurchaseReturn.companyid = location.productgroup;
            }
        }

        $scope.fileuploadedcomplete = function (value) {
            $scope.uploadfile = null;
            if ($scope.PurchaseReturn.companyid == 'undefined' || $scope.PurchaseReturn.companyid == 0) {
                $rootScope.Toaster.Info("Please select company.");
                return;
            }
            $scope.SearchInvoiceCondition.mode = "1";
            var barcodes = [];
            $scope.UploadedBarcodes = [];
            $.each(value.rows, function (k, v) {
                var barcode = v.values[0];
                var qty = 1;
                if (v.values.length > 1) {
                    qty = parseFloat(v.values[1]);
                    if (typeof (qty) == 'undefined' || qty <= 0 || isNaN(qty))
                        qty = 1;
                }
                if (barcode != null && barcode != '' && barcode.length > 0) {
                    barcodes.push(barcode);

                    var itm = $scope.UploadedBarcodes.find(function (item) {
                        return item.barcode == barcode;
                    });
                    if (itm == 'undefined' || itm == null)
                        $scope.UploadedBarcodes.push({ 'barcode': barcode, 'qty': qty, 'aqty': 0 });
                    else
                        itm.qty = itm.qty + qty;
                }
            });
            $scope.FileUpload = true;

            var searchcondtion = {
                'locationid': $scope.PurchaseReturn.locationid, 'barcodes': barcodes, 'mode': '1', 'allowall': true, 'companyid': $scope.PurchaseReturn.companyid
            };
            $scope.SearchItemInternal(searchcondtion);
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'PURCHASE RETURN SUMMARY REPORT'
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


        $scope.ErrorMessage = "";

        $scope.SearchItems = function () {
            $scope.FileUpload = false;
            $scope.SearchInvoiceCondition.supplierid = $scope.PurchaseReturn.supplierid;
            $scope.SearchInvoiceCondition.returndate = $scope.PurchaseReturn.returndate;
            $scope.SearchInvoiceCondition.locationid = $scope.PurchaseReturn.locationid;
            $scope.SearchInvoiceCondition.companyid = $scope.PurchaseReturn.companyid;
            $scope.SearchInvoiceCondition.allowall = $scope.PurchaseReturn.mode == 0;
            $scope.SearchItemInternal($scope.SearchInvoiceCondition);
        }

        $scope.SearchItemInternal = function (condtion) {

            condtion.returntype = $scope.PurchaseReturn.returntype;
            if ($scope.PurchaseReturn.returntype && $scope.PurchaseReturn.returntype > 0) {
                condtion.mode = 3;
                $scope.PurchaseReturn.BOMinvoicereturnitems = [];
                $scope.CalculateTotal(true);
            }

            if ($scope.Appsetting_Company) {
                if (typeof (condtion.companyid) == 'undefined' || condtion.companyid == null ||
                    condtion.companyid <= 0) {
                    $rootScope.Toaster.Info("Please Select company.");
                    return;
                }
            }

            var request = {
                "Item": condtion,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "bominvoicereturnService/SearchItems",
                data: srequest
            }).success(function (result) {
                if (result.BOMinvoicereturnitems != null && result.BOMinvoicereturnitems.length > 0) {

                    $scope.PurchaseReturn.supplierid = result.supplierid;
                    $scope.FilterSupplierCompany();
                    $scope.PurchaseReturn.suppliercompanyid = result.suppliercompanyid;
                    $scope.PurchaseReturn.Lrinvoicereturngroups = [];
                    $scope.PurchaseReturn.BOMinvoicereturnitems = [];
                    $scope.AddReturnItems(result.BOMinvoicereturnitems);
                    $scope.PurchaseReturn.packingcharge = result.packingcharge;
                    $scope.PurchaseReturn.freightamt = result.freightamt;
                    $scope.PurchaseReturn.freighttaxamt = result.freighttaxamt;
                    $scope.PurchaseReturn.freighttaxid = result.freighttaxid;
                    $scope.CalculateTotal();
                    $scope.SearchInvoiceCondition.barcode = "";
                    $scope.ConfigureGrid();
                    $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
                }
                else {
                    $rootScope.Toaster.Info("No Stock Available for given condition.");
                }
            }).error(function (result) {
            });
        };


        //$scope.SearchItem = function () {
        //    $scope.SearchInvoiceCondition.supplierid = $scope.PurchaseReturn.supplierid;
        //    var request = {
        //        "Item": $scope.SearchInvoiceCondition,
        //        "securitytoken": "String content"
        //    }

        //    var srequest = JSON.stringify(request);

        //    $http({
        //        method: 'POST',
        //        url: "LrinvoicereturnService/SearchItems",
        //        data: srequest
        //    }).success(function (result) {
        //        $scope.LoadReturnList = result.Items;
        //        $scope.SearchCondition.showSearchResult = true;
        //    }).error(function (result) {
        //    });
        //};

        //$scope.SearchBarcodes = function () {
        //    if ($scope.PurchaseReturn.barcode == null || $scope.PurchaseReturn.barcode == "")
        //        return;

        //    restService.Get('LrinvoicereturnService/Barcode?id=' + $scope.PurchaseReturn.barcode, true).then(function (results) {
        //        if (results != null && results.barcode == $scope.PurchaseReturn.barcode) {
        //            results.taxrate = results.tax / results.qty;
        //            results.discountrate = results.discount / results.qty;
        //            results.qty = 0;
        //            results.tax = 0;
        //            results.amount = 0;
        //            $scope.PurchaseReturn.BOMinvoicereturnitems.push(results);
        //        }
        //    });
        //}

        $scope.LoadReturnList = [];

        $scope.SearchCondition = {
            receivedbyemployeid: "",
            paidbyemployeeid: "",
            paidon: "",
            showSearchResult: false,
            searchtype: 0
        }

        $scope.AddReturnItems = function (items) {
            if (items && items != null && items.length > 0) {
                if (items[0].imageid && items[0].imageid != null && items[0].imageid.length > 0) {
                    $scope.ShowImage = true;
                    $scope.currentimageid = items[0].imageid;
                }
            }

            $.each(items, function (k, v) {

                v.returnqty = v.qty;

                if (!isNaN(v.returnqty) && v.returnqty > 0) {

                    v.returnqty = $scope.PurchaseReturn.returntype > 0 ? v.qty : 0;

                    var returnqty = v.returnqty;

                    if ($scope.PurchaseReturn.mode == 0) {
                        var group = $scope.PurchaseReturn.Lrinvoicereturngroups.find(function (item) {
                            return item.suppliercompanyid == v.suppliercompanyid;
                        });
                        if (group != 'undefined' && group != null) {
                            group.qty += returnqty;
                            if ($scope.PurchaseReturn.Lrinvoicereturngroups.length > 1) {
                                var index = $scope.PurchaseReturn.Lrinvoicereturngroups.indexOf(group);
                                $scope.PurchaseReturn.Lrinvoicereturngroups.splice(index, 1);
                                $scope.PurchaseReturn.Lrinvoicereturngroups.splice(0, 0, group);
                            }
                        }
                        else {
                            var suppliercompany = $scope.SupplierCompany.find(function (item) {
                                return item.id == v.suppliercompanyid;
                            });
                            group = {
                                'suppliercompanyid': v.suppliercompanyid, 'suppliername': suppliercompany.name, 'isinterstatesale': suppliercompany.isinterstatesale, 'qty': returnqty
                            };
                            $scope.PurchaseReturn.Lrinvoicereturngroups.splice(0, 0, group);
                        }
                    }

                    var existing = $scope.PurchaseReturn.BOMinvoicereturnitems.find(function (item) {
                        return item.barcode == v.barcode;
                    });

                    if (v.barcode != '' && v.barcode != null && existing != null) {
                        existing.qty = parseFloat(existing.qty);
                        if (isNaN(existing.qty))
                            existing.qty = 0;
                        existing.availableqty = v.qty;
                        existing.qty += returnqty;
                        existing.tax = (existing.taxpercentage * existing.qty * (existing.rate - existing.discountperitem) / 100).round(2);
                        existing.discount = (existing.discountperitem * existing.qty).round(2);
                        existing.amount = (existing.qty * (existing.rate)).round(2);

                        if (existing.availableqty < existing.qty)
                            existing.style = { "background-color": "lightgrey" };
                        else
                            existing.style = {};
                    }
                    else {
                        v.availableqty = v.qty;
                        v.qty = returnqty;
                        v.taxperitem = v.tax;
                        v.discountperitem = (v.rate * v.discountpercentage / 100).round(2);
                        v.tax = (v.taxpercentage * v.qty * (v.rate - v.discount) / 100).round(2);
                        v.discount = (v.discountperitem * v.qty).round(2);
                        v.amount = (v.qty * (v.rate)).round(2);
                        v.isactive = true;
                        
                        $scope.PurchaseReturn.BOMinvoicereturnitems.push(v);

                        if (v.availableqty < v.qty)
                            v.style = { "background-color": "lightgrey" };
                        else
                            v.style = {};
                    }
                }
            });
            
            if ($scope.SearchInvoiceCondition.mode == 1 && $scope.PurchaseReturn.mode == 1)
                $scope.SearchInvoiceCondition.returnqty = null;
            $scope.CalculateTotal();
            $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
        };

        $scope.CalculateTotal = function (onlytotal) {

            if (typeof (onlytotal) == 'undefined' || !onlytotal) {
                $scope.PurchaseReturn.tax = 0;
                $scope.PurchaseReturn.discount = 0;
                $scope.PurchaseReturn.amount = 0;
                $scope.PurchaseReturn.total = 0;
                $scope.PurchaseReturn.qty = 0;
                $scope.PurchaseReturn.piece = 0;
                $.each($scope.PurchaseReturn.BOMinvoicereturnitems, function (k, v) {
                    $scope.PurchaseReturn.tax = $scope.GetNumber(v.tax) + $scope.PurchaseReturn.tax;
                    $scope.PurchaseReturn.discount = $scope.GetNumber(v.discount) + $scope.PurchaseReturn.discount;
                    $scope.PurchaseReturn.amount = $scope.GetNumber(v.amount) + $scope.PurchaseReturn.amount;
                    $scope.PurchaseReturn.qty = $scope.GetNumber(v.qty) + $scope.PurchaseReturn.qty;

                    var found = $scope.ProductList.find(function (item) {
                        return item.id == v.productid;
                    });
                    if (found != null && found.sellingmode == 3)
                        $scope.PurchaseReturn.piece += 1;
                    else
                        $scope.PurchaseReturn.piece += $scope.GetNumber(v.qty);
                });
            }

            $scope.PurchaseReturn.tax = $scope.PurchaseReturn.tax.round(2);
            $scope.PurchaseReturn.discount = $scope.PurchaseReturn.discount.round(2);
            $scope.PurchaseReturn.amount = $scope.PurchaseReturn.amount.round(2);

            if ($scope.PurchaseReturn.freight != 'undefined' && $scope.PurchaseReturn.freight == 2) {
                if ($scope.PurchaseReturn.freighttaxid != 'undefined' && $scope.PurchaseReturn.freighttaxid > 0) {
                    var freighttax = $scope.PurchaseTax.find(function (sitem) {
                        return $scope.PurchaseReturn.freighttaxid == sitem.id;
                    });
                    if (freighttax != 'undefined' && freighttax != null) {
                        var ftax = (($scope.GetNumber($scope.PurchaseReturn.freightamt) * freighttax.taxpercentage) / 100).round(2);
                        $scope.PurchaseReturn.freighttaxamt = ftax;
                    }
                }
            }
            if ($scope.PurchaseReturn.freight != 'undefined' && $scope.PurchaseReturn.freight == 2)
                $scope.PurchaseReturn.total = ($scope.PurchaseReturn.amount + $scope.PurchaseReturn.tax + $scope.GetNumber($scope.PurchaseReturn.packingcharge) - $scope.PurchaseReturn.discount).round(0) + $scope.GetNumber($scope.PurchaseReturn.freightamt) + $scope.GetNumber($scope.PurchaseReturn.freighttaxamt);
            else
                $scope.PurchaseReturn.total = ($scope.PurchaseReturn.amount + $scope.PurchaseReturn.tax + $scope.GetNumber($scope.PurchaseReturn.packingcharge) - $scope.PurchaseReturn.discount).round(0);
        }



        $scope.ManagePurchaseItem = function (action, item) {
            if (action == 'Delete') {
                var index = $scope.PurchaseReturn.BOMinvoicereturnitems.indexOf(item);
                $scope.PurchaseReturn.BOMinvoicereturnitems.splice(index, 1);
                $scope.CalculateTotal();
                var ditem = item;
                var group = $scope.PurchaseReturn.Lrinvoicereturngroups.find(function (sitem) {
                    return sitem.suppliercompanyid == ditem.suppliercompanyid;
                });
                if (group && group != null) {
                    group.qty -= $scope.GetNumber(ditem.qty);
                    if (group.qty <= 0) {
                        var index = $scope.PurchaseReturn.Lrinvoicereturngroups.indexOf(group);
                        $scope.PurchaseReturn.Lrinvoicereturngroups.splice(index, 1);
                    }
                }
                $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
            }
            else if (action == 'Edit') {
                item.newrate = item.rate;
                item.newqty = item.qty;
                item.newdiscountperitem = item.discountperitem;
                item.newdiscountpercentage = item.discountpercentage;
                item.newtax = item.tax;
                item.newtaxpercentage = item.taxpercentage;
                item.newdiscount = item.discount;
                item.newamount = item.amount;
                item.newdesignid = item.designid;
                $scope.taxpercentagechanged(item);
                item.editable = true;
                $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
            }
            else if (action == 'Cancel') {
                item.editable = false;
                $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
            }
            else {
                if ($scope.GetNumber(item.newqty) > $scope.GetNumber(item.availableqty)) {
                    $rootScope.Toaster.Info("Return Qty must be less than or equal to available Qty");
                    return;
                }

                var group = $scope.PurchaseReturn.Lrinvoicereturngroups.find(function (sitem) {
                    return sitem.suppliercompanyid == item.suppliercompanyid;
                });

                if (typeof (group) == 'undefined' && $scope.PurchaseReturn.Lrinvoicereturngroups.length > 0)
                    group = $scope.PurchaseReturn.Lrinvoicereturngroups[0];
                if (typeof (group) != 'undefined')
                    group.qty -= $scope.GetNumber(item.qty);
                $scope.taxpercentagechanged(item);
                item.tax = item.newtax;
                item.rate = item.newrate;
                item.qty = item.newqty;
                item.amount = item.newamount;
                item.taxpercentage = item.newtaxpercentage;
                item.discount = $scope.GetNumber(item.newdiscount);
                item.designid = item.newdesignid;
                item.discountperitem = item.newdiscountperitem;
                item.discountpercentage = $scope.GetNumber(item.newdiscountpercentage);;
                item.editable = false;
                if (typeof (group) != 'undefined')
                    group.qty += $scope.GetNumber(item.newqty);
                $scope.CalculateTotal();
                $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
            }
        }


        $scope.QtyChanged = function (item) {
            /*var oldValue = item.amount;
            var oldtaxValue = item.tax;
            var olddiscount = item.discount;
            item.amount = isNaN(item.rate * item.qty) ? 0 : item.rate * item.qty;
            item.tax = isNaN(item.taxrate * item.qty) ? 0 : item.taxrate * item.qty;
            item.discount = isNaN(item.discountrate * item.qty) ? 0 : item.discountrate * item.qty;
            $scope.PurchaseReturn.tax = $scope.PurchaseReturn.tax - oldtaxValue + item.tax;
            $scope.PurchaseReturn.amount = $scope.PurchaseReturn.amount - oldValue + item.amount;
            $scope.PurchaseReturn.discount = $scope.PurchaseReturn.discount - olddiscount + item.discount;*/
        };

        $scope.Save = function () {

            if ($scope.PurchaseReturn.id > 0 && $scope.Permission.p_PORETURN.modify == false) {
                $rootScope.Toaster.Info("No Permission!!");
                return;
            }

            if (typeof ($scope.PurchaseReturn.locationid) == 'undefined' || $scope.PurchaseReturn.locationid == null) {
                $rootScope.Toaster.Info("Please Select Stock Location.!");
                return;
            }

            if ($scope.PurchaseReturn.mode == 1) {
                if ($scope.PurchaseReturn.companyid == null || $scope.PurchaseReturn.companyid == 0) {
                    $rootScope.Toaster.Info("Please Select Company.!");
                    return;
                }

                if ($scope.PurchaseReturn.supplierid == null || $scope.PurchaseReturn.supplierid == 0) {
                    $scope['setFocussupplierid']();
                    $rootScope.Toaster.Info("Please Select Supplier.!");
                    return;
                }

                if ($scope.PurchaseReturn.suppliercompanyid == null || $scope.PurchaseReturn.suppliercompanyid == 0) {
                    $scope['setFocussuppliercompanyid']();
                    $rootScope.Toaster.Info("Please select suppliercompany");
                    return;
                }
            }

            if ($scope.PurchaseReturn.returndate == null || $scope.PurchaseReturn.returndate == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Info("Please Select Date.!");
                return;
            }

            var request = {
                "Item": (JSON.parse(JSON.stringify($scope.PurchaseReturn))),
                "securitytoken": "String content"
            }

            request.Item.tax = parseFloat(request.Item.tax).round(2);
            request.Item.discount = parseFloat(request.Item.discount).round(2);
            request.Item.amount = parseFloat(request.Item.amount).round(2);
            request.Item.total = parseFloat(request.Item.total).round(0);
            request.Item.freightamt = parseFloat(request.Item.freightamt).round(0);
            request.Item.qty = parseFloat(request.Item.piece).round(2);
            request.Item.freighttaxamt = parseFloat(request.Item.freighttaxamt).round(2);
            if ($scope.PurchaseReturn.freight != 'undefined' && $scope.PurchaseReturn.freight == 2)
                request.Item.rounding = parseFloat(request.Item.total - (request.Item.tax + request.Item.amount + request.Item.freightamt + request.Item.freighttaxamt + request.Item.packingcharge - request.Item.discount)).round(2);
            else
                request.Item.rounding = parseFloat(request.Item.total - (request.Item.tax + request.Item.amount + request.Item.packingcharge - request.Item.discount)).round(2);

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "bominvoicereturnService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && typeof (result.length) == 'undefined' && result.id > 0) {
                    $rootScope.Toaster.Info("Purchase Return Saved Successfully with Debit note no : " + result.returnno);
                    $scope.PurchaseReturn = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
                } else if (result != null && typeof (result.length) != 'undefined' &&
                    result.length > 0 && result[0].id > 0) {
                    if (result.length == 1) {
                        $rootScope.Toaster.Info("Purchase Return Saved Successfully with Debit note no : " + result[0].returnno);
                    }
                    else {
                        var returnnos = [];
                        $.each(result, function (k, v) {
                            returnnos.push(v.returnno);
                        });
                        $rootScope.Toaster.Info(returnnos.length + " Purchase Return Saved Successfully with Debit note no : " + returnnos.join(','));
                    }
                    $scope.PurchaseReturn = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
                }
                else if (typeof (result.length) != 'undefined') {
                    $rootScope.Toaster.Error(result[0].errormessage);
                }
                else {
                    $rootScope.Toaster.Error(result.errormessage);
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
                                    url: "bominvoicereturnService/Delete",
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
                $scope.ShowSearch = false;

                request = {
                    "Item": { 'id': item.id },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "bominvoicereturnService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.PurchaseReturn = result[0];
                    $scope.CalculateTotal();
                    $scope.Navigation.SetMode('EDIT');
                    $.each($scope.PurchaseReturn.BOMinvoicereturnitems, function (k, v) {
                        v.taxperitem = (v.tax / v.qty).round(2);
                        v.discountperitem = (v.discount / v.qty / v.rate * 100).round(2);
                        v.availableqty = v.qty;
                        if (v.serialno != null && v.serialno != '')
                            v.displaycode = v.serialno;
                        else
                            v.displaycode = v.barcode;
                    });

                    $scope.PurchaseReturn.tax = $scope.PurchaseReturn.tax.round(2);
                    $scope.PurchaseReturn.discount = $scope.PurchaseReturn.discount.round(2);
                    $scope.PurchaseReturn.amount = $scope.PurchaseReturn.amount.round(2);
                    $scope.PurchaseReturn.total = ($scope.PurchaseReturn.amount - $scope.PurchaseReturn.discount + $scope.PurchaseReturn.tax + $scope.PurchaseReturn.freightamt + $scope.PurchaseReturn.freighttaxamt + $scope.PurchaseReturn.packingcharge).round(0);

                    $scope.PurchaseReturn.Lrinvoicereturngroups = [];

                    if ($scope.SupplierCompany != null) {
                        var suppliercompany = $scope.SupplierCompany.find(function (item) {
                            return item.supplierid == $scope.PurchaseReturn.supplierid;
                        });
                        var group = {
                            'suppliercompanyid': $scope.PurchaseReturn.suppliercompanyid, 'suppliername': suppliercompany.name, 'isinterstatesale': suppliercompany.isinterstatesale, 'qty': $scope.PurchaseReturn.qty
                        };
                        $scope.PurchaseReturn.Lrinvoicereturngroups.splice(0, 0, group);
                    }
                    $scope.ConfigureGrid();
                    $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
                }).error(function (result) {
                });

            }
        }

        $scope.Search = function () {
            $scope.ShowSearch = true;

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "bominvoicereturnService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ReturnConfig();
                if (result == 'undefined' || result == null || result.length == 0)
                    $rootScope.Toaster.Info("No Results Found.");
            }).error(function (result) {
            });

        }

        $scope.AddItem = function () {
            var newitem = (JSON.parse(JSON.stringify($scope.ItemDetails)));
            $scope.PurchaseReturn.BOMinvoicereturnitems.push($scope.ItemDetails);
            $scope.CalculateTotal();
            $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
        }

        $scope.ReturnConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);
            igrid.AddSimpleColumn('returnno', 'Return No', 100);

            igrid.AddFormatedColumn('returndate', 'Return Date', 85, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('supplierid', 'Supplier', 170, $scope.Suppliers);
            igrid.AddReferenceColumn('cityid', 'Supplier City', 100, $scope.Cities);
            igrid.AddReferenceColumn('transportid', 'Transport', 120, $scope.Transports);
            igrid.AddSimpleColumn('createdusername', 'Who Created', 100);
            igrid.AddSimpleColumn('lrno', 'LR No', 80);
            igrid.AddFormatedColumn('lrdate', 'LR Date', 85, 'dd-MM-yyyy');

            igrid.AddFormatedColumn('qty', 'Qty', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('total', 'Amount', 90, 'N2', 2).total = true;
            igrid.AddFormatedColumn('bundles', 'Bundles', 60, 'N2', 2).total = true;

            var commands = "";
            if ($rootScope.UserInfo.p_PORETURN.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_PORETURN.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>  &nbsp;"
            if ($rootScope.UserInfo.p_PORETURN.print)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='LRInfo'/> &nbsp;"
            if ($rootScope.UserInfo.p_PORETURN.print)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Print'/>"

            if ($rootScope.UserInfo.p_PORETURN.modify || $rootScope.UserInfo.p_PORETURN.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 200, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                        else if ($(e.target).val() == 'LRInfo')
                            $scope.UpdateTransports(args.item);
                        else if ($(e.target).val() == 'Print')
                            $scope.PrintDebitNote(args.item.id);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.Initialize();

            igrid.dataView.getItemMetadata = function (row) {
                var item = igrid.dataView.getItem(row);
                item.style = item.packedby == 0 ||
                    item.lrno == '' ? 'igridwithgrey' : '';
                return { cssClasses: '' };
            };
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.SearchResult);

        }


        var pagesize = 20;

        $scope.PrintPage = function (from, to) {
            $scope.PrintDocument.items = [];
            for (var i = from; i < to; i++) {
                $scope.PrintDocument.items.push($scope.PrintDocument.BOMinvoicereturnitems[i]);
            };

            $scope.PrintDocument.printstatus = ($scope.PrintDocument.BOMinvoicereturnitems.length == to) ? '' : 'Continue';

            var emptylines = pagesize - $scope.PrintDocument.items.length;
            $scope.PrintDocument.emptyline = [];
            if (emptylines > 0) {
                for (var i = 0; i < emptylines; i++) {
                    $scope.PrintDocument.emptyline.push({ 'index': i + $scope.PrintDocument.items.length });
                }
            }

            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
                var titlele = angular.element('#apptitle').context.title = "DEBIT NOTE";

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


        $scope.PrintDebitNotePage = function () {
            var page = $scope.PrintDocument.pages.find(function (item) {
                return item.from == $scope.PrintDocument.pagefrom;
            });
            if (page != null) {
                $scope.PrintPage(page.from, page.to);
            }
        }

        $scope.PrintDebitNote = function (id) {
            $timeout(function () {
                $rootScope.Download('bominvoicereturnService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.PurchaseReturn.id : id), 'debitnote.pdf', 'application/pdf');
                return;

                var link = document.createElement('a');
                link.download = "debitnote.pdf";
                link.href = 'bominvoicereturnService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.PurchaseReturn.id : id);
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }


        $scope.UpdateTransports = function (item) {
            var dialog = ngDialog.open({
                template: 'UpdateLRInfo',
                data: [$scope, item],
                width: '700px',
                controller: ['$scope', '$timeout', '$http', '$rootScope', function ($scope, $timeout, $http, $rootScope) {
                    $scope.PurchaseReturn = (JSON.parse(JSON.stringify($scope.ngDialogData[1])));
                    $scope.Transports = $scope.ngDialogData[0].Transports;
                    $scope.Cities = $scope.ngDialogData[0].Cities;
                    $scope.Employ = $scope.ngDialogData[0].Employ;
                    if ($scope.PurchaseReturn.lrdate && new Date($scope.PurchaseReturn.lrdate).getFullYear() < 2000) {
                        $scope.PurchaseReturn.lrdate = new Date();
                    }
                    $scope.BillImageChanged = function () {
                        $scope.PurchaseReturn.billimg = $scope.billimg;
                        $scope.billimg = null;
                    }
                    $scope.ChangeSupplier = function () {
                        var request = {
                            "Item": $scope.PurchaseReturn,
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "bominvoicereturnService/ClosedSupplier",
                            data: srequest
                        }).success(function (result) {

                            if (result.version > item.version) {
                                $rootScope.Toaster.Info(result.errormessage);
                                $scope.closeThisDialog(result);
                            }
                            else {
                                $rootScope.Toaster.Error(result.errormessage);
                            }
                        });
                    }
                    $scope.Update = function ($event) {

                        var request = {
                            "Item": $scope.PurchaseReturn,
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "bominvoicereturnService/UpdateLRInfo",
                            data: srequest
                        }).success(function (result) {
                            if (result.version > item.version) {
                                $scope.closeThisDialog(result);
                            }
                        });
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$document'
                        && data.value != '$escape') {
                        item.remarks = data.value.remarks;
                        item.transportid = data.value.transportid;
                        item.lrno = data.value.lrno;
                        item.lrdate = data.value.lrdate;
                        item.version = data.value.version;
                    }
                }
            });
        };

        $scope.PrintDebitNote_Working = function () {

            request = {
                "Item": { 'id': $scope.PurchaseReturn.id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "bominvoicereturnService/Print",
                data: srequest
            }).success(function (result) {

                $scope.PrintDocument = result;
                $scope.PrintDocument.piece = $scope.PrintDocument.qty;
                $scope.PrintDocument.pages = [];
                $scope.PrintDocument.qty = 0;
                $scope.PrintDocument.cgsttaxamount = 0;
                $scope.PrintDocument.sgsttaxamount = 0;
                var sno = 1;
                $.each($scope.PrintDocument.BOMinvoicereturnitems, function (k, v) {
                    $scope.PrintDocument.qty += v.qty;
                    $scope.PrintDocument.cgsttaxamount += v.taxsplit.cgsttaxamount;
                    $scope.PrintDocument.sgsttaxamount += v.taxsplit.sgsttaxamount;
                    v.amount = v.qty * v.rate;
                    v.sno = sno++;
                });

                $scope.PrintDocument.cgsttaxamount = $scope.PrintDocument.cgsttaxamount.round(2);
                $scope.PrintDocument.sgsttaxamount = $scope.PrintDocument.sgsttaxamount.round(2);

                $scope.PrintDocument.secondtax = $scope.PrintDocument.isinterstatesale ? 'IGST' : 'SGST';
                if ($scope.PrintDocument.BOMinvoicereturnitems.length <= pagesize) {
                    $scope.PrintPage(0, $scope.PrintDocument.BOMinvoicereturnitems.length);
                }
                else {
                    var pageindex = 0;
                    for (var i = 0; i < $scope.PrintDocument.BOMinvoicereturnitems.length; i = i + pagesize) {
                        pageindex++;
                        $scope.PrintDocument.pages.push({ 'name': 'Page - ' + pageindex, 'from': i, 'to': ($scope.PrintDocument.BOMinvoicereturnitems.length < i + pagesize) ? $scope.PrintDocument.BOMinvoicereturnitems.length : i + pagesize });
                    }
                }

            }).error(function (result) {
            });
        }

        $scope.ClearItems = function () {
            $scope.PurchaseReturn.BOMinvoicereturnitems = [];
            $scope.PurchaseReturn.Lrinvoicereturngroups = [];
            $scope.MissingBarcodes = null;
            $scope.PurchaseReturn = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.PurchaseReturn.returndate = new Date();
            $scope.CalculateTotal();
            $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
        }

        $scope.ApplyDiscount = function () {

            if (typeof ($scope.PurchaseReturn._total) != 'undefined')
                $scope.PurchaseReturn._total = $scope.PurchaseReturn.total;
            else
                $scope.PurchaseReturn.total = $scope.PurchaseReturn.total;

            var discountpercentage = parseFloat($scope.SearchInvoiceCondition.discountpercentage);
            if ($scope.SearchInvoiceCondition.discountmode == 1)
                discountpercentage = discountpercentage / $scope.PurchaseReturn.total * 100;

            $.each($scope.PurchaseReturn.BOMinvoicereturnitems, function (k, v) {
                if (typeof (v.source) == 'undefined') {
                    v.source = (JSON.parse(JSON.stringify(v)));
                }
                v.rate = (v.source.rate * discountpercentage / 100).round(2);
                v.taxperitem = (v.source.taxperitem * discountpercentage / 100).round(2);
                v.discountperitem = 0;
                v.discount = 0;
                v.tax = (v.taxpercentage * v.qty * v.rate / 100).round(2);
                v.amount = (v.qty * (v.rate)).round(2);
            });
            $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
            $scope.CalculateTotal();
        }

        $scope.DeleteGroup = function (item) {

            $scope.PurchaseReturn.Lrinvoicereturngroups.splice($scope.PurchaseReturn.Lrinvoicereturngroups.indexOf(item), 1);

            var deletelist = [];
            $.each($scope.PurchaseReturn.BOMinvoicereturnitems, function (k, v) {
                if (v.suppliercompanyid == item.suppliercompanyid) {
                    deletelist.push(v);
                }
            });

            $.each(deletelist, function (k, v) {
                var index = $scope.PurchaseReturn.BOMinvoicereturnitems.indexOf(v);
                $scope.PurchaseReturn.BOMinvoicereturnitems.splice(index, 1);
            });
            $scope.CalculateTotal();
            $scope.igrid.UpdateData($scope.PurchaseReturn.BOMinvoicereturnitems, '_id');
        }

        $scope.Appsetting_Company = false;
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined') {
                $scope.Appsetting_Company = true;


            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }

        if (typeof ($location.search().id) != 'undefined' && $location.search().id.length > 0) {
            $scope.ManageSearchItem('Edit', { 'id': $location.search().id })
        }

        $timeout(function () {
            $scope.LoadApplicationSetting();
            $scope.ConfigureGrid();
        }, 10);

        $scope.LoadDefaultItems = function (type) {
            $scope.PurchaseReturn.mode = 0;
            $scope.PurchaseReturn.locationid = $stateParams.data.locationid;
            $scope.PurchaseReturn.companyid = $stateParams.data.companyid;

            if (typeof ($scope.PurchaseReturn.companyid) == 'undefined' ||
                $scope.PurchaseReturn.companyid == 0) {
                $scope.PurchaseReturn.companyid = $stateParams.data.list[0].companyid;
            }
            var barcodes = [];
            $.each($stateParams.data.list, function (k, v) {
                barcodes.push(v.barcode);
            });
            var searchcondtion = {
                'locationid': $scope.PurchaseReturn.locationid,
                'barcodes': barcodes, 'mode': '1',
                'allowall': true,
                'companyid': $scope.PurchaseReturn.companyid
            };
            $scope.SearchItemInternal(searchcondtion);
        }
        //PurchaseReturn.BOMinvoicereturnitems

        $scope.ShowDirectBOM = function () {
            $scope.ConfigureGrid();
        }

        $scope.marginChanged = function () {
            var qty = $scope.GetNumber($scope.ItemDetails.qty);
            var rate = $scope.GetNumber($scope.ItemDetails.rate);
            var taxpercentage = $scope.GetNumber($scope.ItemDetails.taxpercentage);
            $scope.ItemDetails.gross = qty * rate;
            $scope.ItemDetails.tax = (rate * qty * taxpercentage / 100).round(4);
            $scope.ItemDetails.amount = ($scope.ItemDetails.gross + $scope.ItemDetails.tax).round(2);
        }

        $scope.ConfigureGrid = function () {

            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContaineritems').empty();
            }

            var igrid = new InfyGrid('gridContaineritems', '425px', '100%', false, true, true, $scope.PurchaseReturn.isdirectbom ? 200 : 140);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('bominwardcode', 'Barcode', 100);
            igrid.AddReferenceColumn('bomid', 'BOM', 170, $scope.BOMMaster);
            igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
            igrid.AddSimpleColumn('width', 'Width', 80);
            var column = igrid.AddColumn(
                {
                    id: "qty", name: "Qty", field: "qty",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            column.total = true;
            column.decimal = 2;
            column.format = 'N2';

            var column = igrid.AddColumn(
                {
                    id: "rate", name: "Rate", field: "rate",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            column.decimal = 2;
            column.format = 'N2';

            var column = igrid.AddColumn(
                {
                    id: "discountpercentage", name: "Discount %", field: "discountpercentage",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            column.decimal = 2;
            column.format = 'N2';

            var column = igrid.AddColumn(
                {
                    id: "taxpercentage", name: "Tax %", field: "taxpercentage",
                    width: 40, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            column.decimal = 2;
            column.format = 'N2';

            var column = igrid.AddColumn(
                {
                    id: "tax", name: "Tax", field: "tax",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            column.total = true;
            column.decimal = 2;
            column.format = 'N2';

            var column = igrid.AddColumn(
                {
                    id: "discount", name: "Discount", field: "discount",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            column.cssClass = 'rightalign';
            column.total = true;
            column.decimal = 2;
            column.format = 'N2';

            var column = igrid.AddFormatedColumn('amount', 'Amount', 80, 'N2', 2);
            column.cssClass = 'rightalign';
            column.total = true;

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Remove'/> &nbsp;"

            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='Delete' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            //igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='Edit' value='Edit' class='btn btn-primary btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            //igrid.AddButtonColumn('Apply', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='Apply Changes' value='Apply' class='btn btn-info btn-xs'><i class='fa fa-check' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            //igrid.AddButtonColumn('Cancel', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' title='Cancel Changes' value='Cancel' class='btn btn-default btn-xs'><i class='fa fa-close' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();


            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $timeout(function () {
                    if (command == 'Delete')
                        $scope.ManagePurchaseItem('Delete', item);
                    else if (command == 'Edit')
                        $scope.ManagePurchaseItem('Edit', item);
                    else if (command == 'Apply')
                        $scope.ManagePurchaseItem('Apply', item);
                    else if (command == 'Cancel')
                        $scope.ManagePurchaseItem('Cancel', item);
                    else if (command == 'displaycode') {
                        $scope.ShowImage = false;
                        $scope.currentimageid = item.imageid;
                        if (item.imageid && item.imageid != null && item.imageid != '') {
                            $scope.ShowImage = true;
                            $scope.currentimageid = item.imageid;
                        }
                    }
                }, 10);

            });


            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var column = args.grid.getColumns()[args.cell].id;
                if (column == 'discount') {
                    $scope.discountchanged(item, true);
                    $scope.CalculateTotal();
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                if (column == 'taxpercentage' || column == 'discountpercentage' ||
                    column == 'rate' || column == 'qty') {
                    $scope.taxpercentagechanged(item, true);
                    $scope.CalculateTotal();
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });
            $scope.igrid = igrid;
        }
    }]);

