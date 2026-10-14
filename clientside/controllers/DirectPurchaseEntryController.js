var app = angular.module('InfinityLab');

app.controller('DirectEntryController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'cntxService', '$filter','localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, cntxService, $filter, localStorageService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.SetMode('SEARCH');
        $scope.LocalContext = cntxService.CreatePageContext('lrinvoiceitems');
        $scope.ProductSearch = {
            "ProductID": null, "SearchItemName": "",
            "currentInvoiceID": "", "ptype": 0
        };
       
        $scope.InvoiceInfo = { invoiceid: $location.search().invoiceid };
        $scope.AutoCalculation = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = false;
        $scope.showtax = false;
        $scope.SearchResult = [];
        $scope.ErrorMessage = '';
        $scope.InvoiceTaxItem = {};
        $scope.additionaldiscountpercentage = 0;
        $scope.ProductTypes = [
            { 'id': 0, 'name': 'Textile' },
            { 'id': 1, 'name': 'UAN/GLN' },
            { 'id': 2, 'name': 'Product' },
            { 'id': 3, 'name': 'B2B Textile' }
        ];
        $scope.EntryTypes = [
            { 'id': -1, 'name': 'None', 'discount': true, 'amount': true, 'tax': false, 'directamount': false },
            { 'id': 0, 'name': 'Amount', 'discount': true, 'amount': true, 'tax': true, 'directamount': false },
            { 'id': 1, 'name': 'Tax', 'discount': false, 'amount': false, 'tax': true, 'directamount': false },
            { 'id': 2, 'name': 'Discount', 'discount': true, 'amount': false, 'tax': false, 'directamount': false },
            { 'id': 3, 'name': 'OnDiscount', 'discount': true, 'amount': false, 'tax': false, 'directamount': false },
            { 'id': 4, 'name': 'Insurance', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
            { 'id': 5, 'name': 'Service', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
            { 'id': 6, 'name': 'Courier', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
            { 'id': 7, 'name': 'Packing', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
            { 'id': 8, 'name': 'Fright', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
            { 'id': 9, 'name': 'Rounding', 'discount': true, 'amount': true, 'tax': true, 'directamount': false },
            { 'id': 10, 'name': 'Job Work', 'discount': false, 'amount': true, 'tax': true, 'directamount': false }];
        $scope.Total = {
            "buyingqty": 0,
            "mrp": 0,
            "sellingprice": 0,
            "buyingprice": 0,
            "tax": 0,
            "buyingpiece": 0,
            "discount": 0,
            "purchasediscount":0
        };

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            if (!$scope.ShowSearch && $scope.egrid == null) {
                $timeout(function () {
                    $scope.ShowEditableGrid();
                }, 20);
            }
        }

        $scope.ShowTax = function () {
            $scope.showtax = !$scope.showtax;
        }
        $scope.SearchCondition = {
            companyid: 0,
            supplierid: 0,
            invoiceno: "",
            addedfrom: null,
            addedto: null,
            lrentryno: ""
        }

        $scope.ItemList = [];
        $scope.RetialLocations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.ALLRetialLocations = results;
            $scope.RetialLocations = (JSON.parse(JSON.stringify(results)));
            $rootScope.BindLocation($scope, 'Invoice.retaillocationid', $scope.CompanyChanged);
        });


        $scope.PurchaseTax = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.purchaseTaxActual = results;
            var taxlist = [];
            $.each(results, function (k, v) {
                if (typeof (v.taxsplit) == 'undefined' || typeof (v.taxsplit.items) == 'undefined' || v.taxsplit.items == null) {
                    var taxitem = (JSON.parse(JSON.stringify(v)));
                    taxitem.name = 'GST @ ' + taxitem.taxpercentage + '%';
                    taxitem.code = '' + taxitem.id + '_' + 1;
                    taxitem.taxsplit = { 'items': [{ 'cgst': taxitem.taxpercentage, 'sgst': 0 }] };
                    taxitem.type = 0;
                    taxlist.push(taxitem);
                }
                else {
                    $.each(v.taxsplit.items, function (kk, vv) {
                        var taxitem = (JSON.parse(JSON.stringify(v)));
                        taxitem.taxpercentage = vv.cgst + vv.sgst;
                        taxitem.type = 0;
                        if (v.taxsplit.items.length > 1)
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '% (' + vv.fromamount + ' - ' + vv.toamount + ')';
                        else
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = '' + taxitem.id + '_' + kk;
                        taxlist.push(taxitem);
                    });
                }
            })
            $scope.PurchaseTax = taxlist;
            $scope.PurchaseTax.push({ 'id': -1, 'code': '-1', 'name': 'Rounding', 'taxpercentage': 'Rounding', 'type': 9 });

            $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
                $.each(results, function (k, v) {

                    var taxitem = $scope.purchaseTaxActual.find(function (item) {
                        return item.id == v.referenceid;
                    });
                    if (typeof (taxitem) != 'undefined' && taxitem != null) {
                        var taxitem = (JSON.parse(JSON.stringify(taxitem)));
                        taxitem.name = v.name + ' @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = 'Ad_' + taxitem.id + '_' + 1;
                        if (taxitem.taxpercentage > 0)
                            taxitem.taxsplit = { 'items': [{ 'cgst': taxitem.taxpercentage / 2, 'sgst': taxitem.taxpercentage / 2 }] };
                        else
                            taxitem.taxsplit = { 'items': [{ 'cgst': 0, 'sgst': 0 }] };
                        taxitem.type = 8;
                        $scope.PurchaseTax.push(taxitem);
                    }
                });

            });
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.Transport = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transport = results;
        });
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });
        $scope.PatternList = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.PatternList = results;
        });

        $scope.StyleList = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.StyleList = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
            var exist = false;
            $.each($scope.SizeList, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.SizeList.push({ id: -2, name: 'Jump' });
                $scope.SizeList.push({ id: -3, name: 'Cut' });
            }
        });
        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
            $rootScope.BindCompany($scope, 'Invoice.companyid', $scope.CompanyChanged);
        });
        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });
        $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
        });
        $scope.MaterialList = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.MaterialList = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.CompanyChanged = function (focuslocation) {
            var availablelocations = [];
            if ($scope.ALLRetialLocations && $scope.ALLRetialLocations != null) {
                $.each($scope.ALLRetialLocations, function (k, v) {
                    if ($rootScope.GetNumber(v.productgroup) == 0
                        || $rootScope.GetNumber(v.productgroup) == $rootScope.GetNumber($scope.Invoice.companyid))
                        availablelocations.push(v)
                });
                availablelocations.sort(function (first, next) {
                    return first.name == next.name;
                });
                $scope.RetialLocations = availablelocations;
                $scope.ShowLastEntryNo();
            }
            if (focuslocation) {
                $timeout(function () {
                    $scope.setFocuslocationid();
                }, 10);
            }
        }

        $scope.SearchItem = function () {
            var request = {
                "Item": {
                    'name': $scope.ProductSearch.SearchItemName, 'productid': $scope.ProductSearch.ProductID
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
                    $scope.ItemDetails.Details = result[0];
                    $scope.ItemDetails.productid = $scope.ItemDetails.Details.productid;
                    $scope.ProductSearch.ProductID = $scope.ItemDetails.Details.productid;
                    $scope.findLastPrice();
                    $scope.productChange();
                }
                else if (result.length > 1) {
                    $scope.ShowProductSearch(result);
                }
            });
        };

        $scope.StatusList = [{ id: 1, name: 'Invoice Progress' }, { id: 3, name: 'Temporary' }, { id: 2, name: 'Invoice Completed' }];

        $scope.ItemTemplate = {
            "buyingqty": null,
            "mrp": null,
            "sellingprice": null,
            "buyingprice": null,
            "tax": 0,
            "staxpercentage": 0,
            "ptaxpercentage": 0,
            "taxid": null,
            "pricingmode": 0,
            "calculatedPrice": 0,
            "readonly": false,
            "isactive": true,
            "designid": "",
            "sno": 1,
            Details: {
                "productid": null,
                "printingname": null,
                "brandid": null,
                "patternid": null,
                "styleid": null,
                "typeid": null,
                "sizeid": null,
                "colourid": null,
                "materialid": null
            }
        }

        $scope.InvoiceTemplate = {
            "companyid": 0, "invoiceno": null, "entrydate": new Date(), "invoicedate": "", "lrdate": "", "noofpieces": 0, "noofbundle": 0, "baseamount": 0.0, "itemid": 0,
            "grossamount": 0.0, "totalamount": 0.0, "lrid": 0, "id": 0, "otheramount": 0.0, "modifiedon": "", "createdon": "",
            "createdby": 0, "modifiedby": 0, "version": 1, "suppliercompanyid": 0, "isactive": true, lrinvoicetaxitems: [], uniqueid: 0,
            Lrinvoiceitems: [], Lrentrydetails: [], "ptype": 0
        }

        $scope.defaultConfig = localStorageService.get('pcntx.directpurchase');
        if ($scope.defaultConfig == undefined || $scope.defaultConfig == null)
            $scope.defaultConfig = {};
        else {
            $scope.InvoiceTemplate.companyid = $scope.defaultConfig['Invoice.companyid'];
            $scope.InvoiceTemplate.retaillocationid = $scope.defaultConfig['Invoice.retaillocationid'];
            $scope.InvoiceTemplate.ptype = $scope.defaultConfig['Invoice.ptype'];
        }
        $scope.SetDefualtConfig = function () {
            $scope.defaultConfig = { 'Invoice.ptype': $scope.Invoice.ptype, 'Invoice.retaillocationid': $scope.Invoice.retaillocationid, 'Invoice.companyid': $scope.Invoice.companyid };
            localStorageService.set('pcntx.directpurchase', $scope.defaultConfig);
        }

        $scope.LastPrice = null;

        $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
        $scope.Invoice = (JSON.parse(JSON.stringify($scope.InvoiceTemplate)));
        //$scope.AddTax = (JSON.parse(JSON.stringify($scope.AdditionalTax)));
        $scope.Invoice.lrinvoicetaxitems = [];
        $scope.Invoice.Lrentrydetails = [];

        $scope.productItemChange = function () {
            if ($scope.ItemDetails.Details == undefined ||
                $scope.ItemDetails.Details == null) {
                $scope.ItemDetails.Details = {};
            }
            var item = $scope.ItemList.find(function (item) {
                return item.id == $scope.ItemDetails.itemid;
            });
            if (item && item != null) {
                $scope.ItemDetails.Details.itemid = item.id;
                $scope.ProductSearch.ProductID = item.productid;
                $scope.productChange();
                $scope.ItemDetails.Details.serialno = item.gln;
                $scope.SelectItem(item);
            }
        }

        $scope.CurrentPO = null;
        $scope.SearchPO = function (importpo) {
            var request = {
                "Item": {
                    'pono': $scope.Invoice.pono,
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PurchaseorderService/Search",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length == 1) {
                    $scope.CurrentPO = result[0];
                    $scope.Invoice.companyid = $scope.CurrentPO.companyid;
                    $scope.CompanyChanged();
                    $scope.Invoice.transportid = $scope.CurrentPO.transportid;
                    $scope.Invoice.supplierid = $scope.CurrentPO.supplierid;
                    var suppliercompany = $scope.SupplierCompany.find(function (item) {
                        return item.supplierid = $scope.Invoice.supplierid;
                    });
                    if (suppliercompany &&
                        suppliercompany != null) {
                        $scope.Invoice.suppliercompanyid = suppliercompany.id;
                    }
                }
            });
        }

        $scope.SerialNoEntered = function (event) {
            var request = {
                "Item": { 'gln': $scope.ItemDetails.Details.serialno },
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "ItemsService/Search",
                data: request
            }).success(function (result) {
                if (result != null &&
                    result.length > 0) {
                    $scope.ProductSearch.ProductID = result[0].productid;
                    $scope.productChange();
                    $scope.SelectItem(result[0]);
                    $scope.setFocushbuyingqty();

                    if ($scope.CurrentPO && $scope.CurrentPO != null) {
                        if ($scope.CurrentPO && $scope.CurrentPO.Purchaseorderitems &&
                            $scope.CurrentPO.Purchaseorderitems != null) {
                            var purchaseorder = $scope.CurrentPO.Purchaseorderitems.find(function (item) {
                                return item.gln == result[0].gln;
                            });
                            if (purchaseorder &&
                                purchaseorder != null) {
                                $scope.ItemDetails.buyingqty = purchaseorder.oqty;
                                $scope.ItemDetails.basebuyingprice = purchaseorder.price;
                                $scope.ItemDetails.buyingprice = $scope.ItemDetails.basebuyingprice;
                                $scope.ItemDetails.mrp = purchaseorder.mrp;
                                $scope.ItemDetails.sellingprice = purchaseorder.sellingprice;
                                $scope.ItemDetails.poinfo = [];
                                $scope.ItemDetails.poinfo.push(
                                    {
                                        'purchaseorderitemsid': purchaseorder.id,
                                        'sizeid': purchaseorder.sizeid,
                                        'designid': purchaseorder.designid,
                                        'pono': $scope.CurrentPO.pono,
                                        'aqty': purchaseorder.oqty,
                                        'oqty': purchaseorder.oqty
                                    });
                            }
                        }
                    }
                }
            });
        }

        $scope.SelectItem = function (item) {
            $scope.ItemDetails.Details.itemid = item.id;
            $scope.ItemDetails.Details.brandid = item.brandid;
            $scope.ItemDetails.Details.sizeid = item.sizeid;
            $scope.ItemDetails.Details.styleid = item.styleid;
            $scope.ItemDetails.Details.fitid = item.fitid;
            $scope.ItemDetails.Details.typeid = item.typeid;
            $scope.ItemDetails.Details.sleeveid = item.sleeveid;
            $scope.ItemDetails.Details.materialid = item.materialid;
            $scope.ItemDetails.Details.patternid = item.patternid;
            $scope.ItemDetails.Details.colourid = item.colourid;
            $scope.ItemDetails.Details.dealerprice = item.dealerprice;
            $scope.ItemDetails.Details.sellingname = item.printingname;
            $scope.ItemDetails.Details.productname = $scope.SearchRef(item.productid, $scope.ProductList);
            $scope.ItemDetails.mrp = item.mrp;
            $scope.ItemDetails.sellingprice = item.salerate;
            $scope.ItemDetails.basebuyingprice = item.purchaserate;
            $scope.ItemDetails.buyingprice = $scope.ItemDetails.basebuyingprice;
            $scope.ItemDetails.marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / $scope.GetNumber($scope.ItemDetails.basebuyingprice) * 100) - 100).round(2);
        }

        $scope.gproductChange = function () {
            $scope.productChange();
            var request = {
                "Item": {
                    'productid': $scope.ProductSearch.ProductID,
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
                    $scope.ItemList = result;
                    $timeout(function () {
                        $scope.setFocusgitemid();
                    }, 20);
                }
            });
        }

        $scope.productChange = function () {
            if ($scope.ProductSearch.ProductID != null) {
                var prditem = $scope.ProductList.find(function (item) {
                    return (item.id == $scope.ProductSearch.ProductID);
                });
                if (prditem || prditem != null) {
                    $scope.ItemDetails.productid = prditem.id;
                    $scope.ItemDetails.Details.productid = prditem.id;
                    $scope.ItemDetails.taxid = prditem.purchasetaxid;
                    $scope.ItemDetails.pricingmode = prditem.pricingmode;
                    $scope.ItemDetails.hsncode = prditem.hsncode;
                    $scope.ItemDetails.margin = prditem.margin;
                    $scope.ItemDetails.Details.sizeid = prditem.sellingmode == 3 ? -3 : -2;
                    $scope.taxChange();
                }

                if ($scope.Invoice.Lrinvoicetaxitems != null && $scope.Invoice.Lrinvoicetaxitems.length > 0) {
                    var _taxitem = $scope.Invoice.Lrinvoicetaxitems.find(function (item) {
                        return (item.taxamount > 0);
                    });
                    if (_taxitem == 'undefined' || _taxitem == null)
                        $scope.ItemDetails.taxid = $scope.Invoice.Lrinvoicetaxitems[0].taxid;
                }
            };
            $scope.fixPrice();
        };

        $scope.CurrentTaxConfig = null;
        $scope.taxChange = function () {
            $scope.ItemDetails.ptaxpercentage = 0;
            $scope.ItemDetails.staxpercentage = 0;
            if ($scope.ItemDetails.taxid != null) {
                var taxitem = $scope.purchaseTaxActual.find(function (item) {
                    return (item.id == $scope.ItemDetails.taxid);
                });
                if (taxitem || taxitem != null) {
                    $scope.CurrentTaxConfig = taxitem;
                    if (taxitem != null && typeof (taxitem.taxsplit.findTax))
                        taxitem.taxsplit.findTax = $scope.findTax;
                    $scope.fixTax();
                }
            };
            $scope.fixPurhcaseDiscount();
        };

        $scope.itemchange = function (attr, item) {
            if (item.IsParent) {
                $.each(item.Child, function (k, v) {
                    if (attr == 'designid') {
                        v.designid = item.designid;
                    } else if (attr == 'hsncode') {
                        v.hsncode = item.hsncode;
                    }
                })
            }
        };

        $scope.fixTax = function (findDiscount) {
            var taxitem = $scope.CurrentTaxConfig;
            if (taxitem == null && $scope.ItemDetails.taxid > 0) {
                $scope.taxChange();
                taxitem = $scope.CurrentTaxConfig;
            }
            if (typeof (findDiscount) == 'undefined' || findDiscount) {
                $scope.fixPurhcaseDiscount();
                if (taxitem == null) {
                    $scope.ItemDetails.purchasetaxpercentage = 0;
                    $scope.ItemDetails.salestaxpercentage = 0;
                    $scope.ItemDetails.salestax = 0;
                    $scope.ItemDetails.purchasetax = 0;
                    $scope.fixPrice();
                    return;
                }
            }

            $scope.fixPrice();
            var taxconfig = taxitem.taxsplit.findTax($scope.ItemDetails.buyingprice);
            if (taxconfig.cgst == 0) {
                taxconfig = { 'cgst': 0, 'sgst': 0 };
            }
            $scope.ItemDetails.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
            $scope.calculatesalestax();
            $scope.marginChanged();
        }

        $scope.findTax = function (amount) {
            if (this.items == null || this.items.length == 0)
                return null;

            if (this.items.length == 1)
                return this.items[0];

            if (isNaN(amount)) amount = 0;
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            return { 'cgst': 0, 'sgst': 0 };
        }

        $scope.fixPurhcaseDiscount = function () {
            var discountvalue = 0;
            var grossvalue = 0;
            var discountpercent = 0;
            if (typeof ($scope.Invoice.Lrinvoicetaxitems) == 'undefined' || $scope.Invoice.Lrinvoicetaxitems == null)
                $scope.Invoice.Lrinvoicetaxitems = [];

            /*var discountitem = $scope.Invoice.Lrinvoicetaxitems.find(function (k) {
                return Math.abs(k.taxid) == Math.abs($scope.ItemDetails.taxid);
            })*/

            $.each($scope.Invoice.Lrinvoicetaxitems, function (k, discountitem) {
                if ($scope.ItemDetails.taxid == discountitem.taxid && discountitem.type == 0) {
                    discountvalue += discountitem.discount;
                    grossvalue += discountitem.amounton;
                }
            });

            if (grossvalue > 0 && discountvalue > 0) {
                discountpercent = (discountvalue / grossvalue * 100).round(2);
            }
            $scope.ItemDetails.purchasediscountpercentage = (discountpercent + $scope.additionaldiscountpercentage).round(2);
        }

        $scope.fixPrice = function () {

            $scope.ItemDetails.purchasediscount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.ItemDetails.purchasediscountpercentage / 100).round(5);
            if (isNaN($scope.ItemDetails.purchasediscount))
                $scope.ItemDetails.purchasediscount = 0;

            $scope.ItemDetails.buyingprice = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) - $scope.ItemDetails.purchasediscount).round(2);

            //if ($scope.ItemDetails.pricingmode && $scope.ItemDetails.pricingmode != null && $scope.ItemDetails.pricingmode >= 0) {
            //    switch ($scope.ItemDetails.pricingmode) {
            //        case 0:
            //        case 1:
            //            if ($scope.ItemDetails.calculatedPrice == $scope.ItemDetails.sellingprice)
            //                $scope.ItemDetails.sellingprice = null;
            //            $scope.ItemDetails.calculatedPrice = 0;
            //            $scope.ItemDetails.readonly = false;
            //            break;
            //        case 2:
            //            $scope.ItemDetails.sellingprice = $scope.ItemDetails.mrp;
            //            $scope.ItemDetails.calculatedPrice = $scope.ItemDetails.sellingprice;
            //            $scope.ItemDetails.readonly = true;
            //            break;
            //        case 3:
            //            $scope.ItemDetails.sellingprice = $scope.findLastPrice();
            //            $scope.ItemDetails.calculatedPrice = $scope.ItemDetails.sellingprice;
            //            $scope.ItemDetails.readonly = $scope.ItemDetails.calculatedPrice > 0;
            //            break;
            //        case 4:
            //            $scope.ItemDetails.sellingprice = $scope.ItemDetails.buyingprice + $scope.ItemDetails.tax +
            //                ($scope.ItemDetails.buyingprice * $scope.ItemDetails.margin / 100);
            //            $scope.ItemDetails.calculatedPrice = $scope.ItemDetails.sellingprice;
            //            $scope.ItemDetails.readonly = $scope.ItemDetails.calculatedPrice > 0;
            //            break;
            //        case 5:
            //            $scope.ItemDetails.sellingprice = $scope.ItemDetails.buyingprice + $scope.ItemDetails.tax + $scope.ItemDetails.margin;
            //            $scope.ItemDetails.calculatedPrice = $scope.ItemDetails.sellingprice;
            //            $scope.ItemDetails.readonly = $scope.ItemDetails.calculatedPrice > 0;
            //            break;
            //    }
            //}
        };

        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };


        $scope.taxablevaluechanged = function () {
            var taxitem = $scope.PurchaseTax.find(function (item) {
                return item.code == $scope.InvoiceTaxItem.taxcode;
            });
            if (typeof (taxitem) != 'undefined' && taxitem != null) {
                $scope.InvoiceTaxItem.taxid = taxitem.id;
                $scope.InvoiceTaxItem.taxpercentage = taxitem.taxpercentage;
                var taxable = $scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) - $scope.GetNumber($scope.InvoiceTaxItem.discount);
                $scope.InvoiceTaxItem.taxamount = (taxable * $scope.GetNumber(taxitem.taxpercentage) / 100).round(2);
            }
        }

        $scope.CheckTaxValue = function (event) {
            if (typeof ($scope.InvoiceTaxItem.taxablevalue) == 'undefined'
                || $scope.InvoiceTaxItem.taxablevalue == null
                || $scope.InvoiceTaxItem.taxablevalue == '') {

                event.preventDefault();
                event.stopPropagation();
                $scope.setFocussno();
                return;
            }
        }

        $scope.AddTaxItem = function () {

            if (typeof ($scope.InvoiceTaxItem.discount) != 'undefined' &&
                $scope.InvoiceTaxItem.discount != null &&
                $scope.InvoiceTaxItem.discount.toString().indexOf('%') > 0) {

                var discountondiscount = 0;
                var discountpercent = 0;

                if ($scope.InvoiceTaxItem.discount.toString().indexOf('+') > 0) {
                    var discountparts = $scope.InvoiceTaxItem.discount.toString().split('+');
                    $scope.InvoiceTaxItem.discount = discountparts[0];
                    discountondiscount = $scope.GetNumber(discountparts[1].toString().replace('%', ''));
                }

                discountpercent = $scope.GetNumber($scope.InvoiceTaxItem.discount.toString().replace('%', ''));

                $scope.purchasediscountpercentage = discountpercent;
                $scope.InvoiceTaxItem.purchasediscountpercentage = discountpercent;

                if ($scope.Invoice.isinclusivediscount) {
                    $scope.InvoiceTaxItem.taxablevalue = ($scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) / (100 - discountpercent) * 100).round(2);
                }

                $scope.InvoiceTaxItem.discount = ($scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) * discountpercent / 100).round(2);

                if (discountondiscount > 0) {
                    var discountondiscountvalue = (($scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) - $scope.InvoiceTaxItem.discount) * discountondiscount / 100).round(2);
                    $scope.purchasediscountpercentage = ($scope.purchasediscountpercentage + (discountondiscountvalue / $scope.InvoiceTaxItem.taxablevalue * 100)).round(2);
                    $scope.InvoiceTaxItem.purchasediscountpercentage = $scope.purchasediscountpercentage;
                    $scope.InvoiceTaxItem.discount = ($scope.InvoiceTaxItem.discount + discountondiscountvalue).round(2);
                }

                $scope.taxablevaluechanged(false);
                return;
            }

            $scope.taxablevaluechanged(false);

            if (typeof ($scope.Invoice.Lrinvoicetaxitems) == 'undefined' || $scope.Invoice.Lrinvoicetaxitems == null)
                $scope.Invoice.Lrinvoicetaxitems = [];

            var taxitem = $scope.PurchaseTax.find(function (item) {
                return item.code == $scope.InvoiceTaxItem.taxcode;
            });

            var lrtaxitem = {
                'taxid': taxitem.id,
                'type': taxitem.type,
                'percentage': taxitem.taxpercentage,
                'display': taxitem.taxpercentage,
                'amounton': $scope.GetNumber($scope.InvoiceTaxItem.taxablevalue),
                'discount': $scope.InvoiceTaxItem.discount,
                'taxamount': $scope.InvoiceTaxItem.taxamount,
                'isactive': true,
                'netamount': $scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) +
                    $scope.GetNumber($scope.InvoiceTaxItem.taxamount) - $scope.GetNumber($scope.InvoiceTaxItem.discount)
            };
            $scope.Invoice.Lrinvoicetaxitems.push(lrtaxitem);
            $scope.ComputeTaxTotal();
            $scope.InvoiceTaxItem.taxablevalue = null;
            $scope.InvoiceTaxItem.discount = null;
            $scope.InvoiceTaxItem.taxamount = null;
            $scope.setFocustaxid();
        }

        $scope.ComputeTaxTotal = function () {
            var netamount = 0; var netamounton = 0; var nettax = 0;
            var netdiscount = 0; var netdiscountpercent = 0;
            var othercharge = 0; var othertax = 0; var otherdiscount = 0;
            $.each($scope.Invoice.Lrinvoicetaxitems, function (k, v) {
                if (v.type == 8) {
                    othertax += v.taxamount;
                    otherdiscount += $scope.GetNumber(v.discount);
                    othercharge += v.amounton;
                }
                else {
                    netamount += v.netamount;
                    netamounton += v.amounton;
                    nettax += v.taxamount;
                    netdiscount += $scope.GetNumber(v.discount);
                }
                if ($scope.GetNumber(v.discount) > 0) {
                    v.discountpercentage = ($scope.GetNumber(v.discount) / v.amounton * 100).round(2);
                }
            });
            $scope.Invoice.otheramount = othercharge;
            $scope.Invoice.billvalue = netamounton;
            $scope.Invoice.taxamount = nettax + othertax;
            $scope.Invoice.purchasediscount = netdiscount + otherdiscount;
            $scope.Invoice.purchasediscountpercentage = (netdiscount / netamounton * 100).round(2);
            $scope.Invoice.totalamount = (netamount + othertax - otherdiscount + $scope.GetNumber($scope.Invoice.otheramount)).round(2);
            //$scope.Invoice.payableamount = $scope.Invoice.totalamount - $scope.GetNumber($scope.Invoice.debitnotediscount);
        }

        $scope.AddItem = function () {
            if (typeof ($scope.ProductSearch.ProductID) == 'undefined' || $scope.ProductSearch.ProductID == 0) {
                alert('Please select the product');
                return;
            }

            var duplicatesno = $scope.Invoice.Lrinvoiceitems.find(function (item) {
                return item.sno == $scope.ItemDetails.sno;
            });

            if (typeof (duplicatesno) != 'undefined' && duplicatesno != null) {
                $rootScope.ShowAlert('Please enter different serial number', 'Duplicate serial no');
                return;
            };

            $scope.ItemDetails.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.Details.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.Details.productname = $scope.SearchRef($scope.ProductSearch.ProductID, $scope.ProductList);
            if (typeof ($scope.ItemDetails.productid) == 'undefined' || $scope.ItemDetails.productid == null || $scope.ItemDetails.productid < 0) {
                $rootScope.Toaster.Info("Please select the product!");
                return;
            }

            if ($scope.ItemDetails.poinfo &&
                $scope.ItemDetails.poinfo != null &&
                $scope.ItemDetails.poinfo.length == 1) {
                $scope.ItemDetails.poinfo[0].aqty = $scope.ItemDetails.buyingqty;
            }

            if ($scope.ItemDetails != null && $scope.ItemDetails.Details != null
                && $scope.ItemDetails.Details.sizeid == -2) {
                $scope.CreateMultiItems($scope.ItemDetails, true, false);
            }
            else if ($scope.ItemDetails != null && $scope.ItemDetails.Details != null
                && $scope.ItemDetails.Details.sizeid == -3) {
                $scope.CreateMultiItems($scope.ItemDetails, false, false);
            } else {
                var productitem = $scope.ProductList.find(function (item) {
                    return item.id == $scope.ItemDetails.productid;
                });
                if (productitem != null && productitem.sellingmode == 3) {
                    $scope.CreateMultiItems($scope.ItemDetails, false, false);
                }
                else {
                    var buyingqty = $scope.ItemDetails.buyingqty;
                    if (typeof (buyingqty) != 'undefined' && buyingqty != null)
                        buyingqty = buyingqty.toString();

                    if (buyingqty.indexOf('*') >= 0) {
                        var qtyparts = buyingqty.split('*');
                        if (qtyparts.length == 2) {
                            var qty = $scope.GetNumber(qtyparts[1]);
                            var pieces = $scope.GetNumber(qtyparts[0]);
                            if (qty > 0 && pieces > 0) {
                                $scope.AddMultipleItemInternal(qty, pieces);
                                $scope.CreateNew();
                            }
                        }
                    }
                    else {

                        $scope.AddItemInternal(true);
                        //$scope.ItemDetails.buyingamount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);
                        //$scope.ItemDetails.amount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);
                        $scope.CreateNew();
                    }

                }
            }
        };

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.Invoice', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.Invoice = value;
        };

        $scope.New = function () {
            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
            $scope.Invoice = (JSON.parse(JSON.stringify($scope.InvoiceTemplate)));
            $scope.Invoice.bominvoicetaxitems = [];
            $scope.Invoice.Lrentrydetails = [];
            $scope.CurrentPO = null;
            $scope.ShowEditableGrid();
        }

        $scope.CreateNew = function () {

            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
            $scope.ItemDetails.buyingqty = null;
            $scope.ItemDetails.buyingprice = null;
            $scope.ItemDetails.basebuyingprice = null;
            $scope.ItemDetails.sellingprice = null;
            $scope.ItemDetails.mrp = null;
            $scope.ItemDetails.purchasetax = null;
            $scope.ItemDetails.salestax = null;
            $scope.ItemDetails.purchasediscount = null;
            $scope.ItemDetails.purchasediscountpercentage = null;
            $scope.ItemDetails.Details.serialno = '';
            $scope.ItemDetails.Details.sellingname = '';
            $scope.ItemDetails.Details.hsncode = '';
            $scope.ItemDetails.Details.freeqty = '';
            //$scope.ProductSearch.ProductID = 0;

            
            //$scope.ItemDetails.salediscount = null;
            //$scope.ItemDetails.marginpercentage = null;
            //$scope.ItemDetails.salediscountpercentage = null;

            $scope.ItemDetails.IsChild = false;
            $scope.ItemDetails.IsParent = false;
            if ($scope.Invoice.Lrinvoiceitems.length > 0)
                $scope.ItemDetails.sno = parseInt($scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].sno) + 1;
            else
                $scope.ItemDetails.sno = 1;

            if ($scope.Invoice.ptype == 1) {
                $scope['setFocushsno']();
            }
            else {
                $scope['setFocussno']();
            }

            $scope.LocalContext.Post('$scope.Invoice', $scope.Invoice, false);
            if ($scope.egrid == null) {
                $timeout(function () {
                    $scope.ShowEditableGrid();
                }, 10);
            }
            else {
                $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
            }
        }

        $scope.defaultTaxID = function () {
            if ($scope.Invoice.Lrinvoicetaxitems.length > 0)
                return $scope.Invoice.Lrinvoicetaxitems[0].taxid;
            return null;
        };

        $scope.LoadInvoice = function () {
            $scope.InvoiceInfo.invoiceid = $scope.ProductSearch.currentInvoiceID;
            $scope.InitializeInvoiceList(true);
        };

        $scope.AddMultipleItemInternal = function (qty, pieces) {
            $scope.ItemDetails.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.Details.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.searchtext = $scope.ProductSearch.SearchItemName;
            $scope.ItemDetails.sno = parseFloat($scope.ItemDetails.sno);

            if (typeof ($scope.ItemDetails.itemid) == 'undefined' || $scope.ItemDetails.itemid == 0) {
                $scope.ItemDetails.Details.printingname = $scope.SearchRef($scope.ItemDetails.productid, $scope.ProductList, '');
            }

            if (typeof ($scope.Invoice.Lrinvoiceitems) == 'undefined')
                $scope.Invoice.Lrinvoiceitems = [];
            $scope.ItemDetails.buyingqty = qty;
            $scope.fixTax();
            $scope.ItemDetails.buyingamount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);
            $scope.ItemDetails.amount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);

            for (var i = 0; i < pieces; i++) {
                var newitem = JSON.parse(JSON.stringify($scope.ItemDetails));
                newitem.sno = newitem.sno + (i / 10);
                $scope.Invoice.Lrinvoiceitems.push(newitem);
            }
            $scope.CalculateTotal();
        };

        $scope.RowValuechanged = function (item) {

            if (item.purchasediscount.toString().indexOf('%') > 0) {
                item.purchasediscount = item.purchasediscount.toString().replace('%', '');
                item.purchasediscount = ($scope.GetNumber(item.buyingprice) * $scope.GetNumber(item.purchasediscount) / 100).round(2);
            }
            item.buyingprice = item.basebuyingprice - item.purchasediscount
            var taxitem = $scope.purchaseTaxActual.find(function (pitem) {
                return (pitem.id == item.taxid);
            });
            if (taxitem || taxitem != null) {
                taxitem.taxsplit.findTax = $scope.findTax;
                var taxconfig = taxitem.taxsplit.findTax(item.buyingprice);
                if (taxconfig.cgst == 0) {
                    taxconfig = { 'cgst': 0, 'sgst': 0 };
                }
                item.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }

            item.amount = $scope.GetNumber(item.basebuyingprice) * $scope.GetNumber(item.buyingqty);
            item.purchasetotaldiscount = ($scope.GetNumber(item.purchasediscount) * $scope.GetNumber(item.buyingqty)).round(2);
            item.amount -= item.purchasetotaldiscount;
            item.purchasetax = (item.amount * item.purchasetaxpercentage / 100).round(2);
            $scope.CalculateTotal();
        }

        $scope.AddItemInternal = function (computetotal) {
            $scope.ItemDetails.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.Details.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.searchtext = $scope.ProductSearch.SearchItemName;
            $scope.ItemDetails.sno = parseFloat($scope.ItemDetails.sno);
            $scope.ItemDetails.amount = $scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty);

            if ($scope.ItemDetails.purchasediscountpercentage == undefined ||
                $scope.ItemDetails.purchasediscountpercentage == null |
                $scope.ItemDetails.purchasediscountpercentage == '') {
                if ($scope.Invoice.purchasediscountpercentage &&
                    $scope.Invoice.purchasediscountpercentage != null &&
                    $scope.Invoice.purchasediscountpercentage != '' &&
                    $scope.GetNumber($scope.Invoice.purchasediscountpercentage) > 0) {
                    $scope.ItemDetails.purchasediscountpercentage = $scope.GetNumber($scope.Invoice.purchasediscountpercentage);
                    }
            }

            $scope.ItemDetails.purchasetotaldiscount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty) * $scope.ItemDetails.purchasediscountpercentage / 100).round(2);
            $scope.ItemDetails.amount -= $scope.ItemDetails.purchasetotaldiscount;
            $scope.ItemDetails.purchasediscount = ($scope.ItemDetails.purchasetotaldiscount / $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);

            var taxitem = $scope.purchaseTaxActual.find(function (pitem) {
                return (pitem.id == $scope.ItemDetails.taxid);
            });
            if (taxitem || taxitem != null) {
                taxitem.taxsplit.findTax = $scope.findTax;
                var taxconfig = taxitem.taxsplit.findTax($scope.ItemDetails.buyingprice);
                if (taxconfig.cgst == 0) {
                    taxconfig = { 'cgst': 0, 'sgst': 0 };
                }
                $scope.ItemDetails.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }


            $scope.ItemDetails.purchasetax = ($scope.ItemDetails.amount * $scope.ItemDetails.purchasetaxpercentage / 100).round(2);

            if (typeof ($scope.ItemDetails.Details.sizeid) != 'undefined' && $scope.ItemDetails.Details.sizeid > 0) {
                $scope.ItemDetails.Details.sizename = restService.SearchReference($scope.ItemDetails.Details.sizeid, $scope.SizeList);
            }

            if (isNaN($scope.ItemDetails.sno)) {
                if ($scope.Invoice.Lrinvoiceitems.length == 0)
                    $scope.ItemDetails.sno = 1;
                else
                    $scope.ItemDetails.sno = $scope.Invoice.Lrinvoiceitems[0].sno + 1;
            }

            if (typeof ($scope.ItemDetails.productid) == 'undefined' || $scope.ItemDetails.productid == null || $scope.ItemDetails.productid < 0) {
                $rootScope.Toaster.Info("Please select the product!");
                return;
            }

            if (typeof ($scope.ItemDetails.itemid) == 'undefined' || $scope.ItemDetails.itemid == 0) {
                $scope.ItemDetails.Details.printingname = $scope.SearchRef($scope.ItemDetails.productid, $scope.ProductList, '');
            }

            if (typeof ($scope.Invoice.Lrinvoiceitems) == 'undefined')
                $scope.Invoice.Lrinvoiceitems = [];

            var currentdate = new Date();
            $scope.ItemDetails.uniqueid = currentdate.getTime();
            if ($scope.Invoice.Lrinvoiceitems.indexOf($scope.ItemDetails) < 0) {
                if ($scope.Invoice.Lrinvoiceitems.length > 0) {
                    var index = 0;
                    $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                        if (v.sno < $scope.ItemDetails.sno) {
                            index = k;
                        }
                    });
                    if (index + 1 < $scope.Invoice.Lrinvoiceitems.length) {
                        if ($scope.Invoice.Lrinvoiceitems[index + 1].sno == $scope.ItemDetails.sno) {
                            index += 1;
                            while (index < $scope.Invoice.Lrinvoiceitems.length &&
                                $scope.Invoice.Lrinvoiceitems[index].sno == $scope.ItemDetails.sno) {
                                index += 1;
                            }
                        }
                        if (index < $scope.Invoice.Lrinvoiceitems.length)
                            $scope.Invoice.Lrinvoiceitems.splice(index + 1, 0, $scope.SetRowStyle($scope.ItemDetails));
                        else
                            $scope.Invoice.Lrinvoiceitems.push($scope.SetRowStyle($scope.ItemDetails));
                    }
                    else {
                        $scope.Invoice.Lrinvoiceitems.push($scope.SetRowStyle($scope.ItemDetails));
                    }
                }
                else {
                    $scope.Invoice.Lrinvoiceitems.push($scope.ItemDetails);
                }
            }

            if ($scope.Invoice.ptype == 3 &&
                (!$scope.ItemDetails.IsParent ||
                  $scope.ItemDetails.IsChild == undefined)) {

                if ($scope.Invoice.Lrinvoicetaxitems == undefined ||
                    $scope.Invoice.Lrinvoicetaxitems == null)
                    $scope.Invoice.Lrinvoicetaxitems = [];

                var lrtaxitem = {
                    'taxid': $scope.ItemDetails.taxid,
                    'type': 1,
                    'percentage': $scope.ItemDetails.purchasetaxpercentage,
                    'display': $scope.ItemDetails.purchasetaxpercentage,
                    'amounton': $scope.ItemDetails.amount,
                    'discount': $scope.ItemDetails.purchasetotaldiscount,
                    'taxamount': $scope.ItemDetails.purchasetax,
                    'isactive': true,
                    'netamount': $scope.ItemDetails.amount + $scope.ItemDetails.purchasetax
                };

                var exist = $scope.Invoice.Lrinvoicetaxitems.find(function (item) {
                    return item.taxid == lrtaxitem.taxid &&
                        item.percentage == lrtaxitem.percentage &&
                        item.type == lrtaxitem.type
                })

                if (exist && exist != null) {
                    exist.amounton += lrtaxitem.amounton;
                    exist.discount += lrtaxitem.discount;
                    exist.taxamount += lrtaxitem.taxamount;
                    exist.netamount += lrtaxitem.netamount;
                }
                else {
                    $scope.Invoice.Lrinvoicetaxitems.push(lrtaxitem);
                }
                $scope.ComputeTaxTotal();
            }

            //$scope.ItemDetails.amount += $scope.ItemDetails.purchasetotaldiscount;
            if (typeof (computetotal) == 'undefined' || computetotal)
                $scope.CalculateTotal();
        };

        $scope.SetRowStyle = function (item) {

            //var item = (JSON.parse(JSON.stringify(item)));

            if (typeof (item.poinfo) != 'undefined' &&
                item.poinfo != null && item.poinfo.length > 0)
                item.style = { "background-color": "lightgrey" };
            else
                item.style = {};
            return item;

        }

        $scope.RoundPrice = function (amount) {

            if (!$scope.SupportRounding)
                return amount.round(0);

            if ($scope.PurchaseRounding != '') {
                amount = Math.round(amount, 0);

                /*$scope.RoundingMode = [{ 'id': 0, 'name': 'None' }, { 'id': 1, 'name': 'Rounding-5' } ,
     * { 'id': 2, 'name': 'Down-5' },
    { 'id': 3, 'name': 'Up-5' }, { 'id': 4, 'name': 'Down-10' }, { 'id': 5, 'name': 'Up-10' },
    { 'id': 6, 'name': 'Rounding-10' },
    { 'id': 7, 'name': 'Down-1' },{ 'id': 8, 'name': 'Up-1' },
    { 'id': 9, 'name': 'Rounding-1' }];*/
                switch ($scope.PurchaseRounding) {
                    case 1:
                        var diff = amount - (Math.floor(amount / 10) * 10);
                        if (diff > 5) {
                            if (diff - 5 >= 2.5) {
                                amount = (Math.floor(amount / 10) * 10) + 10;
                            }
                            else {
                                amount = (Math.floor(amount / 10) * 10) + 5;
                            }
                        }
                        else {
                            if (diff >= 2.5) {
                                amount = (Math.floor(amount / 10) * 10) + 5;
                            }
                            else {
                                amount = (Math.floor(amount / 10) * 10);
                            }
                        }
                        break;
                    case 6:
                        var diff = amount - (Math.floor(amount / 10) * 10);
                        if (diff > 5) {
                            amount = (Math.floor(amount / 10) * 10) + 10;
                        }
                        else {
                            amount = (Math.floor(amount / 10) * 10);
                        }
                        break;
                }
            }

            if (amount < 100) {
                amount = Math.round(amount, 0);
            }
            if (amount > 100 && amount < 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    if (diff - 5 >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 10;
                    }
                    else {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    }
                }
                else {
                    if (diff >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    }
                    else {
                        amount = (Math.floor(amount / 10) * 10);
                    }
                }

            } else if (amount > 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    amount = (Math.floor(amount / 10) * 10) + 10;
                }
                else {
                    amount = (Math.floor(amount / 10) * 10);
                }
            }
            return amount;
        }

        $scope.CheckFocus = function (nextfocus) {
            if (nextfocus == 'brandid') {
                if ($scope.SupportHyperApplication) {
                    $scope.ItemDetails.Details.sizeid = 0;
                    $scope.ItemDetails.Details.colourid = 0;
                    $scope.ItemDetails.Details.brandid = 0;
                    $scope.setFocushsncode();
                }
                else {
                    $scope.setFocusbrandid();
                }
            }
        }

        $scope.sellingpriceChange = function (syncmrp) {
            var value = $scope.ItemDetails.sellingprice;
            if (!$scope.SupportSalepriceMrpdifference && typeof (syncmrp) != 'undefined' && syncmrp)
                $scope.ItemDetails.mrp = value;

            if ($scope.ItemDetails.buyingprice == null || isNaN($scope.ItemDetails.buyingprice)) {
                $scope.ItemDetails.buyingprice = $scope.GetNumber($scope.ItemDetails.basebuyingprice);
                if ($scope.ItemDetails.purchasetaxpercentage > 0 && $scope.Invoice.isinclusivetax) {
                    $scope.ItemDetails.buyingprice = $scope.ItemDetails.buyingprice / (100 + $scope.ItemDetails.purchasetaxpercentage) * 100;
                }
            }

            if ($scope.SupportMarginexcludetax) {
                var salestaxpercentage = $scope.ItemDetails.salestaxpercentage;
                var taxitem = $scope.CurrentSalesTaxConfig;
                if (taxitem != null) {
                    if (taxitem.taxsplit.findTax == undefined || taxitem.taxsplit.findTax == null)
                        taxitem.taxsplit.findTax = $scope.findTax;
                    var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.sellingprice));
                    salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
                }

                var buyingprice = $scope.SupportMarginOnCost ? $scope.GetNumber($scope.ItemDetails.basebuyingprice) : $scope.GetNumber($scope.ItemDetails.buyingprice);

                if ($scope.SupportMarginincludetax) {
                    var newtaxvalue = buyingprice * ($scope.ItemDetails.purchasetaxpercentage) / 100;
                    buyingprice = newtaxvalue + buyingprice;
                }

                if ($scope.SupportAddlrexptomargion) {

                    if ($scope.Invoice.expenseperpiece > 0)
                        buyingprice = buyingprice + $scope.Invoice.expenseperpiece;

                    if ($scope.Invoice.expensepercentage > 0)
                        buyingprice = buyingprice + (buyingprice * $scope.Invoice.expensepercentage / 100).round(0);
                }

                if (typeof ($scope.Invoice.premargin) != 'undefined' && $scope.Invoice.premargin > 0) {
                    buyingprice += (buyingprice * $scope.Invoice.premargin / 100).round(0);
                }

                if (typeof ($scope.ItemDetails.rowexpense) != 'undefined' && $scope.ItemDetails.rowexpense > 0) {
                    buyingprice = buyingprice + (buyingprice * $scope.ItemDetails.rowexpense / 100).round(0);
                }

                var asellingprice = $scope.GetNumber($scope.ItemDetails.sellingprice) / (salestaxpercentage + 100) * 100;

                var marginpercentage = ((asellingprice - buyingprice) / buyingprice * 100).round(2);
                if ($scope.SupportMarkDOWN)
                    marginpercentage = (100 - ((buyingprice / asellingprice) * 100)).round(2);


                $scope.ItemDetails.marginpercentage = marginpercentage;
            } else {
                var buyingprice = $scope.SupportMarginOnCost ? $scope.GetNumber($scope.ItemDetails.basebuyingprice) : $scope.GetNumber($scope.ItemDetails.buyingprice);

                if ($scope.SupportMarginincludetax) {
                    var newtaxvalue = buyingprice * ($scope.ItemDetails.purchasetaxpercentage) / 100;
                    buyingprice = newtaxvalue + buyingprice;
                }

                if ($scope.SupportAddlrexptomargion) {

                    if ($scope.Invoice.expenseperpiece > 0)
                        buyingprice = buyingprice + $scope.Invoice.expenseperpiece;

                    if ($scope.Invoice.expensepercentage > 0)
                        buyingprice = buyingprice + (buyingprice * $scope.Invoice.expensepercentage / 100).round(0);
                }

                if (typeof ($scope.Invoice.premargin) != 'undefined' && $scope.Invoice.premargin > 0) {
                    buyingprice += (buyingprice * $scope.Invoice.premargin / 100).round(0);
                }

                if (typeof ($scope.ItemDetails.rowexpense) != 'undefined' && $scope.ItemDetails.rowexpense > 0) {
                    buyingprice = buyingprice + (buyingprice * $scope.ItemDetails.rowexpense / 100).round(0);
                }

                var marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / buyingprice * 100) - 100).round(2);

                $scope.ItemDetails.marginpercentage = marginpercentage;
                if ($scope.SupportMarkDOWN)
                    marginpercentage = (100 - ((buyingprice / $scope.ItemDetails.sellingprice) * 100)).round(2);

                $scope.ItemDetails.marginpercentage = marginpercentage;
                $scope.ItemDetails.rowexpensevalue = buyingprice - $scope.ItemDetails.buyingprice;
            }

            var taxitem = $scope.CurrentSalesTaxConfig;

            if (taxitem == null) {
                $scope.ItemDetails.salestaxpercentage = 0;
            } else {
                var taxconfig = taxitem.taxsplit.findSalesTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }
            $scope.salediscountChanged(undefined, false);
        }

        $scope.marginChanged = function () {
            var baseprice = 0;
            if ($scope.SupportMarginOnCost) {
                baseprice = $scope.GetNumber($scope.ItemDetails.basebuyingprice);
            } else {
                baseprice = $scope.ItemDetails.buyingprice;
            }

            var marginpercentage = $scope.GetNumber($scope.ItemDetails.marginpercentage);

            if ($scope.SupportMarkDOWN) {
                var markpercent = 1 + (marginpercentage / (100 - marginpercentage));
                $scope.ItemDetails.sellingprice = (baseprice * markpercent).round(2);
            } else {
                $scope.ItemDetails.sellingprice = baseprice + (baseprice * marginpercentage / 100).round(2);
            }

            if ($scope.SupportMarginexcludetax) {
                var newtaxvalue = $scope.GetNumber($scope.ItemDetails.sellingprice) * $scope.ItemDetails.salestaxpercentage / 100;
                var newsellingprice = $scope.RoundPrice(newtaxvalue + $scope.ItemDetails.sellingprice);
                var taxitem = $scope.CurrentTaxConfig;
                if (taxitem != null) {
                    var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.sellingprice));
                    if ($scope.ItemDetails.salestaxpercentage < taxconfig.cgst + taxconfig.sgst) {
                        var newtaxvalue = $scope.GetNumber($scope.ItemDetails.sellingprice) * (taxconfig.cgst + taxconfig.sgst) / 100;
                        newsellingprice = $scope.RoundPrice(newtaxvalue + $scope.ItemDetails.sellingprice);
                    }
                }
                $scope.ItemDetails.sellingprice = newsellingprice;
            } else {
                $scope.ItemDetails.sellingprice = $scope.RoundPrice($scope.ItemDetails.sellingprice);
            }
            $scope.ItemDetails.mrp = $scope.ItemDetails.sellingprice;
            $scope.salediscountChanged();
        };


        $scope.salediscountChanged = function () {
            $scope.ItemDetails.salediscount = Math.round($scope.GetNumber($scope.ItemDetails.sellingprice) * $scope.GetNumber($scope.ItemDetails.salediscountpercentage) / 100, 0);
            $scope.ItemDetails.purchasetax = ($scope.GetNumber($scope.ItemDetails.buyingqty) * $scope.ItemDetails.buyingprice * $scope.ItemDetails.purchasetaxpercentage / 100).round(2);
            $scope.calculatesalestax();
        };

        $scope.calculatesalestax = function () {
            var taxitem = $scope.CurrentTaxConfig;
            if (taxitem == null) {
                $scope.ItemDetails.salestaxpercentage = 0;
            }
            else {
                var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }

            $scope.ItemDetails.salestax = ($scope.GetNumber($scope.ItemDetails.buyingqty) * ($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount) / ($scope.ItemDetails.salestaxpercentage + 100) * $scope.ItemDetails.salestaxpercentage).round(2);
        };



        $scope.CalculateTotal = function () {
            $scope.Total = {
                "buyingqty": 0,
                "buyingpiece": 0,
                "mrp": 0,
                "purchasediscount": 0,
                "sellingprice": 0,
                "buyingprice": 0,
                "basebuyingprice": 0,
                "tax": 0,
                "amount": 0
            };

            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {

                var productitem = $scope.ProductList.find(function (item) {
                    return item.id == v.productid;
                });

                if (typeof (productitem) != 'undefined' && productitem != null)
                    v.Details.productname = productitem.name;
                else
                    v.Details.productname = 'PRODUCT NOT FOUND';

                if (typeof (v.IsChild) == 'undefined' || v.IsChild || (v.IsChild == false && v.IsParent == false)) {

                    v.buyingqty = parseFloat(v.buyingqty);
                    v.buyingprice = parseFloat(v.buyingprice);
                    v.sellingprice = parseFloat(v.sellingprice);

                    if (isNaN(v.buyingprice)) v.buyingprice = 0.0;
                    if (isNaN(v.buyingqty)) v.buyingqty = 0.0;
                    if (isNaN(v.sellingprice)) v.sellingprice = 0.0;

                    $scope.Total.mrp += (v.mrp * v.buyingqty);
                    $scope.Total.buyingprice += (v.buyingprice * v.buyingqty);
                    $scope.Total.basebuyingprice += (v.basebuyingprice * v.buyingqty);
                    $scope.Total.purchasediscount += v.purchasetotaldiscount;
                    $scope.Total.sellingprice += (v.sellingprice * v.buyingqty);
                    $scope.Total.buyingqty += v.buyingqty;
                    $scope.Total.buyingpiece += (productitem.sellingmode != 3 ? v.buyingqty : 1);
                    $scope.Total.tax += v.purchasetax;
                    $scope.Total.amount += v.amount;
                }
            });

            $scope.Total.tax = $scope.Total.tax.round(2);
            $scope.Total.amount = $scope.Total.amount.round(2);
            $scope.Total.mrp = $scope.Total.mrp.round(2);
            $scope.Total.buyingprice = $scope.Total.buyingprice.round(2);
            $scope.Total.sellingprice = $scope.Total.sellingprice.round(2);
            $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);

            $scope.Invoice.baseamount = $scope.Total.buyingprice.round(2);
            $scope.Invoice.grossamount = $scope.Total.basebuyingprice.round(2);
            $scope.Invoice.discount = $scope.Total.purchasediscount.round(2);
        };

        $scope.InvoiceList = [];
        $scope.ShowInvoiceList = false;
        //if ($scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
        //    $scope.ShowInvoiceList = true;
        //    $scope.InvoiceList.push($scope.InvoiceInfo.invoiceid);
        //}


        $scope.InitializeInvoiceList = function (loaddirect) {

            request = {
                "Item": { id: $scope.InvoiceInfo.invoiceid },
                "SecurityToken": "String content"
            }

            if (!loaddirect && typeof ($scope.InvoiceInfo.invoiceid) != 'undefined' && $scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
                request.Item.ids = $scope.InvoiceInfo.invoiceid;
                request.Item.id = 0;
            }

            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: request
            }).success(function (result) {

                if (typeof (request.Item.ids) != 'undefined') {
                    $scope.InvoiceList = result;
                    $scope.ShowInvoiceList = true;
                }
                else {
                    $scope.ShowSearch = false;
                    $scope.DisplayInvoice(result.Items[0]);
                    $scope.Invoice.lrid = result.lrid;
                    $scope.Invoice.lrno = result.lrno;
                    $scope.Invoice.lrdate = result.lrdate;
                    $scope.Invoice.lrentryno = result.lrentryno;
                    $scope.Invoice.invoiceno = result.Items[0].invoiceno;
                    $scope.Invoice.status = result.status;
                    $scope.Invoice.supplierid = result.supplierid;
                    $scope.Invoice.suppliercompanyname = result.suppliercompanyname;
                    $scope.Invoice.companyname = result.companyname;
                    $scope.Invoice.transportid = result.transportid;
                    $scope.Invoice.retaillocationid = result.retaillocationid;
                    $scope.Invoice.entrylocationid = result.retaillocationid;
                    $scope.Invoice.bundles = result.noofbundels;
                    $scope.Invoice.lrnoInfo = result.lrentryno + " / " + result.lrno;
                    $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
                    $scope.CalculateTotal();
                    if ($scope.Invoice.Lrinvoiceitems.length > 0) {
                        $scope.ItemDetails.sno = $scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].sno + 1;
                    }
                    if (result.modeofdelivery == 1 && result.statusid < 202) {
                        $rootScope.ShowAlert('InBox', "Lorry delivery can't add products until contains issue & receive.");
                    }
                    if ($scope.egrid == null) {
                        $timeout(function () {
                            $scope.ShowEditableGrid();
                            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                        }, 10);
                    }
                    else {
                        $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                    }
                    $scope.CompanyChanged();
                    $scope.Invoice.retaillocationid = result.retaillocationid;
                    $scope.Invoice.entrylocationid = result.retaillocationid;
                }
            }).error(function (result) {
            });


        };

        if (typeof ($scope.InvoiceInfo.invoiceid) == 'undefined') {
            $scope.SwitchPanel();
        }
        else if ($scope.InvoiceInfo.invoiceid > 0 || $scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
            $scope.InitializeInvoiceList(false);
        }

        var itemsCache = [];

        $scope.findItems = function () {
            if (typeof ($scope.ItemDetails.Details.brandid) == 'undefined' ||
                typeof ($scope.ItemDetails.Details.typeid) == 'undefined' ||
                typeof ($scope.ProductSearch.ProductID) == 'undefined' ||
                typeof (itemsCache['' + $scope.ItemDetails.Details.brandid + ':' + $scope.ItemDetails.Details.typeid + ':' + $scope.ProductSearch.ProductID]) != 'undefined')
                return;

            var request = {
                "Item": { brandid: $scope.ItemDetails.Details.brandid, typeid: $scope.ItemDetails.Details.typeid, productid: $scope.ProductSearch.ProductID },
                "SecurityToken": "String content"
            }
            $http({
                method: 'POST',
                url: "ItemsService/Search",
                data: request
            }).success(function (result) {
                if (result.length > 0) {
                    $scope.ItemDetails.itemid = result[0].id;
                    itemsCache[$scope.ItemDetails.Details.brandid.toString() + ':' + $scope.ProductSearch.ProductID] = result;
                }
            }).error(function (result) {
            });
        };

        $scope.SupplierChanged = function () {
            if (typeof ($scope.Invoice.suppliercompanyid) == 'undefined' ||
                $scope.Invoice.suppliercompanyid == null) {
                $scope.Invoice.lrsupplierid = 0;
            }

            $scope.Invoice.lrsupplierid = $scope.SupplierCompany.find(function (item) {
                return item.id == $scope.Invoice.suppliercompanyid;
            }).supplierid;
        }

        $scope.Save = function (movenext) {
            //Total.amount + Total.tax + Invoice.otheramount
            if ($scope.Invoice.totalamount != ($scope.Total.amount + $scope.Total.tax + $scope.GetNumber($scope.Invoice.otheramount))) {
                var diffvalue = Math.abs(Math.abs($scope.Invoice.totalamount) - Math.abs($scope.Total.amount + $scope.Total.tax + $scope.GetNumber($scope.Invoice.otheramount)));
                if (diffvalue > 1) {
                    $rootScope.Confirm('Invoice Value Mismatch', diffvalue + ' amount different in invoice amount do you want to continue', function () {
                        $scope.ActualSave(movenext);
                    });
                }
                else {
                    $scope.ActualSave(movenext);
                }
            }
            else {
                $scope.ActualSave(movenext);
            }
        }

        $scope.ActualSave = function (movenext) {
            if ($scope.SaveRequest) return;

            $scope.Invoice.isdirectpurchase = true;

            if (typeof ($scope.Invoice.companyid) == 'undefined' || $scope.Invoice.companyid == null || $scope.Invoice.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select buying company name");
                return;
            }


            if ($scope.Invoice.suppliercompanyid == null || $scope.Invoice.suppliercompanyid == 0) {
                $scope['setFocussuppliercompanyid']();
                $rootScope.Toaster.Info("Please select supplier company");
                return;
            }
            if ($scope.Invoice.invoiceno == null || $scope.Invoice.invoiceno == 0) {
                $scope['setFocusinvoiceno']();
                $rootScope.Toaster.Info("Please select Invoice No");
                return;
            }
            if ($scope.Invoice.invoicedate == null || $scope.Invoice.invoicedate == "") {
                $scope['setFocusinvoicedate']();
                $rootScope.Toaster.Info("Please select Invoice Date");
                return;
            }

            if ($scope.Invoice.invoicedate > new Date()) {
                $scope['setFocusinvoicedate']();
                $rootScope.Toaster.Info("Invoice date can't be future date");
                return;
            }
            var suppliercompany = $scope.SupplierCompany.find(function (item) {
                return item.id == $scope.Invoice.suppliercompanyid;
            });

            var companycompany = $scope.BuyingCompany.find(function (item) {
                return item.id == $scope.Invoice.companyid;
            });

            if (suppliercompany && suppliercompany.gst && companycompany && companycompany.gstno) {
                var companystate = companycompany.gstno.substring(0, 2);
                var suppliercompanystate = suppliercompany.gst.substring(0, 2);
                if (companystate != suppliercompanystate && !$scope.Invoice.isinterstatesale) {
                    $rootScope.Toaster.Info("Different state transaction,check IGST");
                    return;
                }

                if (companystate == suppliercompanystate && $scope.Invoice.isinterstatesale) {
                    $rootScope.Toaster.Info("Same state transaction,check IGST");
                    return;
                }
            }

            if ($scope.Invoice.totalamount == null || $scope.Invoice.totalamount == 0) {
                $scope['setFocusbillvalue']();
                $rootScope.Toaster.Info("Please enter Bill value");
                return;
            }

            $scope.SupplierChanged();
            var cleaninvoice = (JSON.parse(JSON.stringify($scope.Invoice)));
            var removeList = [];
            $.each(cleaninvoice.Lrinvoiceitems, function (k, v) {
                if (v.IsParent)
                    removeList.push(v);
            });

            $.each(removeList, function (k, v) {
                cleaninvoice.Lrinvoiceitems.splice(cleaninvoice.Lrinvoiceitems.indexOf(v), 1);
            });
            cleaninvoice.isdirectpurchase = true;
            $scope.SaveRequest = true;
            cleaninvoice.entrylocationid = cleaninvoice.retaillocationid;

            request = {
                "Item": cleaninvoice,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/SaveInvoiceItems",
                data: srequest
            }).success(function (result) {
                $scope.SetDefualtConfig();
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    //var invoiceids = result.id;

                    if (movenext) {
                        $location.url('/barcode?invoiceid=' + result.id);
                        $rootScope.Toaster.Info("InvoiceItems Saved Successfully.!");
                    }
                    else {

                        $rootScope.Toaster.Info("InvoiceItems Saved Successfully.!");
                        $scope.DisplayInvoice(result);
                        $scope.InvoiceItem = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
                    }
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }

        $scope.DisplayInvoice = function (invoice) {
            $scope.Navigation.SetMode('EDIT');
            $scope.Invoice = invoice;
            if ($scope.Invoice.Lrinvoiceitems.length > 0) {
                $scope.ItemDetails.sno = $scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].sno + 1;
                if ($scope.Invoice.multipacksno.length > 0) {
                    $.each($scope.Invoice.multipacksno, function (k, v) {
                        var list = [];
                        var qty = 0;
                        var amt = 0;
                        $.each($scope.Invoice.Lrinvoiceitems, function (ik, iv) {
                            if (iv.sno == v) {
                                list.push(iv);
                                qty += iv.buyingqty;
                                amt += iv.basebuyingprice * iv.buyingqty
                                iv.IsChild = true;
                                iv.IsParent = false;
                            }
                        });

                        var firstIndex = $scope.Invoice.Lrinvoiceitems.indexOf(list[0]);
                        var parent = (JSON.parse(JSON.stringify(list[0])));
                        parent.IsParent = true;
                        parent.Details.sizeid = -2;
                        parent.IsChild = false;
                        parent.buyingqty = qty;
                        parent.Child = list;
                        parent.buyingamount = amt.round(2);
                        parent.amount = amt.round(2);
                        if (firstIndex < $scope.Invoice.Lrinvoiceitems.length) {
                            $scope.Invoice.Lrinvoiceitems.splice(firstIndex + 1, 0, parent);
                        }
                        else {
                            $scope.Invoice.Lrinvoiceitems.push(parent);
                        }

                    });


                    $scope.Invoice.Lrinvoiceitems.sort(function (first, next) {
                        return first.sno == next.sno;
                    });
                }

                $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                    if (typeof (v.buyingamount) == 'undefined') {
                        v.buyingamount = (v.basebuyingprice * v.buyingqty).round(2);
                    }
                });
            }

            $scope.CalculateTotal();
            $scope.ComputeTaxTotal();
            $scope.additionaldiscountpercentage = 0;
            $.each($scope.Invoice.Lrinvoicetaxitems, function (k, v) {

                v.display = v.percentage;
                if (v.type != 0) {
                    v.style = { 'background-color': 'lightgray' };

                    var etype = $scope.EntryTypes.find(function (item) {
                        return item.id == v.type;
                    });

                    if (etype && etype != null)
                        v.display = etype.name;
                }
                else
                    v.style = {};
                if (v.type > 0) {
                    $scope.additionaldiscountpercentage += v.discountpercentage;
                    //v.percentage = 'Rounding';
                }
            });

            if ($scope.egrid == null) {
                $timeout(function () {
                    $scope.ShowEditableGrid();
                    $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                }, 10);
            }
            else {
                $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
            }
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {

                if (typeof (item.IsParent) == 'undefined' || !item.IsParent) {
                    $scope.Total.mrp -= (item.mrp * item.buyingqty);
                    $scope.Total.buyingprice -= (item.buyingprice * item.buyingqty);
                    $scope.Total.sellingprice -= (item.sellingprice * item.buyingqty);
                    $scope.Total.buyingqty -= item.buyingqty;
                    $scope.Total.tax -= item.purchasetax;
                    $scope.Total.purchasediscount -= item.purchasetotaldiscount;
                    $scope.Total.amount -= item.amount;
                }

                $scope.Invoice.Lrinvoiceitems.splice($scope.Invoice.Lrinvoiceitems.indexOf(item), 1);

                if (typeof (item.IsParent) != 'undefined' && item.IsParent && item.Child.length > 0) {
                    $.each(item.Child, function (k, v) {

                        if ($scope.Invoice.ptype == 3) {
                            var exist = $scope.Invoice.Lrinvoicetaxitems.find(function (itm) {
                                return itm.taxid == v.taxid &&
                                    itm.percentage == v.percentage &&
                                    itm.type == 1;
                            })
                            if (exist && exist != null) {
                                exist.amounton -= v.amount;
                                exist.discount -= v.purchasetotaldiscount;
                                exist.taxamount -= v.purchasetax;
                                exist.netamount -= (v.amount + v.purchasetax);
                                $scope.ComputeTaxTotal();
                            }
                        }

                        $scope.Total.mrp -= (v.mrp * v.buyingqty);
                        $scope.Total.buyingprice -= (v.buyingprice * v.buyingqty);
                        $scope.Total.sellingprice -= (v.sellingprice * v.buyingqty);
                        $scope.Total.buyingqty -= v.buyingqty;
                        $scope.Total.tax -= v.purchasetax;
                        $scope.Total.purchasediscount -= v.purchasetotaldiscount;
                        $scope.Total.amount -= v.amount;
                        $scope.Invoice.Lrinvoiceitems.splice($scope.Invoice.Lrinvoiceitems.indexOf(v), 1);
                    })
                }
                else {
                    if ($scope.Invoice.ptype == 3) {
                        var exist = $scope.Invoice.Lrinvoicetaxitems.find(function (itm) {
                            return itm.taxid == item.taxid &&
                                itm.percentage == item.purchasetaxpercentage &&
                                itm.type == 1;
                        })
                        if (exist && exist != null) {
                            exist.amounton -= item.amount;
                            exist.discount -= item.purchasetotaldiscount;
                            exist.taxamount -= item.purchasetax;
                            exist.netamount -= (item.amount + item.purchasetax);
                            $scope.ComputeTaxTotal();
                        }
                    }
                }

                if ($scope.Invoice.Lrinvoiceitems.length == 0) {
                    $scope.ItemDetails.sno = 0;
                    $scope.Total = {
                        "buyingqty": 0,
                        "mrp": 0,
                        "sellingprice": 0,
                        "buyingprice": 0,
                        "tax": 0,
                        "amount": 0
                    };
                }
                else {
                    //if (isNaN($scope.Total.discount)) $scope.Total.discount = 0;
                    $scope.ItemDetails.sno = $scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].sno;
                    $scope.Total.tax = $scope.Total.tax.round(2);
                    $scope.Total.amount = $scope.Total.amount.round(2);
                    $scope.Total.purchasediscount = $scope.Total.purchasediscount.round(2);
                    $scope.Total.mrp = $scope.Total.mrp.round(2);
                    $scope.Total.buyingprice = $scope.Total.buyingprice.round(2);
                    $scope.Total.sellingprice = $scope.Total.sellingprice.round(2);
                    $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);
                }
                $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
            }
            else if (action == "View") {
                $scope.CreateMultiItems(item, true, true);
            }
            else if (action == "Edit") {
                if (typeof ($scope.ItemDetails.IsEditItem) != 'undefined' &&
                    $scope.ItemDetails.IsEditItem) {
                    $rootScope.Confirm('Editing', 'you will loose the current editing item', function () {
                        $scope.$apply(function () {
                            $scope.EditItem(item);
                            $scope.ItemDetails.IsEditItem = true;
                            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                        });
                    });
                }
                else {
                    $scope.EditItem(item);
                    $scope.ItemDetails.IsEditItem = true;
                }

            } else if (action == "EditInvoice") {
                $scope.InvoiceInfo.invoiceid = item.id;
                $scope.InitializeInvoiceList(true);

            } else if (action == "DeleteInvoice") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete the selected purchase entry?',
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
                                    url: "LrinvoiceService/Delete",
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
        }

        $scope.EditItem = function (item) {
            var productitem = $scope.ProductList.find(function (prd) {
                return prd.id == item.productid;
            });

            if (typeof (item.IsParent) == 'undefined' || !item.IsParent) {
                $scope.Total.mrp -= (item.mrp * item.buyingqty);
                $scope.Total.buyingprice -= (item.buyingprice * item.buyingqty);
                $scope.Total.sellingprice -= (item.sellingprice * item.buyingqty);
                $scope.Total.buyingqty -= item.buyingqty;
                $scope.Total.buyingpiece -= (productitem.sellingmode != 3 ? item.buyingqty : 1);
                $scope.Total.tax -= item.purchasetax;
                $scope.Total.purchasediscount -= item.purchasetotaldiscount;
                $scope.Total.amount -= item.amount;
            }

            $scope.Invoice.Lrinvoiceitems.splice($scope.Invoice.Lrinvoiceitems.indexOf(item), 1);


            if (typeof (item.IsParent) != 'undefined' && item.IsParent && item.Child.length > 0) {
                $.each(item.Child, function (k, v) {

                    $scope.Total.mrp -= (v.mrp * v.buyingqty);
                    $scope.Total.buyingprice -= (v.buyingprice * v.buyingqty);
                    $scope.Total.sellingprice -= (v.sellingprice * v.buyingqty);
                    $scope.Total.buyingqty -= v.buyingqty;
                    $scope.Total.buyingpiece -= (productitem.sellingmode != 3 ? v.buyingqty : 1);
                    $scope.Total.tax -= v.purchasetax;
                    $scope.Total.purchasediscount -= v.purchasetotaldiscount;
                    $scope.Total.amount -= v.amount;
                    $scope.Invoice.Lrinvoiceitems.splice($scope.Invoice.Lrinvoiceitems.indexOf(v), 1);

                    if ($scope.Invoice.ptype == 3) {
                        var exist = $scope.Invoice.Lrinvoicetaxitems.find(function (itm) {
                            return itm.taxid == v.taxid &&
                                itm.percentage == v.purchasetaxpercentage &&
                                itm.type == 1;
                        })
                        if (exist && exist != null) {
                            exist.amounton -= v.amount;
                            exist.discount -= v.purchasetotaldiscount;
                            exist.taxamount -= v.purchasetax;
                            exist.netamount -= (v.amount + v.purchasetax);
                            $scope.ComputeTaxTotal();
                        }
                    }
                });
            }
            else {
                if ($scope.Invoice.ptype == 3) {
                    var exist = $scope.Invoice.Lrinvoicetaxitems.find(function (itm) {
                        return itm.taxid == item.taxid &&
                            itm.percentage == item.purchasetaxpercentage &&
                            itm.type == 1;
                    })
                    if (exist && exist != null) {
                        exist.amounton -= item.amount;
                        exist.discount -= item.purchasetotaldiscount;
                        exist.taxamount -= item.purchasetax;
                        exist.netamount -= (item.amount + item.purchasetax);
                        $scope.ComputeTaxTotal();
                    }
                }
            }

            $scope.Total.purchasediscount = $scope.Total.purchasediscount.round(2);
            $scope.Total.tax = $scope.Total.tax.round(2);
            $scope.Total.amount = $scope.Total.amount.round(2);
            $scope.Total.mrp = $scope.Total.mrp.round(2);
            $scope.Total.buyingprice = $scope.Total.buyingprice.round(2);
            $scope.Total.sellingprice = $scope.Total.sellingprice.round(2);
            $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);
            $scope.ProductSearch.ProductID = item.productid;
            $scope.ItemDetails = item;
            $scope.LastEntryNo = 'Entry No :' + item.invoiceentryno;
            $scope.taxChange();
        }

        $scope.EditArrayItem = function (item) {
            var dialog = ngDialog.open({
                template: 'ChooseTaxType',
                data: [$scope, item],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.type = null;
                    $scope.EntryTypes = [
                        { 'id': 0, 'name': 'Amount', 'discount': true, 'amount': true, 'tax': true, 'directamount': false },
                        { 'id': 4, 'name': 'Insurance', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
                        { 'id': 5, 'name': 'Service', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
                        { 'id': 6, 'name': 'Courier', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
                        { 'id': 7, 'name': 'Packing', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
                        { 'id': 8, 'name': 'Fright', 'discount': false, 'amount': true, 'tax': true, 'directamount': false },
                        { 'id': 9, 'name': 'Rounding', 'discount': true, 'amount': true, 'tax': true, 'directamount': false },
                        { 'id': 10, 'name': 'Job Work', 'discount': false, 'amount': true, 'tax': true, 'directamount': false }];
                    $timeout(function () {
                        $scope.type = $scope.ngDialogData[1].type;
                    }, 10);

                    $scope.Selected = function () {
                        $scope.closeThisDialog($scope.type);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        item.type = data.value;
                        item.display = item.percentage;
                        item.style = {};

                        if (item.type != 0) {
                            item.style = { 'background-color': 'lightgray' };
                            var etype = $scope.EntryTypes.find(function (v) {
                                return v.id == data.value;
                            });

                            if (etype && etype != null)
                                item.display = etype.name;
                        }
                    }
                }
            });
        }

        $scope.DeleteArrayItem = function (source, list) {
            list.splice(list.indexOf(source), 1);
            $scope.ComputeTaxTotal();
        }

        $scope.mrpChange = function () {
            var value = $scope.ItemDetails.mrp;
            $scope.ItemDetails.sellingprice = value;
            if ($scope.ItemDetails.buyingprice == null || isNaN($scope.ItemDetails.buyingprice))
                $scope.ItemDetails.buyingprice = $scope.GetNumber($scope.ItemDetails.basebuyingprice);

            $scope.ItemDetails.marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / $scope.GetNumber($scope.ItemDetails.basebuyingprice) * 100) - 100).round(2);

            var taxitem = $scope.CurrentTaxConfig;

            if (taxitem == null) {
                $scope.ItemDetails.salestaxpercentage = 0;
            }
            else {
                var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }
            $scope.salediscountChanged();
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                if (typeof ($scope.SearchCondition.lrid) == 'undefined' || $scope.SearchCondition.lrid == 0) {
                    $scope.SearchResult = result;
                }
                else {
                    $scope.SearchResult = result.Items;
                    $scope.SearchCondition.lrid = 0;
                }

                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '500px', '100%', false, true, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.EnableSerial($scope.SearchResult);

                igrid.AddReferenceColumn('suppliercompanyid', 'Supplier', 150, $scope.SupplierCompany);
                igrid.AddFormatedColumn('entrydate', 'Entry Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('invoiceentryno', 'Entry No', 100);
                igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
                igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('noofbundle', 'Bundles', 80, 'N0', 2).total = true;
                igrid.AddFormatedColumn('baseamount', 'Gross', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('taxamount', 'Tax', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('totalamount', 'Total', 80, 'N2', 2).total = true;

                if ($rootScope.UserInfo.p_DIRECTPURCHASE.modify || $rootScope.UserInfo.p_DIRECTPURCHASE.delete) {
                    var commands = "";
                    if ($rootScope.UserInfo.p_DIRECTPURCHASE.modify)
                        commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
                    if ($rootScope.UserInfo.p_DIRECTPURCHASE.delete)
                        commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"

                    igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                        $scope.$apply(function () {
                            if ($(e.target).val() == 'Edit')
                                $scope.ManageSearchItem('EditInvoice', args.item);
                            else if ($(e.target).val() == 'Delete')
                                $scope.ManageSearchItem('DeleteInvoice', args.item);
                        });
                    });
                }

                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData($scope.SearchResult);
            }).error(function (result) {
            });
        }

        $scope.producttypeChange = function () {
            $scope.ShowEditableGrid();
        }

        $scope.egrid = null;
        $scope.ShowEditableGrid = function () {
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('editablegrid', '500px', '100%', false, true, true, 50);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);
            var column = igrid.AddCustomColumn('productname', 'Product', 120, function (item, col) {
                return item.Details.productname;
            });

            if ($scope.Invoice.ptype == 0) {
                var column = igrid.AddCustomColumn('brandid', 'Brand', 120, function (item, col) {
                    return $scope.SearchRef(item.Details.brandid, $scope.BrandName);
                });
            }
            else {
                var column = igrid.AddCustomColumn('serialno', 'UAN/GLN', 100, function (item, col) {
                    return item.Details.serialno;
                });
            }

            var column = igrid.AddCustomColumn('sizeid', 'Size', 80, function (item, col) {
                return $scope.SearchRef(item.Details.sizeid, $scope.SizeList);
            });
            igrid.AddSimpleColumn('designid', 'Design', 100);

            var column = igrid.AddColumn({
                id: "buyingqty",
                name: "Qty",
                field: "buyingqty",
                width: 60,
                cssClass: "cell-title",
                editor: Slick.Editors.Text,
                'sortable': true
            });
            column.total = true;

            if ($scope.Invoice.ptype < 0) {
                var column = igrid.AddColumn({
                    id: "freeqty",
                    name: "Free",
                    field: "freeqty",
                    width: 60,
                    cssClass: "cell-title",
                    editor: Slick.Editors.Text,
                    'sortable': true
                });
                column.total = true;
            }

            igrid.AddFormatedColumn('basebuyingprice', 'Cost', 80, 'N0', 2).total = true;
            var column = igrid.AddColumn({
                id: "purchasediscount",
                name: "Discount",
                field: "purchasediscount",
                width: 60,
                cssClass: "cell-title",
                editor: Slick.Editors.Text,
                'sortable': true
            });
            igrid.AddFormatedColumn('marginpercentage', 'Margin%', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('sellingprice', 'Price', 80, 'N0', 2).total = true;
            var column = igrid.AddCustomColumn('amount', 'Amount', 120, function (item, col) {
                return $filter('IndianNumber')(item.amount + item.purchasetotaldiscount);
            });
            column.getValue = function (item, col) {
                return item.amount + item.purchasetotaldiscount;
            };
            column.total = true;
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-info btn-xs'><i class='fa fa-pencil' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('View', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='View' class='btn btn-info btn-xs'><i class='fa fa-plus-circle' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.options.autoEdit = true;
            igrid.options.showHeaderRow = true;
            igrid.filter = function (item, args) {
                return !(typeof (item.IsChild) != 'undefined' && item.IsChild);
            }
            igrid.EnableTotal();
            igrid.Initialize();
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $timeout(function () {
                    if (command == 'Delete' || command == 'Edit' || command == 'View') {
                        $scope.ManageSearchItem(command, item);
                        $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                    }
                }, 10);
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'designid') {
                    $scope.itemchange('designid', item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                } else if (args.grid.getColumns()[args.cell].id == 'hsncode') {
                    $scope.itemchange('hsncode', item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                } else if (args.grid.getColumns()[args.cell].id == 'UAN') {
                    item.Details.serialno = item[args.grid.getColumns()[args.cell].id];
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                } else if (args.grid.getColumns()[args.cell].id == 'purchasediscount' ||
                    args.grid.getColumns()[args.cell].id == 'buyingqty') {
                    $scope.RowValuechanged(item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }

            });
            $scope.egrid = igrid;
            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
        }

        $scope.ShowProductSearch = function (list) {
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
                if (v.typename != null && v.typename.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.typename;
                if (v.materialname != null && v.materialname.length > 0)
                    v.display += (v.display.length > 0 ? ',' : '') + v.materialname;
            });

            var dialog = ngDialog.open({
                template: 'ProductSearch',
                data: [$scope, list],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.PopupList = $scope.ngDialogData[1];
                    $scope.Selected = function (item) {
                        $scope.closeThisDialog(item);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    $scope.ItemDetails.Details = data.value;


                    $scope.ItemDetails.Details.productid = $scope.ProductSearch.ProductID;

                    $scope.productChange();
                    $scope.findLastPrice();
                }
            });
        }

        $scope.findLastPrice = function () {

            var request = {
                "Item": $scope.ItemDetails.Details,
                "securitytoken": "String content"
            }

            request.item.supplierid = $scope.Invoice.supplierid;
            request.item.productid = $scope.ItemDetails.productid;
            request.item.fetchprices = true;
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PricecontrolService/Search",
                data: srequest
            }).success(function (result) {
                $scope.Price = null;
                $scope.LastPrice = null;
                if (result.items != null && result.items.length > 0) {
                    $scope.Price = result.items[0];
                }

                if (result.lastprice != null) {
                    $scope.LastPrice = result.lastprice;
                }

                $scope.FitSellingPrice();

            }).error(function (result) {
            });
        };

        $scope.purchasediscountpercentagechanged = function () {
            $scope.ItemDetails.purchasediscount = ($scope.ItemDetails.purchasediscountpercentage * $scope.GetNumber($scope.ItemDetails.basebuyingprice) / 100).round(2);
            $scope.fixTax(false);
        };

        $scope.purchasediscountchanged = function () {
            $scope.ItemDetails.purchasediscountpercentage = ($scope.ItemDetails.purchasediscount / $scope.GetNumber($scope.ItemDetails.basebuyingprice) * 100).round(2);
            $scope.fixTax(false);
        };

        $scope.SearchDesign = function () {
            var request = {
                "Item": {
                    'supplierid': $scope.Invoice.supplierid,
                    'productid': $scope.ProductSearch.ProductID,
                    'designid': $scope.ItemDetails.designid
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/SearchByDesignName",
                data: srequest
            }).success(function (result) {
                if (result != null) {
                    $scope.ItemDetails.Details = result.Details;
                    $scope.ProductSearch.ProductID = $scope.ItemDetails.Details.productid;
                    $scope.findLastPrice();
                    $scope.productChange();
                }
            });
        };

        $scope.FitSellingPrice = function () {
            var lastSellingPrice = 0;
            var lastDiscount = 0;
            var cnfgSellingPrice = 0;
            var cnfgDiscount = 0;
            var cnfgName = ''
            if ($scope.LastPrice != null) {
                $scope.ItemDetails.designid = $scope.LastPrice.designid;
                $scope.ItemDetails.lastsellingprice = $scope.LastPrice.sellingprice;
                $scope.ItemDetails.lastbuyingprice = $scope.LastPrice.buyingprice;
                $scope.ItemDetails.lastdiscount = $scope.LastPrice.discount;
                lastSellingPrice = $scope.LastPrice.sellingprice;
                lastDiscount = $scope.LastPrice.discount;
            }

            if ($scope.Price != null) {

                if ($scope.Price.pricingmode == 1) {
                    cnfgSellingPrice = $scope.ItemDetails.mrp;
                    cnfgName = 'MRP';
                }

                if ($scope.Price.pricingmode == 2) {
                    cnfgName = 'Margin @' + $scope.Price.pricingvalue + '%';
                    cnfgSellingPrice = $scope.ItemDetails.buyingprice + ($scope.ItemDetails.buyingprice * $scope.Price.pricingvalue / 100).round(2);
                }

                if ($scope.Price.pricingmode == 3) {
                    cnfgName = 'Margin @' + $scope.Price.pricingvalue;
                    cnfgSellingPrice = $scope.ItemDetails.buyingprice + $scope.Price.pricingvalue;
                }

                if ($scope.Price.pricingmode == 4) {
                    cnfgName = 'Fixed Price @' + $scope.Price.pricingvalue;
                    cnfgSellingPrice = $scope.Price.pricingvalue;
                }

                if ($scope.Price.discountmode == 2)
                    cnfgDiscount = $scope.Price.discountvalue;
                else if ($scope.Price.discountmode == 2)
                    cnfgDiscount = (cnfgSellingPrice * $scope.Price.discountvalue / 100).round(2);
            }

            if (lastSellingPrice > 0 && cnfgSellingPrice == 0) {
                $scope.ItemDetails.sellingprice = lastSellingPrice;
            } else if (lastSellingPrice == 0 && cnfgSellingPrice > 0) {
                $scope.ItemDetails.sellingprice = cnfgSellingPrice;
            }
            else if (lastSellingPrice == cnfgSellingPrice) {
                $scope.ItemDetails.sellingprice = lastSellingPrice;
            }
            else if (lastSellingPrice != cnfgSellingPrice) {
                $scope.ShowPricingOption(
                    {
                        'Mechanism': cnfgName,
                        'Value': cnfgSellingPrice,
                        'Discount': cnfgDiscount,
                        'LastPrice': lastSellingPrice,
                        'LastDiscount': lastDiscount
                    });
            }
        };

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.Invoice', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            //$scope.Invoice = value;
            $scope.DisplayInvoice(value);
        };





        $scope.ShowPricingOption = function (priceConfig) {
            var dialog = ngDialog.open({
                template: 'PriceSearch',
                data: [priceConfig],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Price = $scope.ngDialogData[0];
                    $scope.Selected = function (value, discount) {
                        $scope.closeThisDialog([value, discount]);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    $scope.ItemDetails.sellingprice = data.value[0];
                    $scope.ItemDetails.discount = data.value[1];
                }
            });
        };

        $scope.CreateMultiItems = function (ItemDetails, isJump, isView) {
            var dialog = ngDialog.open({
                template: 'MultiSize',
                data: {
                    'ItemDetails': ItemDetails, 'SearchRef': $scope.SearchRef, 'AutoTotal': $scope.Invoice.ptype == 3,
                    'SizeList': $scope.SizeList, 'isJump': isJump, 'isView': isView, 'scope': $scope
                },
                width: '680px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.IsJump = $scope.ngDialogData.isJump;
                    $scope.AutoTotal = $scope.ngDialogData.AutoTotal;
                    $scope.sizeInfo = '';
                    $scope.isView = $scope.ngDialogData.isView;
                    $scope.Created = 0;
                    $scope.AllowChangePrice = false;
                    $scope.CoreItem = $scope.ngDialogData.ItemDetails;
                    $scope.TotalAmount = 0;
                    if ($scope.CoreItem.buyingqty == undefined ||
                        $scope.CoreItem.buyingqty == null)
                        $scope.CoreItem.buyingqty = 0;
                    $scope.Total = $scope.CoreItem.buyingqty;

                    $scope.PopupList = [];
                    $scope.CanClear = true;
                    $timeout(function () {
                        if ($scope.IsJump) {
                            $scope['setFocussizeInfo']();
                        }
                        else {
                            $scope['setFocussizeInfoBit']();
                        }
                    }, 100);

                    //if (!$scope.IsJump)
                    //    $scope.sizeInfo = $scope.CoreItem.buyingqty;

                    if (typeof ($scope.CoreItem.Child) != 'undefined' &&
                        $scope.CoreItem.Child.length > 0) {
                        //if (!$scope.IsJump) {

                        //    $.each($scope.CoreItem.Child, function (k, v) {
                        //        var nitem = {
                        //            'sizename': 'Piece', 'sizeid': v.Details.sizeid,
                        //            'qty': v.buyingqty
                        //        }
                        //        $scope.PopupList.push(nitem);
                        //        $scope.Created += v.buyingqty;
                        //        $scope.sizeInfo = ($scope.CoreItem.buyingqty - $scope.Created).toString();
                        //    });
                        //}
                        //else {

                        $.each($scope.CoreItem.Child, function (k, v) {
                            v.buyingqty = parseFloat(v.buyingqty);
                            v.qty = typeof (v.qty) == 'undefined' ? v.buyingqty : parseFloat(v.qty);
                            v.buyingprice = parseFloat(v.buyingprice);

                            if (isNaN(v.qty)) v.qty = 0.0;
                            if (isNaN(v.buyingqty)) v.buyingqty = 0.0;
                            if (isNaN(v.buyingprice)) v.buyingprice = 0.0;

                            v.sizeid = v.Details.sizeid;
                            v.sizename = restService.SearchReference(v.sizeid, $scope.ngDialogData.SizeList);

                            //var nitem = {
                            //    'sizename': restService.SearchReference($scope.CoreItem.Details.sizeid, $scope.ngDialogData.SizeList), 'sizeid': v.Details.sizeid,
                            //    'qty': v.buyingqty, 'sellingprice': v.sellingprice,
                            //    'mrp': v.mrp,
                            //    'buyingprice': v.buyingprice,
                            //    'marginpercentage': v.marginpercentage,
                            //    'basebuyingprice': v.basebuyingprice,
                            //    'salediscount': v.salediscount,
                            //    'salediscountpercentage': v.salediscountpercentage,
                            //    'purchasediscountpercentage': v.purchasediscountpercentage
                            //}

                            if (typeof (v.id) != 'undefined' && v.id > 0)
                                $scope.CanClear = false;

                            $scope.TotalAmount += (v.qty * v.buyingprice).round(2);

                            $scope.PopupList.push((JSON.parse(JSON.stringify(v))));
                            $scope.Created += v.buyingqty;
                            $scope.Created = $scope.Created.round(2);
                        });
                        if ($scope.AutoTotal) {
                            $scope.Total = $scope.Created;
                        }
                    }



                    $scope.CreateItems = function ($event) {
                        if (typeof ($scope.sizeInfo) == 'undefined' || $scope.sizeInfo == null || $scope.sizeInfo.toString() == '') {
                            if (!$scope.Selected()) {
                                if ($scope.PopupList.length > 0) {
                                    $scope.setFocusmcell_firstrow();
                                }
                                else {
                                    if ($scope.IsJump) {
                                        $scope['setFocussizeInfo']();
                                    }
                                    else {
                                        $scope['setFocussizeInfoBit']();
                                    }
                                }
                                $event.preventDefault();
                            }
                            return true;
                        }

                        var cellid = 0;
                        if (!$scope.IsJump) {
                            var nqty = $scope.sizeInfo.toString().split('-');

                            var tqty = (nqty.length == 2 ? parseFloat(nqty[0]) : 1);
                            var meter = (nqty.length == 2 ? parseFloat(nqty[1]) : parseFloat(nqty[0]));

                            if (!$scope.IsJump) {
                                var nqty = $scope.sizeInfo.toString().split('-');

                                for (var index = 0; index < tqty; index++) {

                                    var nitem = {
                                        'sizename': 'Piece', 'sizeid': $scope.CoreItem.sizeid,
                                        'qty': meter, 'sellingprice': $scope.GetNumber($scope.CoreItem.sellingprice),
                                        'mrp': $scope.GetNumber($scope.CoreItem.mrp),
                                        'buyingprice': $scope.GetNumber($scope.CoreItem.buyingprice),
                                        'marginpercentage': $scope.GetNumber($scope.CoreItem.marginpercentage),
                                        'basebuyingprice': $scope.GetNumber($scope.CoreItem.basebuyingprice),
                                        'salediscount': $scope.GetNumber($scope.CoreItem.salediscount),
                                        'salediscountpercentage': $scope.GetNumber($scope.CoreItem.salediscountpercentage),
                                        'purchasediscountpercentage': $scope.GetNumber($scope.CoreItem.purchasediscountpercentage)
                                    }
                                    $scope.TotalAmount += (nitem.qty * nitem.buyingprice).round(2);

                                    if ($scope.PopupList.length == 0) {
                                        nitem.cellid = 'mcell_firstrow';
                                    }
                                    $scope.PopupList.push(nitem);
                                    nitem.cellid = 'mcell_' + (cellid++);
                                    $scope.Created += meter;
                                }
                                $scope.Created = $scope.Created.round(2);
                                $scope.sizeInfo = null;//($scope.CoreItem.buyingqty - $scope.Created).toString();
                            }
                            $scope.sizeInfo = null;//($scope.CoreItem.buyingqty - $scope.Created).toString();
                        }
                        else {

                            var sizeidList = $scope.sizeInfo.toString().split(',');
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
                            $.each($scope.ngDialogData.SizeList, function (k, v) {
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

                            if (idList.length > 0) {

                                idList.sort(function (a, b) {
                                    return a.index - b.index;
                                });

                                var qty = Math.floor(($scope.CoreItem.buyingqty - $scope.Created) / idList.length);
                                if (!isNaN(qty)) {
                                    $.each(idList, function (k, v) {
                                        var nitem = {
                                            'sizename': v.name, 'sizeid': v.id,
                                            'qty': qty, 'sellingprice': $scope.GetNumber($scope.CoreItem.sellingprice),
                                            'mrp': $scope.GetNumber($scope.CoreItem.mrp),
                                            'buyingprice': $scope.GetNumber($scope.CoreItem.buyingprice),
                                            'marginpercentage': $scope.GetNumber($scope.CoreItem.marginpercentage),
                                            'basebuyingprice': $scope.GetNumber($scope.CoreItem.basebuyingprice),
                                            'salediscount': $scope.GetNumber($scope.CoreItem.salediscount),
                                            'salediscountpercentage': $scope.GetNumber($scope.CoreItem.salediscountpercentage),
                                            'purchasediscountpercentage': $scope.GetNumber($scope.CoreItem.purchasediscountpercentage)
                                        }
                                        $scope.TotalAmount += (nitem.qty * nitem.buyingprice).round(2);
                                        if ($scope.PopupList.length == 0) {
                                            nitem.cellid = 'mcell_firstrow';
                                        }
                                        $scope.PopupList.push(nitem);
                                        $scope.Created += qty;
                                    });
                                    $scope.Created = $scope.Created.round(2);
                                    if ($scope.Created < qty)
                                        $scope.PopupList[$scope.PopupList.length - 1].qty = qty - $scope.Created;
                                }
                            }
                            $scope.sizeInfo = null;
                        }
                        if ($scope.AutoTotal) {
                            $scope.Total = $scope.Created;
                        }
                    };

                    $scope.GetNumber = function (value) {
                        if (typeof (value) == 'undefined' || value == null || isNaN(value))
                            return 0;
                        var nvalue = parseFloat(value);
                        if (isNaN(nvalue)) return 0;
                        return nvalue;
                    };

                    $scope.PriceChanged = function (item, newbasebuyingprice, oldbasebuyingprice) {
                        var oldvalue = $scope.GetNumber(oldbasebuyingprice);
                        var newvalue = $scope.GetNumber(newbasebuyingprice);

                        if (isNaN(oldvalue)) {
                            oldvalue = 0;
                        }

                        if (isNaN(newvalue)) {
                            newvalue = 0;
                        }

                        if (newvalue != oldvalue) {
                            $scope.TotalAmount -= (oldvalue * item.qty).round(2)
                            $scope.TotalAmount += (newvalue * item.qty).round(2)
                            $scope.TotalAmount = $scope.TotalAmount.round(2);
                        }

                        item.manualprice = true;
                        item.purchasediscount = ($scope.GetNumber(item.basebuyingprice) * item.purchasediscountpercentage / 100).round(2);
                        if (typeof (item.purchasediscount) == 'undefined' || item.purchasediscount == null || isNaN(item.purchasediscount))
                            item.purchasediscount = 0;

                        item.buyingprice = item.basebuyingprice - item.purchasediscount;
                        item.sellingprice = item.buyingprice + (item.buyingprice * $scope.GetNumber(item.marginpercentage) / 100);
                        item.sellingprice = $scope.ngDialogData.scope.RoundPrice(item.sellingprice);
                        item.salediscount = Math.round(item.sellingprice * item.salediscountpercentage / 100, 0);
                        item.mrp = item.sellingprice;
                    }

                    $scope.MRPChanged = function (item) {
                        item.manualprice = true;
                        var value = $scope.GetNumber(item.mrp);
                        item.sellingprice = value;
                        if (typeof (item.buyingprice) == 'undefined' || item.buyingprice == null || isNaN(item.buyingprice))
                            item.buyingprice = item.basebuyingprice;
                        item.marginpercentage = ((item.sellingprice / item.buyingprice * 100).round(2) - 100).round(2);
                        item.salediscount = Math.round(item.sellingprice * item.salediscountpercentage / 100, 0);
                    }

                    $scope.QtyChanged = function (item, newVal, oldVal) {

                        var oldvalue = $scope.GetNumber(oldVal);
                        var newvalue = $scope.GetNumber(newVal);

                        if (isNaN(oldvalue)) {
                            oldvalue = 0;
                        }
                        if (isNaN(newvalue)) {
                            newvalue = 0;
                        }
                        if (typeof (item.sellableqty) != 'undefined' && item.sellableqty > 0 && item.sellableqty > newvalue) {
                            item.qty = item.sellableqty;
                            return;
                        }

                        $scope.TotalAmount -= (oldvalue * item.buyingprice).round(2)
                        $scope.TotalAmount += (newvalue * item.buyingprice).round(2)
                        $scope.Created -= oldvalue;
                        $scope.Created += newvalue;
                        $scope.Created = $scope.Created.round(2);
                        if ($scope.AutoTotal) {
                            $scope.Total = $scope.Created;
                        }
                        $scope.PriceChanged(item);
                    };

                    $scope.Validate = function () {
                        $.each($scope.PopupList, function (k, item) {
                            if (item.purchasediscount == 0 && item.purchasediscountpercentage > 0) {
                                item.purchasediscount = ($scope.GetNumber(item.basebuyingprice) * item.purchasediscountpercentage / 100).round(2);
                                if (typeof (item.purchasediscount) == 'undefined' || item.purchasediscount == null || isNaN(item.purchasediscount))
                                    item.purchasediscount = 0;

                                item.buyingprice = item.basebuyingprice - item.purchasediscount;
                            }
                        })
                    };

                    $scope.Delete = function (item) {
                        if (typeof (item.sellableqty) != 'undefined' && item.sellableqty > 0) {
                            return;
                        }
                        $scope.PopupList.splice($scope.PopupList.indexOf(item), 1);
                        $scope.TotalAmount -= (item.qty * item.buyingprice).round(2)
                        $scope.Created -= item.qty;
                        $scope.Created = $scope.Created.round(2);
                        if ($scope.AutoTotal) {
                            $scope.Total = $scope.Created;
                        }
                    };

                    $scope.Clear = function () {
                        $scope.PopupList = [];
                        $scope.Created = 0;
                        $scope.TotalAmount = 0;
                        if ($scope.AutoTotal) {
                            $scope.Total = $scope.Created;
                        }
                    }
                    $scope.Selected = function () {
                        if ($scope.Created != $scope.Total || $scope.Created == 'undefined' || $scope.Created == null) {
                            $rootScope.Toaster.Error("Check Qty..!");
                            return ($scope.Created == $scope.Total);
                        }
                        else {
                            if (!$scope.IsJump) {
                                $.each($scope.PopupList, function (k, v) {
                                    $scope.isbit = $scope.isbit;
                                });
                            }
                            $scope.closeThisDialog($scope.PopupList);
                            return true;
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var cloneDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                        cloneDetails.id = 0;
                        $scope.ItemDetails = (JSON.parse(JSON.stringify(cloneDetails)));
                        $scope.ItemDetails.IsParent = true;
                        $scope.ItemDetails.IsChild = false;

                        $scope.AddItemInternal(false);
                        var parent = $scope.ItemDetails;
                        parent.Child = [];
                        parent.buyingamount = 0;
                        var taxitem = $scope.CurrentTaxConfig;

                        $.each(data.value, function (k, v) {
                            try {
                                $scope.ItemDetails = (JSON.parse(JSON.stringify(cloneDetails)));
                                $scope.ItemDetails.Details.sizeid = v.sizeid;
                                $scope.ItemDetails.Details.isbit = v.isbit;
                                $scope.ItemDetails.buyingqty = $scope.GetNumber(v.qty);
                                $scope.ItemDetails.purchasediscountpercentage = $scope.GetNumber(v.purchasediscountpercentage);
                                $scope.ItemDetails.purchasediscount = $scope.GetNumber(v.purchasediscount);
                                $scope.ItemDetails.basebuyingprice = $scope.GetNumber(v.basebuyingprice);
                                $scope.ItemDetails.buyingprice = $scope.GetNumber(v.buyingprice);
                                $scope.ItemDetails.sellingprice = $scope.GetNumber(v.sellingprice);
                                $scope.ItemDetails.salediscount = $scope.GetNumber(v.salediscount);
                                $scope.ItemDetails.mrp = $scope.GetNumber(v.mrp);
                                $scope.ItemDetails.barcodes = v.barcodes;
                                $scope.ItemDetails.sellableqty = v.sellableqty;
                                $scope.ItemDetails.damagedqty = v.damagedqty;
                                $scope.ItemDetails.actualqty = v.actualqty;
                                $scope.ItemDetails.dealerprice = v.dealerprice;
                                $scope.ItemDetails.wsrate = v.wsrate;

                                $scope.ItemDetails.id = v.id;
                                if (taxitem == null) {
                                    $scope.ItemDetails.purchasetaxpercentage = 0;
                                    $scope.ItemDetails.salestaxpercentage = 0;
                                }
                                else {
                                    var taxconfig = taxitem.taxsplit.findTax($scope.ItemDetails.buyingprice);
                                    $scope.ItemDetails.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
                                    taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                                    $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
                                }

                                $scope.ItemDetails.amount = $scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty);
                                $scope.ItemDetails.purchasetotaldiscount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty) * $scope.ItemDetails.purchasediscountpercentage / 100).round(2);
                                $scope.ItemDetails.amount -= $scope.ItemDetails.purchasetotaldiscount;
                                $scope.ItemDetails.purchasetax = ($scope.ItemDetails.amount * $scope.ItemDetails.purchasetaxpercentage / 100).round(2);
                                $scope.ItemDetails.purchasediscount = ($scope.ItemDetails.purchasetotaldiscount / $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);

                                $scope.ItemDetails.salediscountpercentage = $scope.GetNumber(v.salediscountpercentage);
                                $scope.ItemDetails.salestax = ($scope.GetNumber($scope.ItemDetails.buyingqty) * ($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount) / ($scope.ItemDetails.salestaxpercentage + 100) * $scope.ItemDetails.salestaxpercentage).round(2);
                                $scope.ItemDetails.IsChild = true;
                                $scope.ItemDetails.IsParent = false;
                                parent.buyingamount += $scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty);
                                parent.Child.push($scope.ItemDetails);
                                $scope.AddItemInternal(false);
                            } catch (e) {
                                e = null;
                            }
                        });
                        parent.buyingamount = parent.buyingamount.round(2);
                        parent.amount = parent.buyingamount.round(2);
                        $scope.CalculateTotal();
                        $scope.ItemDetails = cloneDetails;
                        $scope.CreateNew();
                    }
                }
            });
        };

        if (typeof ($location.search().id) != 'undefined' &&
            $location.search().id.length > 0) {
            request = {
                "Item": { 'lrid': $location.search().id },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ShowSearch = true;
                if (result.length == 1) {
                    $scope.InvoiceInfo.invoiceid = result[0].id;
                    $scope.InitializeInvoiceList(true);
                }
                else {
                    $scope.SearchResult = result;
                }
            }).error(function (result) {
            });
        }

        if (typeof ($location.search().lrid) != 'undefined') {
            $scope.SearchCondition = {
                companyid: 0,
                supplierid: 0,
                invoiceno: "",
                addedfrom: null,
                addedto: null,
                lrid: 0
            }

            $scope.SearchCondition.lrid = $location.search().lrid;
            $scope.Search();
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


        $scope.LastEntryNo = null;
        $scope.ShowLastEntryNo = function () {
            if ($scope.Invoice && $scope.Invoice.id && $scope.Invoice.id > 0) return;
            var companyid = (typeof ($scope.Invoice.companyid) && $scope.Invoice.companyid > 0) ? $scope.Invoice.companyid : 0;

            restService.Get('LrinvoiceService/LastEntry/' + companyid, true).then(function (results) {
                if (results == '')
                    $scope.LastEntryNo = '';
                else
                    $scope.LastEntryNo = 'Last Entry No:' + results;
            });
        }

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
            $scope.SupportRounding = $rootScope.UserInfo.appsettings.system_supportrounding || false;
            $scope.PurchaseRounding = $rootScope.UserInfo.appsettings.system_purchaserounding || '';
            $scope.SupportMarginOnCost = $rootScope.UserInfo.appsettings.system_marginoncost || false;
            $scope.SupportMarkDOWN = $rootScope.UserInfo.appsettings.system_marginonmarkup || $rootScope.UserInfo.appsettings.system_marginonmarkdown || false;
            $scope.SupportAdditionalOnCost = $rootScope.UserInfo.appsettings.system_costwithadditional || false;
            $scope.SupportMarginexcludetax = $rootScope.UserInfo.appsettings.system_marginexcludetax || false;
            $scope.SupportDealerPrice = $rootScope.UserInfo.appsettings.system_dealerprice || false;
            $scope.SupportDealerPriceWOTax = $rootScope.UserInfo.appsettings.system_dealerpricewotax || false;
            $scope.SupportHyperApplication = $rootScope.UserInfo.appsettings.system_ishypermarketapplication || false;
        }

        $timeout(function () {
            $scope.LoadItems();
        }, 25);

        $scope.SupportHyperApplication = false;
        $scope.SupportRounding = false;
        $scope.PurchaseRounding = '';
        $scope.SupportMarginOnCost = false;
        $scope.SupportMarkDOWN = false;
        $scope.SupportMarginexcludetax = false;
        $scope.SupportDealerPrice = false;
        $scope.SupportDealerPriceWOTax = false;
    }]);
