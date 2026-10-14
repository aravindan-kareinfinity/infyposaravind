var app = angular.module('InfinityLab');

app.controller('LRInvoiceItemsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', '$filter', 'cntxService', '$compile',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, $filter, cntxService, $compile) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.SetMode('SEARCH');
        $scope.SearchRefDirect = function (id, list) {
            return $scope.SearchRef(id, list);
        }

        $scope.EntryTypes = [{
            'id': -1,
            'name': 'None',
            'discount': true,
            'amount': true,
            'tax': false,
            'directamount': false
        },
        {
            'id': 0,
            'name': 'Bill Value',
            'discount': true,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 1,
            'name': 'Tax',
            'discount': false,
            'amount': false,
            'tax': true,
            'directamount': false
        },
        {
            'id': 2,
            'name': 'Discount',
            'discount': true,
            'amount': false,
            'tax': false,
            'directamount': false
        },
        {
            'id': 3,
            'name': 'OnDiscount',
            'discount': true,
            'amount': false,
            'tax': false,
            'directamount': false
        },
        {
            'id': 4,
            'name': 'Insurance',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 5,
            'name': 'Service',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 6,
            'name': 'Courier',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 7,
            'name': 'Packing',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 8,
            'name': 'Fright',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 9,
            'name': 'Rounding',
            'discount': true,
            'amount': true,
            'tax': true,
            'directamount': false
        },
        {
            'id': 10,
            'name': 'Job Work',
            'discount': false,
            'amount': true,
            'tax': true,
            'directamount': false
        }
        ];

        $scope.AllProductList = [];
        $scope.LocalContext = cntxService.CreatePageContext('lrinvoiceitems');
        $scope.ProductSearch = {
            "ProductID": null,
            "SearchItemName": "",
            "currentInvoiceID": "",
            "CurrentSKUList": []
        };
        $scope.InvoiceInfo = {
            invoiceid: $location.search().invoiceid
        };
        $scope.AutoCalculation = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = false;
        $scope.showtax = false;
        $scope.isHideIncrese = true;
        $scope.SupportRounding = true;
        $scope.system_supportpricingflow = false;
        $scope.SupportDesignList = false;
        $scope.system_supportpricingflowproducts = false;
        $scope.PurchaseRoundingConfig = null;
        $scope.SupportDiscountOnCost = false;
        $scope.SupportMarginOnCost = false;
        $scope.SupportAdditionalOnCost = false;
        $scope.SupportLocationBased = false;
        $scope.SupportUANPurchase = false;
        $scope.System_b2brounding = 0;
        $scope.SupportMarkDOWN = false;
        $scope.CollectLocationInfo = false;
        $scope.SupportMarginexcludetax = false;
        $scope.SupportMarginincludetax = false;
        $scope.SupportSalepriceMrpdifference = false;
        $scope.SupportAttributeFilter = false;
        $scope.SupportFinalPriceChange = true;
        $scope.SupportPOMandatory = false;
        $scope.SupportRoundingDiscount = false;
        $scope.SupportAddlrexptomargion = false;
        $scope.SupportDealerPrice = false;
        $scope.SupportDealerPriceWOTax = false;
        $scope.SupportImageRecording = false;
        $scope.HideSupplier = false;
        $scope.system_invcol_colour = false;
        $scope.system_invcol_size = false;
        $scope.system_invcol_brand = false;
        $scope.system_invcol_style = false;
        $scope.system_invcol_type = false;
        $scope.system_invcol_expmargin = false;
        $scope.system_invcol_dppercent = false;
        $scope.system_invcol_txpercent = false;
        $scope.system_hidecostinventory = false;
        $scope.system_invcol_pricetag = false;
        $scope.system_invcol_pricetype = false;
        $scope.system_marginpercentagezero = false;
        $scope.system_designidautoid = false;
        $scope.SupportItemwiseEntry = false;
        $scope.ItemBasedCostFixed = false;

        $scope.PriceTagColumn = null;
        $scope.SearchResult = [];
        $scope.PurchaseOrderItems = [];
        $scope.ErrorMessage = '';
        $scope.additionaldiscountpercentage = 0;
        $scope.Total = {
            "buyingqty": 0,
            "mrp": 0,
            "purchasediscount": 0,
            "sellingprice": 0,
            "buyingprice": 0,
            "tax": 0,
            "buyingpiece": 0,
            "amount": 0,
            "value": 0
        };

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        };

        $scope.ShowTax = function () {
            $scope.showtax = !$scope.showtax;
        };

        $scope.LastSelectedItem = null;
        $scope.ProductItemChange = function () {
            var selecteditem = $scope.ItemList.find(function (item) {
                return item.id == $scope.ProductSearch.Itemid;
            });

            if (typeof (selecteditem) != 'undefined' && selecteditem != null) {
                $scope.LastSelectedItem = selecteditem;
                $scope.ItemDetails.Details.brandid = selecteditem.brandid;
                $scope.ItemDetails.Details.colourid = selecteditem.colourid;
                $scope.ItemDetails.Details.sizeid = selecteditem.sizeid;
                $scope.ItemDetails.Details.sizename = selecteditem.sizename;
                $scope.ItemDetails.Details.patternid = selecteditem.patternid;
                $scope.ItemDetails.Details.styleid = selecteditem.styleid;
                $scope.ItemDetails.Details.fitid = selecteditem.fitid;
                $scope.ItemDetails.Details.sleeveid = selecteditem.sleeveid;
                $scope.ItemDetails.Details.materialid = selecteditem.materialid;
                $scope.ItemDetails.Details.typeid = selecteditem.typeid;
                $scope.ItemDetails.Details.printingname = selecteditem.printingname;
                $scope.ItemDetails.Details.sellingname = selecteditem.sellingname;

                if (selecteditem.support_gln) {
                    $scope.ItemDetails.Details.serialno = selecteditem.gln;
                    $scope.ItemDetails.Details.uan = selecteditem.gln;
                }
                $scope.ItemDetails.designid = selecteditem.glndesignid;
                $scope.ItemDetails.Details.dealerprice = $rootScope.GetNumber(selecteditem.dealerprice);
                $scope.ItemDetails.dealerprice = $rootScope.GetNumber(selecteditem.dealerprice);
                $scope.ItemDetails.Details.minrate = selecteditem.minrate;

                $scope.ItemDetails.basebuyingprice = $rootScope.GetNumber(selecteditem.purchaserate);
                $scope.ItemDetails.buyingprice = $rootScope.GetNumber(selecteditem.purchaserate);
                $scope.ItemDetails.sellingprice = $rootScope.GetNumber(selecteditem.salerate);
                $scope.ItemDetails.mrp = $rootScope.GetNumber(selecteditem.mrp);
                $scope.ItemDetails.rowexpense = $rootScope.GetNumber(selecteditem.rowexpense);

                $scope.ItemDetails.salediscountpercentage = $rootScope.GetNumber(selecteditem.salediscountpercentage);
                $scope.ItemDetails.salediscount = $rootScope.GetNumber(selecteditem.salediscount);
                $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;

                var marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / $scope.GetNumber($scope.ItemDetails.buyingprice) * 100) - 100).round(2);
                $scope.ItemDetails.marginpercentage = marginpercentage;

                $scope.ItemBasedCostFixed = $scope.GetNumber($scope.ItemDetails.sellingprice) > 0 ? true : false;
                if ($scope.Mandatory && $scope.Mandatory.attributes &&
                    $scope.Mandatory.attributes.colouroption) {
                    $scope['setFocuscolouroptionid']();
                } else {
                    $scope['setFocussizeid']();
                }
            }
        };

        $scope.FocusRqty = function () {
            if ($scope.Invoice.isvendorbill)
                $scope.setFocusreturnqty();
            else
                $scope.FocusFinalPrice();
        }


        $scope.PrintVendorInvoice = function () {

            $rootScope.Download('LrinvoiceService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.Invoice.id, 'jobworkorder.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "vendorinvoice.pdf";
                link.href = 'LrinvoiceService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.Invoice.id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.FocusFinalPrice = function () {
            if ($scope.SupportFinalPriceChange) {
                $scope.TryAddItem();
            } else
                $scope.setFocusfinalprice();
        }

        $scope.SearchCondition = {
            companyid: 0,
            supplierid: 0,
            invoiceno: "",
            addedfrom: null,
            addedto: null,
            lrentryno: ""
        }
        $scope.PurchaseTax = restService.Get('ReferencelistService/ALLTAX', true).then(function (results) {
            $scope.PurchaseTax = results;
        });
        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            var pl = [];
            $.each(results, function (k, v) {
                pl.push(v);
            });
            $scope.AllProductList = results;
            $scope.ProductList = pl;
            if (typeof ($scope.Invoice.Lrinvoiceitems) != 'undefined' && $scope.Invoice.Lrinvoiceitems.length > 0) {
                $scope.CalculateTotal();
            }
        });
        $scope.productbrandmap = false;
        $scope.BrandNameList = [];
        $scope.ItemNameList = [];
        $scope.ItemList = [];
        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
            $scope.BrandNameList = results;
        });
        $scope.PatternList = restService.Get('ReferencelistService/LIST-PATTERN', true).then(function (results) {
            $scope.PatternList = results;
            $scope.ALLPatternList = results;
        });

        $scope.SleeveList = restService.Get('ReferencelistService/LIST-SLEEVE', true).then(function (results) {
            $scope.SleeveList = results;
            $scope.AllSleeveList = results;
        });

        $scope.FitList = restService.Get('ReferencelistService/LIST-FIT', true).then(function (results) {
            $scope.FitList = results;
            $scope.AllFitList = results;
        });
        $scope.SizeGroupList = restService.Get('ReferencelistService/SIZEGROUP', true).then(function (results) {
            $scope.SizeGroupList = results;
        });


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
            if ($rootScope.UserInfo.appsettings.system_invcol_pricetag) {
                $scope.PriceTagList = null;
                restService.Get('ReferencelistService/PRICETAGS', true).then(function (results) {
                    $scope.PriceTagList = [];
                    $.each(results, function (k, v) {
                        if (!v.additionalinfo.billdiscountinclude)
                            $scope.PriceTagList.push(v);
                    });
                    if ($scope.PriceTagColumn != null)
                        $scope.PriceTagColumn.dataSource = $scope.PriceTagList;
                });
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
                };

            } else {
                $scope.FilterItemList = function (productid) { };
            }
        }
        $timeout(function () {
            $scope.LoadItems();
        }, 25);

        $scope.StyleList = restService.Get('ReferencelistService/LIST-STYLE', true).then(function (results) {
            $scope.StyleList = results;
            $scope.ALLStyleList = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/LIST-SIZE', true).then(function (results) {
            $scope.SizeList = results;
            var exist = false;
            $.each($scope.SizeList, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.SizeList.push({
                    id: -2,
                    name: 'Jump'
                });
                $scope.SizeList.push({
                    id: -3,
                    name: 'Cut'
                });
                $scope.SizeList.push({
                    id: -4,
                    name: 'Size Group'
                });
            }
            $scope.ALLSizeList = results;
        });
        $scope.ColorList = restService.Get('ReferencelistService/LIST-COLOUR', true).then(function (results) {
            $scope.ColorList = results;
            $scope.ALLColorList = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });

        $scope.MaterialList = restService.Get('ReferencelistService/LIST-MATERIAL', true).then(function (results) {
            $scope.MaterialList = results;
            $scope.ALLMaterialList = results;
        });
        $scope.TypeList = restService.Get('ReferencelistService/LIST-TYPE', true).then(function (results) {
            $scope.TypeList = results;
            $scope.ALLTypeList = results;
        });
        $scope.RetialLocations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.RetialLocations = results;
        });
        $scope.SearchItem = function () {
            var request = {
                "Item": {
                    'name': $scope.ProductSearch.SearchItemName,
                    'productid': $scope.ProductSearch.ProductID
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
                } else if (result.length > 1) {
                    $scope.ShowProductSearch(result);
                }
            });
        };

        $scope.StatusList = [{
            id: 1,
            name: 'Progress'
        }, {
            id: 2,
            name: 'Completed'
        }, {
            id: 3,
            name: 'Temporary'
        }, {
            id: 4,
            name: 'Merge Entries'
        }];

        $scope.ItemTemplate = {
            "buyingqty": null,
            "mrp": null,
            "rowexpense": null,
            "sellingprice": null,
            "buyingprice": null,
            "tax": 0,
            "staxpercentage": 0,
            "ptaxpercentage": 0,
            "taxid": null,
            "pricingmode": 0,
            "calculatedPrice": 0,
            "readonly": false,
            "hidepricing": false,
            "isactive": true,
            "designid": "",
            "sno": 1,
            "polist": "",
            "poinfo": [],
            Details: {
                "productid": null,
                "printingname": null,
                "brandid": null,
                "patternid": null,
                "styleid": null,
                "typeid": null,
                "sizeid": null,
                "sleeveid": null,
                "fitid": null,
                "colourid": null,
                "materialid": null
            }
        }

        $scope.InvoiceTemplate = {
            "companyid": 0,
            "invoiceno": null,
            "invoicedate": "",
            "invoiceDate": "",
            "noofpieces": 0,
            "noofbundle": 0,
            "baseamount": 0.0,
            "itemid": 0,
            "grossamount": 0.0,
            "totalamount": 0.0,
            "lrid": 0,
            "id": 0,
            "otheramount": 0.0,
            "modifiedon": "",
            "createdon": "",
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "suppliercompanyid": 0,
            "isactive": true,
            "lrinvoicetaxitems": null,
            uniqueid: 0,
            Lrinvoiceitems: []
        }

        $scope.LastPrice = null;
        $scope.BrandClass = 'col-md-6';
        $scope.ColourClass = 'col-md-6';
        $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
        $scope.Invoice = (JSON.parse(JSON.stringify($scope.InvoiceTemplate)));
        $scope.Mandatory = {
            "attributefilter": false,
            "producttype": 0,
            "pattern": {},
            "brand": {},
            "material": {},
            "designid": {},
            "purchaseorder": {},
            "type": {},
            "colour": {},
            "size": {},
            "attributes": {
                'serialno': false,
                'batchandexpiry': false,
                'item': false,
                'colouroption': false,
                'quality': false,
                'model': false,
                'warrenty': false,
                'multipleprice': false,
                "sno": true,
                "hsncode": true,
                'size': true,
                'colour': true,
            },
            "style": {
                "brand": 'col-md-6',
                "colour": 'col-md-6',
                "size": 'col-md-6',
                "pattern": 'col-md-6',
                "style": 'col-md-6',
                "type": 'col-md-6',
                "material": 'col-md-6',
                "fit": 'col-md-6',
                "sleeve": 'col-md-6'
            }
        };

        $scope.MarginDPClass = 'col-md-12 no-padding';
        $scope.MandatoryStyle = {
            'color': 'orangered'
        };
        $scope.CurrentProduct = null;

        $scope.FilterArray = function (sourcelist, filterlist, addlist) {
            if (filterlist == null || filterlist.length == 0) return sourcelist;

            var newlist = [];
            $.each(filterlist, function (k, v) {
                var item = sourcelist.find(function (item) {
                    return item.id == v;
                });
                if ((item && item.id > 0))
                    newlist.push(item);
            });
            if (addlist && addlist.length > 0) {
                $.each(addlist, function (k, v) {
                    var item = sourcelist.find(function (item) {
                        return item.id == v;
                    });
                    if (item && (item.id > 0 || item.id == -2 || item.id == -3 || item.id == -4))
                        newlist.push(item);
                });
            }

            return newlist;
        }

        $scope.ProductAttributeFilter = function (attributelist) {
            $scope.StyleList = $scope.FilterArray($scope.ALLStyleList, attributelist == null ? null : attributelist.style);
            $scope.PatternList = $scope.FilterArray($scope.ALLPatternList, attributelist == null ? null : attributelist.pattern);
            $scope.MaterialList = $scope.FilterArray($scope.ALLMaterialList, attributelist == null ? null : attributelist.material);
            $scope.SizeList = $scope.FilterArray($scope.ALLSizeList, attributelist == null ? null : attributelist.size, [-2, -3, -4]);
            $scope.TypeList = $scope.FilterArray($scope.ALLTypeList, attributelist == null ? null : attributelist.type);
            $scope.ColorList = $scope.FilterArray($scope.ALLColorList, attributelist == null ? null : attributelist.colour);
            $scope.BrandName = $scope.FilterArray($scope.BrandNameList, attributelist == null ? null : attributelist.brand);
            $scope.FitList = $scope.FilterArray($scope.AllFitList, attributelist == null ? null : attributelist.fit);
            $scope.SleeveList = $scope.FilterArray($scope.AllSleeveList, attributelist == null ? null : attributelist.sleeve);
        }

        $scope.productChange = function () {
            $scope.SearchPO();
            $scope.SearchDesignList();
            if ($scope.ProductSearch.ProductID != null) {
                $scope.CurrentProduct = null;
                var prditem = $scope.AllProductList.find(function (item) {
                    return (item.id == $scope.ProductSearch.ProductID);
                });
                if (prditem || prditem != null) {
                    $scope.CurrentProduct = prditem;
                    $scope.ItemDetails.itemid = 0;
                    $scope.ItemDetails.productid = prditem.id;
                    $scope.ItemDetails.Details.productid = prditem.id;
                    $scope.ItemDetails.ptaxid = prditem.purchasetaxid;
                    $scope.ItemDetails.taxid = prditem.salestaxid;
                    if (!$scope.system_marginpercentagezero || $scope.ItemDetails.id == undefined || $scope.ItemDetails.id == null ||
                        $scope.ItemDetails.id == '' || $scope.ItemDetails.id == '0') {
                        $scope.ItemDetails.hsncode = prditem.hsncode;
                    }
                    $scope.ItemDetails.pricingmode = prditem.pricingmode;
                    $scope.ItemDetails.margin = prditem.margin;
                    $scope.ItemDetails.Details.sizeid = prditem.sellingmode == 3 ? -3 : -2;
                    if (prditem.attributes != null) {
                        $scope.Mandatory = {
                            "attributefilter": prditem.attributefilter,
                            "producttype": prditem.producttype,
                            "style": prditem.attributes.mandatorystyle || prditem.attributes.mandatorymodel ? $scope.MandatoryStyle : {},
                            "pattern": prditem.attributes.mandatorypattern || prditem.attributes.mandatoryquality ? $scope.MandatoryStyle : {},
                            "brand": prditem.attributes.mandatorybrand ? $scope.MandatoryStyle : {},
                            "material": prditem.attributes.mandatorymaterial || prditem.attributes.mandatorywarrenty ? $scope.MandatoryStyle : {},
                            "designid": prditem.attributes.mandatorydesignid ? $scope.MandatoryStyle : {},
                            "purchaseorder": prditem.attributes.mandatorypurchaseorder ? $scope.MandatoryStyle : {},
                            "type": prditem.attributes.mandatorytype ? $scope.MandatoryStyle : {},
                            "fit": prditem.attributes.mandatoryfit ? $scope.MandatoryStyle : {},
                            "sleeve": prditem.attributes.mandatorysleeve ? $scope.MandatoryStyle : {},
                            "colour": prditem.attributes.mandatorycolour ? $scope.MandatoryStyle : {},
                            "size": prditem.attributes.mandatorysize ? $scope.MandatoryStyle : {},
                            "item": prditem.attributes.mandatoryitem ? $scope.MandatoryStyle : {},
                            "serialno": prditem.attributes.mandatoryserialno ? $scope.MandatoryStyle : {},
                            "batchandexpiry": prditem.attributes.mandatorybatchandexpiry ? $scope.MandatoryStyle : {},
                            "model": prditem.attributes.mandatorymodel ? $scope.MandatoryStyle : {},
                            "quality": prditem.attributes.mandatoryquality ? $scope.MandatoryStyle : {},
                            "warrenty": prditem.attributes.mandatorywarrenty ? $scope.MandatoryStyle : {},
                            "multipleprice": prditem.attributes.mandatorymultipleprice ? $scope.MandatoryStyle : {},
                            "simtype": prditem.attributes.mandatorysimtype ? $scope.MandatoryStyle : {},
                            "weight": prditem.attributes.mandatoryweight ? $scope.MandatoryStyle : {},
                            "wastage": prditem.attributes.mandatorywastage ? $scope.MandatoryStyle : {},
                            "workingcharge": prditem.attributes.mandatoryworkingcharge ? $scope.MandatoryStyle : {},
                            "attributes": prditem.attributes
                        };

                        if ($scope.system_customfields &&
                            prditem.attributes.customfields) {
                            $.each($scope.ItemTemplate.CustomFields, function (k, v) {
                                var prdconfig = prditem.attributes.customfields.find(function (item) {
                                    return item.key == v.key;
                                });
                                if (prdconfig && prdconfig != null) {
                                    $scope.Mandatory.attributes[v.key] = prdconfig.show;
                                    $scope.Mandatory.style[v.key] = prdconfig.mandatory ? $scope.MandatoryStyle : {};
                                }
                                else {
                                    $scope.Mandatory.attributes[v.key] = false;
                                    $scope.Mandatory.style[v.key] = {};
                                }
                            });
                        }

                        if (prditem.attributefilter && !prditem.attributes.mandatorysize) {
                            $scope.ItemDetails.Details.sizeid = 0;
                        }

                        if (typeof ($scope.Mandatory.attributes) != 'undefined' &&
                            $scope.Mandatory.attributefilter) {
                            $scope.Mandatory.attributes.sno = true;
                            $scope.Mandatory.attributes.hsncode = true;

                            $scope.Mandatory.style = {
                                "brand": 'col-md-6',
                                "colour": 'col-md-6',
                                "size": 'col-md-6',
                                "pattern": 'col-md-6',
                                "style": 'col-md-6',
                                "type": 'col-md-6',
                                "material": 'col-md-6',
                                "fit": 'col-md-6',
                                "sleeve": 'col-md-6'
                            };

                            if ($scope.Mandatory.attributes.colouroption) {
                                if ($scope.Mandatory.attributes.brand != $scope.Mandatory.attributes.size) {
                                    if ($scope.Mandatory.attributes.brand)
                                        $scope.Mandatory.style.brand = 'col-md-12';
                                    else
                                        $scope.Mandatory.style.size = 'col-md-12';
                                }
                            } else if ($scope.Mandatory.attributes.brand != $scope.Mandatory.attributes.colour) {
                                if ($scope.Mandatory.attributes.brand)
                                    $scope.Mandatory.style.brand = 'col-md-12';
                                else
                                    $scope.Mandatory.style.colour = 'col-md-12';
                            }

                            if (($scope.Mandatory.attributes.pattern || $scope.Mandatory.attributes.quality) != ($scope.Mandatory.attributes.style || $scope.Mandatory.attributes.model)) {
                                if ($scope.Mandatory.attributes.pattern || $scope.Mandatory.attributes.quality)
                                    $scope.Mandatory.style.pattern = 'col-md-12';
                                else
                                    $scope.Mandatory.style.style = 'col-md-12';
                            }

                            if (($scope.Mandatory.attributes.type || $scope.Mandatory.attributes.simtype) != ($scope.Mandatory.attributes.material || $scope.Mandatory.attributes.warrenty)) {
                                if ($scope.Mandatory.attributes.type || $scope.Mandatory.attributes.simtype)
                                    $scope.Mandatory.style.type = 'col-md-12';
                                else
                                    $scope.Mandatory.style.material = 'col-md-12';
                            }

                            if (($scope.Mandatory.attributes.fit) != ($scope.Mandatory.attributes.sleeve)) {
                                if ($scope.Mandatory.attributes.fit)
                                    $scope.Mandatory.style.fit = 'col-md-12';
                                else
                                    $scope.Mandatory.style.sleeve = 'col-md-12';
                            }

                        } else {
                            if (typeof ($scope.Mandatory.attributes) != 'undefined')
                                $scope.Mandatory.attributes = {};

                            $scope.Mandatory.attributes.sno = true;
                            $scope.Mandatory.attributes.hsncode = true;
                            $scope.Mandatory.style = {
                                "brand": 'col-md-6',
                                "colour": 'col-md-6',
                                "size": 'col-md-6',
                                "pattern": 'col-md-6',
                                "style": 'col-md-6',
                                "type": 'col-md-6',
                                "material": 'col-md-6',
                                "fit": 'col-md-6',
                                "sleeve": 'col-md-6'
                            };
                        }

                        if (prditem.attributes.item) {
                            $scope.FilterItemList(prditem.id);
                        }
                    } else {
                        $scope.Mandatory = {
                            "pattern": {},
                            "brand": {},
                            "material": {},
                            "warrenty": {},
                            "designid": {},
                            "purchaseorder": {},
                            "type": {},
                            "colour": {},
                            "size": {},
                            "multipleprice": {},
                            "attributefilter": false,
                            "producttype": 0,
                            "item": {},
                            "serialno": {},
                            "batchandexpiry": {},
                            "model": {},
                            "quality": {},
                            "simtype": {},
                            "attributes": {
                                'type': true,
                                'pattern': true,
                                'simtype': false,
                                "sno": true,
                                "hsncode": true,
                                'serialno': false,
                                'batchandexpiry': false,
                                'item': false,
                                'colouroption': false,
                                'quality': false,
                                'model': false,
                                'warrenty': false,
                                'multipleprice': false
                            },
                            "style": {
                                "brand": 'col-md-6',
                                "colour": 'col-md-6',
                                "size": 'col-md-6',
                                "pattern": 'col-md-6',
                                "style": 'col-md-6',
                                "type": 'col-md-6',
                                "material": 'col-md-6',
                                "fit": 'col-md-6',
                                "sleeve": 'col-md-6'
                            }
                        };
                    }

                    if ($scope.SupportAttributeFilter)
                        $scope.ProductAttributeFilter(prditem.attributelist);

                    //if (prditem.brands != null && prditem.brands.length > 0) {
                    //    $scope.productbrandmap = true;
                    //    $scope.BrandName = [];
                    //    $.each(prditem.brands, function (k, v) {
                    //        $scope.BrandName.push({ id: v.id, name: v.name });
                    //    });
                    //}
                    //else if ($scope.productbrandmap) {
                    //    $scope.BrandName = $scope.BrandNameList;
                    //    $scope.productbrandmap = false;
                    //}

                    $scope.taxChange();
                }
            }
            $scope.fixPrice();
        };




        $scope.SearchExistingPO = function () {

            var request = {
                "Item": {
                    'supplierid': $scope.Invoice.supplierid,
                    'lrinvoiceid': $scope.Invoice.id,
                    'expectedate': $scope.Invoice.entrydate
                },
                "SecurityToken": "String content"
            }
            $http({
                method: 'POST',
                url: "PurchaseorderService/SearchPO",
                data: request
            }).success(function (result) {

                $.each(result, function (k, v) {
                    if (typeof ($scope.PurchaseOrderItems[v.productid]) == 'undefined')
                        $scope.PurchaseOrderItems[v.productid] = [];
                    $scope.PurchaseOrderItems[v.productid].push(v);
                });
            }).error(function (result) {

            });
        };

        $scope.SearchPO = function (callback) {

            if (typeof ($scope.PurchaseOrderItems[$scope.ProductSearch.ProductID]) != 'undefined' &&
                typeof ($scope.PurchaseOrderItems[$scope.ProductSearch.ProductID].length) != 'undefined') {
                if (typeof (callback) != 'undefined')
                    callback($scope.PurchaseOrderItems[$scope.ProductSearch.ProductID]);
                else
                    $scope.UpdatePOList($scope.PurchaseOrderItems[$scope.ProductSearch.ProductID], $scope.ProductSearch.ProductID);
                return;
            }

            var request = {
                "Item": {
                    'supplierid': $scope.Invoice.supplierid,
                    'productid': $scope.ProductSearch.ProductID,
                    'lrinvoiceid': $scope.Invoice.id,
                    'expectedate': $scope.Invoice.entrydate
                },
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "PurchaseorderService/SearchPO",
                data: request
            }).success(function (result) {
                if (typeof (callback) == 'undefined') {
                    $scope.PurchaseOrderItems[$scope.ProductSearch.ProductID] = result;
                    $scope.UpdatePOList();
                } else {
                    callback(result);
                }
            }).error(function (result) {

            });
        };

        $scope.POMandatoryValidation = function () {
            $scope.UpdatePOList();

            if ($scope.SupportPOMandatory) {
                var bqty = $scope.GetNumber($scope.ItemDetails.buyingqty);
            }
        }

        $scope.UpdatePOList = function () {
            if (typeof ($scope.PurchaseOrderItems[$scope.ProductSearch.ProductID]) == 'undefined')
                return;
            var orderitems = $scope.PurchaseOrderItems[$scope.ProductSearch.ProductID];
            var productid = $scope.ProductSearch.ProductID;
            var price = 0;
            var accepted = 0;
            var designid = null;
            var hasdesignid = typeof ($scope.ItemDetails.designid) != 'undefined' && $scope.ItemDetails.designid != null && $scope.ItemDetails.designid.length > 0;

            if ($scope.GetNumber($scope.ItemDetails.buyingqty) > 0) {
                $scope.ItemDetails.poinfo = [];
                var result = [];
                var availble = $scope.GetNumber($scope.ItemDetails.buyingqty);
                $.each(orderitems, function (k, v) {
                    if (availble > 0 && (!hasdesignid || v.designid == $scope.ItemDetails.designid)) {
                        if (availble > v.balance) {
                            result.push(v.pono + ':' + v.balance);
                            availble -= v.balance;
                            $scope.ItemDetails.poinfo.push({
                                'purchaseorderitemsid': v.purchaseorderitemsid,
                                'designid': v.designid,
                                'sizeid': v.sizeid,
                                'pono': v.pono,
                                'aqty': v.balance,
                                'oqty': v.balance
                            });
                            accepted += v.balance;
                        } else {
                            result.push(v.pono + ':' + availble);
                            $scope.ItemDetails.poinfo.push({
                                'purchaseorderitemsid': v.purchaseorderitemsid,
                                'designid': v.designid,
                                'sizeid': v.sizeid,
                                'pono': v.pono,
                                'aqty': availble,
                                'oqty': v.balance
                            });
                            accepted += availble;
                            availble = 0;
                        }

                        if (v.price != 0 && price == 0)
                            price = v.price;
                        else if (price != -1 && v.price != 0 && price != v.price)
                            price = -1;
                    }
                });
                $scope.ItemDetails.polist = result.join('/');
            } else {
                var oqty = 0;
                $.each(orderitems, function (k, v) {
                    if (!hasdesignid || v.designid == $scope.ItemDetails.designid) {
                        oqty += v.balance;
                        if (v.price != 0 && price == 0) {
                            price = v.price;
                        } else if (price != -1 && v.price != 0 && price != v.price) {
                            price = -1;
                        }
                    }
                });
                $scope.ItemDetails.polist = '' + oqty + '/' + orderitems.length;
                $scope.ItemDetails.poinfo = [];
                $scope.ItemDetails.postatus = 'Multiple Order';
            }
            if (price > 0)
                $scope.ItemDetails.basebuyingprice = price;
            if (!hasdesignid && designid != '~')
                $scope.ItemDetails.designid = designid;
            else if (!hasdesignid && orderitems.length == 1)
                $scope.ItemDetails.designid = orderitems[0].designid;

            $scope.fixTax(false);
        }

        $scope.AllocatePOItems = function (ItemDetails, edit, callback) {
            if (typeof (ItemDetails) == 'undefined' || ItemDetails == null)
                ItemDetails = $scope.ItemDetails;

            if (typeof (edit) == 'undefined')
                edit = false;

            if (!edit)
                ItemDetails.productid = $scope.ProductSearch.ProductID;

            var orderitems = $scope.PurchaseOrderItems[ItemDetails.productid];
            if (typeof (orderitems) == 'undefined') {
                $scope.Toaster.Error("There is no Purchase order matching for the Invoice Item.");
                if (typeof (callback) != 'undefined')
                    callback(false);
                return;
            }
            orderitems = (JSON.parse(JSON.stringify(orderitems)));
            var currentorderqty = 0;

            if (typeof (ItemDetails.Child) != 'undefined') {
                $.each(ItemDetails.Child, function (kk, vv) {
                    if (vv.poinfo != null && vv.poinfo.length > 0) {
                        $.each(vv.poinfo, function (k, v) {
                            var existingitem = orderitems.find(function (item) {
                                return item.purchaseorderitemsid == v.purchaseorderitemsid;
                            });
                            if (typeof (existingitem) != 'undefined' && existingitem != null) {
                                existingitem.qty = v.aqty;
                            }
                            currentorderqty += v.aqty;
                        });
                    }
                });
            }
            else {
                if (ItemDetails.poinfo != null && ItemDetails.poinfo.length > 0) {
                    $.each(ItemDetails.poinfo, function (k, v) {
                        var existingitem = orderitems.find(function (item) {
                            return item.purchaseorderitemsid == v.purchaseorderitemsid;
                        });
                        if (typeof (existingitem) != 'undefined' && existingitem != null) {
                            existingitem.qty = v.aqty;
                        }
                        currentorderqty += v.aqty;
                    });
                }
            }

            $.each(orderitems, function (k, v) {
                v.currentorderqty = 0;
                $.each($scope.Invoice.Lrinvoiceitems, function (kk, vv) {

                    if (Object.is(ItemDetails, vv)) return;
                    if (ItemDetails.IsParent && ItemDetails.Child.indexOf(vv) >= 0) return;

                    if (!vv.IsParent && vv.poinfo && vv.poinfo.length > 0) {
                        for (var poindex = 0; poindex < vv.poinfo.length; poindex++) {
                            if (vv.poinfo[poindex].purchaseorderitemsid == v.purchaseorderitemsid) {
                                v.currentorderqty += vv.poinfo[poindex].aqty;
                            }
                        }
                    }

                });
            });

            var dialog = ngDialog.open({
                template: 'POAllocation',
                data: [$scope, orderitems, ItemDetails.buyingqty, currentorderqty],
                width: '750px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.PopupList = $scope.ngDialogData[1];
                    $scope.buyingqty = parseFloat($scope.ngDialogData[2]);
                    $scope.orderqty = parseFloat($scope.ngDialogData[3]);
                    if (isNaN($scope.buyingqty)) $scope.buyingqty = 0;

                    $scope.QtyChanged = function () {
                        $scope.orderqty = 0;
                        var maxqty = [];
                        $.each($scope.PopupList, function (k, v) {
                            var qty = parseFloat(v.qty);
                            if (isNaN(qty)) qty = 0;
                            $scope.orderqty += qty;
                        })
                    }

                    $scope.Selected = function (item) {
                        $scope.orderqty = 0;
                        var maxqty = [];
                        $.each($scope.PopupList, function (k, v) {
                            var qty = parseFloat(v.qty);
                            if (isNaN(qty)) qty = 0;
                            $scope.orderqty += qty;
                            if (qty > 0 && (v.balance + (v.maxqty - v.order) - v.currentorderqty) < qty) {
                                maxqty.push(v.pono + " " + v.description + " maximum allowed " + (v.maxqty - v.currentorderqty));
                            }
                        })
                        if (maxqty.length > 0) {
                            $scope.Toaster.Error(maxqty.join('<br>'));
                        }
                        else if ($scope.orderqty != $scope.buyingqty) {
                            $scope.Toaster.Error("Please select " + $scope.buyingqty + " qty only!");
                        }
                        else {
                            $scope.closeThisDialog($scope.PopupList);
                        }
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        ItemDetails.poinfo = [];
                        var alist = [];
                        var oqty = 0;
                        var eMsg = [];

                        $.each(data.value, function (k, v) {
                            if ($scope.GetNumber(v.qty) > 0) {
                                ItemDetails.poinfo.push({
                                    'purchaseorderitemsid': v.purchaseorderitemsid,
                                    'designid': v.designid,
                                    'sizeid': v.sizeid,
                                    'pono': v.pono,
                                    'aqty': $scope.GetNumber(v.qty),
                                    'oqty': v.balance
                                });

                                v.excesspo = false;
                                if (v.maxqty > 0 &&
                                    $scope.GetNumber(v.qty) > (v.maxqty + v.currentorderqty)) {
                                    eMsg.push(v.description + " received higher than allowed qty = " + v.maxqty);
                                    v.qty = v.maxqty;
                                    v.excesspo = true;
                                }

                                alist.push(v.pono + ':' + v.qty);

                                if (typeof (ItemDetails.designid) == 'undefined' || ItemDetails.designid == '')
                                    ItemDetails.designid = v.designid;

                                if (!edit && (typeof (ItemDetails.basebuyingprice) == 'undefined' ||
                                    $scope.GetNumber(ItemDetails.basebuyingprice) == 0)) {
                                    ItemDetails.basebuyingprice = v.price;
                                }
                                oqty += $scope.GetNumber(v.qty);
                            }
                        });

                        if ((typeof (ItemDetails.buyingqty) == 'undefined' ||
                            $scope.GetNumber(ItemDetails.buyingqty) != oqty)) {
                            ItemDetails.buyingqty = oqty;
                            $rootScope.Toaster.Info('Order Qty adjusted to ' + oqty);
                        }

                        if (ItemDetails.poinfo.length == 1 && ItemDetails.poinfo[0].designid != null &&
                            ItemDetails.poinfo[0].designid == '') {

                            if (typeof (ItemDetails.designid) == 'undefined' || ItemDetails.designid == '')
                                ItemDetails.designid = ItemDetails.poinfo[0].designid;
                        }

                        if (alist.length > 0) {
                            ItemDetails.polist = alist.join('/');
                            if (!edit) {
                                $scope.fixTax();
                                $timeout(function () {
                                    $scope.setFocusmarginpercentage();
                                }, 10);
                            }
                        }

                        if (eMsg.length > 0) {
                            $rootScope.Toaster.Error('Extra quantity received ' + eMsg.join(','));
                        } else if ($scope.GetNumber(ItemDetails.buyingqty) < oqty) {
                            $rootScope.Toaster.Error('Order Qty higher than buying qty');
                        } else if (oqty > 0) {
                            if (typeof (callback) != 'undefined') {
                                callback(true);
                                return;
                            }
                        }
                    }
                    if (typeof (callback) != 'undefined') {
                        callback(false);
                    }
                }
            });
        }

        $scope.validateSerialAndGo = function () {
            if ($scope.Mandatory.attributes.serialno) {
                $scope.ItemDetails.Details.serialno = '';
                $scope['setFocusserialno']();
            } else {
                $scope['setFocuspatternid']();
            }
        }

        $scope.validateBatchAndGo = function () {
            if ($scope.Mandatory.attributes.batchandexpiry) {
                $scope['setFocusbatch']();
            } else {
                $scope['setFocuspatternid']();
            }
        }

        $scope.FocusConfig = [{
            element: 'brandid',
            permission: 'brand'
        },
        {
            element: 'isizeid',
            permission: 'size',
            apermission: 'colouroption'
        },
        {
            element: 'icolourid',
            permission: 'colour',
            dpermission: 'colouroption'
        },
        {
            element: 'hsncode',
            permission: 'hsncode'
        },
        {
            element: 'batch',
            permission: 'batchandexpiry'
        },
        {
            element: 'patternid',
            permission: 'quality'
        },
        {
            element: 'patternid',
            permission: 'pattern'
        },
        {
            element: 'styleid',
            permission: 'model'
        },
        {
            element: 'styleid',
            permission: 'style'
        },
        {
            element: 'fitid',
            permission: 'fit'
        },
        {
            element: 'sleeveid',
            permission: 'sleeve'
        },
        {
            element: 'typeid',
            permission: 'simtype'
        },
        {
            element: 'typeid',
            permission: 'type'
        },
        {
            element: 'materialid',
            permission: 'warrenty'
        },
        {
            element: 'materialid',
            permission: 'material'
        },
        {
            element: 'serialno',
            permission: 'serialno'
        },
        {
            element: 'sno',
            permission: 'sno'
        },
        {
            element: 'sizeid',
            permission: 'size',
            dpermission: 'colouroption'
        },
        {
            element: 'colouroptionid',
            permission: 'colour',
            apermission: 'colouroption'
        },
        {
            element: 'weight',
            permission: 'weight'
        },
        {
            element: 'wastage',
            permission: 'wastage'
        },
        {
            element: 'workingcharge',
            permission: 'workingcharge'
        }
        ];

        $scope.CheckProductEntryKey = function () {

            $scope.LastSelectedItem = null;
            if ($scope.Mandatory.attributes.item)
                $scope['setFocusitemid']();
            else
                $scope.SetFocusByFlow('brandid');
        }

        $scope.OnCostEnterKey = function () {

            if (!$scope.IsUANBased() && !$scope.ItemBasedCostFixed)
                $scope.findLastPrice();
            if ($scope.Mandatory.producttype == 0) {
                $scope.setFocusmarginpercentage();
            } else if ($scope.Mandatory.producttype == 8) {
                $scope.setFocussellingprice();
            } else {
                $scope.setFocusdealerprice();
            }
        }
        $scope.SetFocusByFlow = function (focusid) {
            if (!$scope.Mandatory.attributefilter) {
                if (focusid == 'batch') {
                    focusid = 'patternid';
                }
                if (focusid == 'serialno') {
                    focusid = 'sno';
                }
                $scope['setFocus' + focusid]();
                return;
            }

            if (focusid == 'sizeid') {
                if ($scope.Mandatory.attributes.colouroption) {
                    $scope['setFocuscolouroptionid']();
                    return;
                } else if ($scope.Mandatory.attributes.size) {
                    $scope['setFocussizeid']();
                    return;
                }
            }

            var done = false;
            var focusitem = $scope.FocusConfig.find(function (item) {
                return item.element == focusid;
            });

            var index = $scope.FocusConfig.indexOf(focusitem);
            for (; done == false && index < $scope.FocusConfig.length; index++) {
                var permission = $scope.FocusConfig[index].permission;
                if ($scope.Mandatory.attributes[permission]) {

                    var focuselement = null;
                    if (typeof ($scope.FocusConfig[index].apermission) == 'undefined' &&
                        typeof ($scope.FocusConfig[index].dpermission) == 'undefined') {
                        focuselement = $scope.FocusConfig[index].element;
                        done = true;
                    } else if ($scope.FocusConfig[index].apermission &&
                        $scope.Mandatory.attributes[$scope.FocusConfig[index].apermission]) {
                        focuselement = $scope.FocusConfig[index].element;
                        done = true;
                    } else if ($scope.FocusConfig[index].dpermission &&
                        !$scope.Mandatory.attributes[$scope.FocusConfig[index].dpermission]) {
                        focuselement = $scope.FocusConfig[index].element;
                        done = true;
                    }

                    if (done) {
                        $timeout(function () {
                            $scope['setFocus' + focuselement]();
                        }, 50);
                    }
                }
            }
        }

        $scope.CurrentPurchaseTaxConfig = null;
        $scope.CurrentSalesTaxConfig = null;
        $scope.taxChange = function (refresh) {
            $scope.ItemDetails.ptaxpercentage = 0;
            $scope.ItemDetails.staxpercentage = 0;

            if ($scope.ItemDetails.taxid != null) {
                var taxitem = $scope.PurchaseTax.find(function (item) {
                    return (item.id == $scope.ItemDetails.taxid);
                });
                if (taxitem || taxitem != null) {
                    $scope.CurrentSalesTaxConfig = taxitem;
                    if (taxitem != null && typeof (taxitem.taxsplit.findTax)) {
                        taxitem.taxsplit.findSalesTax = $scope.findSalesTax;
                    }
                }
            }

            if ($scope.ItemDetails.ptaxid != null) {
                var taxitem = $scope.PurchaseTax.find(function (item) {
                    return (item.id == $scope.ItemDetails.ptaxid);
                });
                if (taxitem || taxitem != null) {
                    $scope.CurrentPurchaseTaxConfig = taxitem;
                    if (taxitem != null && typeof (taxitem.taxsplit.findTax)) {
                        taxitem.taxsplit.findTax = $scope.findTax;
                    }
                    $scope.fixTax(refresh);
                }
            }

            if (typeof (refresh) == 'undefined' || refresh)
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

        $scope.IsUANBased = function () {
            return ($scope.system_supportskudesignpurchase &&
                $scope.ItemDetails.Details.serialno &&
                $scope.ItemDetails.Details.serialno != null &&
                $scope.ItemDetails.Details.serialno != '' &&
                $scope.ItemDetails.itemid &&
                $scope.ItemDetails.itemid > 0);
        }

        $scope.CostChanged = function () {
            if ($scope.IsUANBased()) {
                $scope.sellingpriceChange();
            }
            $scope.fixTax(false);
        }

        $scope.fixTax = function (findDiscount) {
            var taxitem = $scope.CurrentPurchaseTaxConfig;
            if (taxitem == null && $scope.ItemDetails.ptaxid > 0) {
                $scope.taxChange();
                taxitem = $scope.CurrentPurchaseTaxConfig;
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

            if (taxitem == null) {
                if ($scope.ItemDetails.ptaxid != null) {
                    var taxitem = $scope.PurchaseTax.find(function (item) {
                        return (item.id == $scope.ItemDetails.ptaxid);
                    });
                    if (taxitem || taxitem != null) {
                        $scope.CurrentPurchaseTaxConfig = taxitem;
                        if (taxitem != null && typeof (taxitem.taxsplit.findTax)) {
                            taxitem.taxsplit.findTax = $scope.findTax;
                        }
                    }
                }
            }
            if (taxitem != null) {
                var taxconfig = taxitem.taxsplit.findTax($scope.ItemDetails.buyingprice);
                if (taxconfig.cgst == 0) {
                    taxconfig = {
                        'cgst': 0,
                        'sgst': 0
                    };
                }
                if ($scope.Invoice.IsUnregisteredInvoice)
                    $scope.ItemDetails.purchasetaxpercentage = 0;
                else
                    $scope.ItemDetails.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
                $scope.calculatesalestax();
                if (!$scope.ItemBasedCostFixed) $scope.marginChanged();
            }
        }


        $scope.findSalesTax = function (amount) {
            if (this.items == null || this.items.length == 0)
                return null;

            if (this.items.length == 1)
                return this.items[0];

            if (isNaN(amount)) amount = 0;
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];

                var tamount = amount / (100 + item.cgst + item.sgst) * 100;

                if (item.fromamount <= tamount && item.toamount >= tamount)
                    return item;
            }
            return {
                'cgst': 0,
                'sgst': 0
            };
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
            return {
                'cgst': 0,
                'sgst': 0
            };
        }

        $scope.fixPurhcaseDiscount = function () {
            var discountpercent = 0;
            if ($scope.ItemDetails && $scope.ItemDetails.id && $scope.ItemDetails.id > 0) return;
            var discountitem = null;
            if ($scope.Invoice.Lrinvoicetaxitems.length == 1) {
                discountitem = $scope.Invoice.Lrinvoicetaxitems[0];
            } else if ($scope.purchasediscounttaxitem != null) {
                discountitem = $scope.purchasediscounttaxitem;
            } else {
                discountitem = $scope.Invoice.Lrinvoicetaxitems.find(function (k) {
                    return Math.abs(k.taxid) == Math.abs($scope.ItemDetails.ptaxid);
                });
            }

            if (typeof (discountitem) != 'undefined' && discountitem != null) {
                if (discountitem.discountpercentage > 0) {
                    discountpercent = discountitem.discountpercentage;
                } else if (discountitem.discount > 0) {
                    discountpercent = discountitem.discount / discountitem.amounton * 100;
                }
                if ((discountitem.type == 2 || discountitem.type == 3) || (discountitem.type == 0 && discountitem.ptaxid == 0))
                    discountpercent = 0;
            }
            $scope.ItemDetails.purchasediscountpercentage = discountpercent + $scope.additionaldiscountpercentage;
        }

        $scope.fixPrice = function () {

            $scope.ItemDetails.purchasediscount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.purchasediscountpercentage) / 100).round(4);
            if (isNaN($scope.ItemDetails.purchasediscount))
                $scope.ItemDetails.purchasediscount = 0;

            if ($scope.SupportAdditionalOnCost) {
                $scope.ItemDetails.additionalcost = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.Invoice.addtionalcostpercentage / 100);
                $scope.ItemDetails.buyingprice = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) - $scope.ItemDetails.purchasediscount);

                if ($scope.ItemDetails.purchasetaxpercentage > 0 && $scope.Invoice.isinclusivetax) {
                    $scope.ItemDetails.buyingprice = $scope.ItemDetails.buyingprice / (100 + $scope.ItemDetails.purchasetaxpercentage) * 100;
                }
                //$scope.ItemDetails.buyingprice = ($scope.ItemDetails.buyingprice + $scope.ItemDetails.additionalcost).round(2);
            } else {
                $scope.ItemDetails.buyingprice = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) - $scope.ItemDetails.purchasediscount).round(4);
            }
            if ($scope.Invoice.plessvalue > 0)
                $scope.ItemDetails.buyingprice = $scope.ItemDetails.buyingprice - $scope.Invoice.plessvalue;
        };



        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0;

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };


        $scope.TryAddItem = function () {


            if (typeof ($scope.ProductSearch.ProductID) == 'undefined' || $scope.ProductSearch.ProductID == 0) {
                $rootScope.ShowAlert('Please select the product', 'Mandatory');
                return;
            }

            if (typeof ($rootScope.UserInfo.appsettings.system_isb2bapplication) != 'undefined' &&
                $rootScope.UserInfo.appsettings.system_isb2bapplication) {
                if (typeof ($scope.ItemDetails.designid) == 'undefined' || $scope.ItemDetails.designid == null ||
                    $scope.ItemDetails.designid == '') {
                    $rootScope.Toaster.Info('Please enter the design');
                    return;
                }
            }

            if ($scope.CurrentProduct != null && $scope.CurrentProduct.attributes != null) {
                if ($scope.CurrentProduct.attributes.mandatorybrand) {
                    if (typeof ($scope.ItemDetails.Details.brandid) == 'undefined' || $scope.ItemDetails.Details.brandid == null || $scope.ItemDetails.Details.brandid == 0) {
                        $rootScope.Toaster.Info('Please select the brand');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatorymaterial) {
                    if (typeof ($scope.ItemDetails.Details.materialid) == 'undefined' || $scope.ItemDetails.Details.materialid == null || $scope.ItemDetails.Details.materialid == 0) {
                        $rootScope.Toaster.Info('Please select the material');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatorypattern) {
                    if (typeof ($scope.ItemDetails.Details.patternid) == 'undefined' || $scope.ItemDetails.Details.patternid == null || $scope.ItemDetails.Details.patternid == 0) {
                        $rootScope.Toaster.Info('Please select the pattern');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatorystyle) {
                    if (typeof ($scope.ItemDetails.Details.styleid) == 'undefined' || $scope.ItemDetails.Details.styleid == null || $scope.ItemDetails.Details.styleid == 0) {
                        $rootScope.Toaster.Info('Please select the style');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatorysize) {
                    if (typeof ($scope.ItemDetails.Details.sizeid) == 'undefined' || $scope.ItemDetails.Details.sizeid == null || $scope.ItemDetails.Details.sizeid == 0) {
                        $rootScope.Toaster.Info('Please select the size');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatorysleeve) {
                    if (typeof ($scope.ItemDetails.Details.sleeveid) == 'undefined' || $scope.ItemDetails.Details.sleeveid == null || $scope.ItemDetails.Details.sleeveid == 0) {
                        $rootScope.Toaster.Info('Please select the Sleeve');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatoryfit) {
                    if (typeof ($scope.ItemDetails.Details.fitid) == 'undefined' || $scope.ItemDetails.Details.fitid == null || $scope.ItemDetails.Details.fitid == 0) {
                        $rootScope.Toaster.Info('Please select the Fit');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatorytype) {
                    if (typeof ($scope.ItemDetails.Details.typeid) == 'undefined' || $scope.ItemDetails.Details.typeid == null || $scope.ItemDetails.Details.typeid == 0) {
                        $rootScope.Toaster.Info('Please select the type');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatorybatchandexpiry) {
                    if (typeof ($scope.ItemDetails.Details.batch) == 'undefined' || $scope.ItemDetails.Details.batch == null || $scope.ItemDetails.Details.batch == '') {
                        $rootScope.Toaster.Info('Please enter the batch/expiry');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatoryserialno) {
                    if (typeof ($scope.ItemDetails.Details.serialno) == 'undefined' || $scope.ItemDetails.Details.serialno == null || $scope.ItemDetails.Details.serialno == '') {
                        $rootScope.Toaster.Info('Please enter the serial/imei no');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatoryquality) {
                    if (typeof ($scope.ItemDetails.Details.patternid) == 'undefined' || $scope.ItemDetails.Details.patternid == null || $scope.ItemDetails.Details.patternid == 0) {
                        $rootScope.Toaster.Info('Please select the quality');
                        return;
                    }
                }

                if ($scope.CurrentProduct.attributes.mandatorywarrenty) {
                    if (typeof ($scope.ItemDetails.Details.materialid) == 'undefined' || $scope.ItemDetails.Details.materialid == null || $scope.ItemDetails.Details.materialid == 0) {
                        $rootScope.Toaster.Info('Please select the material');
                        return;
                    }
                }
            }


            if ($scope.SupportPOMandatory) {

                var oqty = 0;

                if (typeof ($scope.ItemDetails.poinfo) != 'undefined' && $scope.ItemDetails.poinfo != null &&
                    $scope.ItemDetails.poinfo.length > 0) {
                    $.each($scope.ItemDetails.poinfo, function (k, v) {
                        oqty += v.aqty;
                    });
                }

                if (oqty < $scope.GetNumber($scope.ItemDetails.buyingqty)) {

                    var edit = typeof ($scope.ItemDetails.id) != 'undefined' && $scope.ItemDetails.id > 0;
                    $scope.AllocatePOItems($scope.ItemDetails, edit, function (additem) {
                        if (additem && $scope.ValidateImageCapture()) {
                            $scope.AddItem();
                        }
                    });
                    return;
                }
            }

            if ($scope.system_supportskudesignpurchase) {
                if ($scope.ItemDetails.designid == undefined || $scope.ItemDetails.designid == null || $scope.ItemDetails.designid == '' ||
                    $scope.ProductSearch.CurrentSKUList == undefined || $scope.ProductSearch.CurrentSKUList == null || $scope.ProductSearch.CurrentSKUList.length == 0) {
                    $rootScope.Confirm('Given Design doesnt have UAN informtion in SKU, do you want to continue', 'UAN Missing', function () {
                        if ($scope.ValidateImageCapture()) {
                            $scope.AddItem();
                        }
                    });
                    return;
                }
                else {
                    var missinguan = 0;
                    if ($scope.ItemDetails.Child && $scope.ItemDetails.Child != null &&
                        $scope.ItemDetails.Child.length > 0) {
                        $.each($scope.ItemDetails.Child, function (k, v) {
                            var uanitems = [];
                            v.Details.colourid = $scope.ItemDetails.Details.colourid;
                            v.Details.styleid = $scope.ItemDetails.Details.styleid;
                            v.Details.fitid = $scope.ItemDetails.Details.fitid;
                            v.Details.sleeveid = $scope.ItemDetails.Details.sleeveid;
                            $.each($scope.ProductSearch.CurrentSKUList, function (kk, vv) {
                                if (vv.sizeid == v.sizeid && vv.colourid == v.Details.colourid) {
                                    uanitems.push(vv);
                                }
                            });

                            if (uanitems.length > 1) {
                                if (v.Details.sleeveid && v.Details.sleeveid != null && v.Details.sleeveid > 0) {
                                    var sleeveuanitem = uanitem.find(function (item) {
                                        return item.sleeveid == v.Details.sleeveid;
                                    });
                                    if (sleeveuanitem && sleeveuanitem != null) {
                                        uanitems = [sleeveuanitem];
                                    }
                                }
                            }

                            if (uanitems.length > 0) {
                                v.Details.serialno = uanitems[0].uid;
                                v.itemid = uanitems[0].itemid;
                            }
                            else {
                                missinguan++;
                            }
                        })
                    }
                    else {
                        var uanitems = [];
                        if ($scope.ItemDetails.Details.sleeveid == undefined || $scope.ItemDetails.Details.sleeveid == null ||
                            $scope.ItemDetails.Details.sleeveid == '')
                            $scope.ItemDetails.Details.sleeveid = 0;

                        if ($scope.ItemDetails.Details.colourid == undefined || $scope.ItemDetails.Details.colourid == null ||
                            $scope.ItemDetails.Details.colourid == '')
                            $scope.ItemDetails.Details.colourid = 0;

                        if ($scope.ItemDetails.Details.sizeid == undefined || $scope.ItemDetails.Details.sizeid == null ||
                            $scope.ItemDetails.Details.sizeid == '')
                            $scope.ItemDetails.Details.sizeid = 0;

                        $.each($scope.ProductSearch.CurrentSKUList, function (kk, vv) {
                            if (($scope.ItemDetails.Details.sizeid <= 0 || vv.sizeid == $scope.ItemDetails.Details.sizeid)
                                && (vv.colourid == 0 || $scope.ItemDetails.Details.colourid == 0 || vv.colourid == $scope.ItemDetails.Details.colourid)
                                && (vv.sleeveid == 0 || $scope.ItemDetails.Details.sleeveid == 0 || vv.sleeveid == $scope.ItemDetails.Details.sleeveid)) {
                                uanitems.push(vv);
                            }
                        });

                        if (uanitems.length > 0) {
                            $scope.ItemDetails.Details.serialno = uanitems[0].uid;
                            $scope.ItemDetails.uan = uanitems[0].uid;
                            $scope.ItemDetails.itemid = uanitems[0].itemid;
                        }

                        if ($scope.ItemDetails.Details.sizeid > 0 &&
                            uanitems.length > 1) {
                            missinguan++;
                        }
                    }
                    if (missinguan > 0) {
                        $scope.Toaster.Error("Few items can't find the UAN - " + missinguan);
                    }
                }
            }

            $scope.ItemBasedCostFixed = false;
            if ($scope.ValidateImageCapture()) {
                $scope.AddItem();
            }
        }

        $scope.ValidateImageCapture = function () {
            if (!$scope.SupportImageRecording)
                return true;
            $scope.ShowImageDialog(function (value) {
                if (value && value != null)
                    $scope.ItemDetails.imgid = value;
                $scope.AddItem();
            }, $scope.ItemDetails.imgid);
            return false;
        }

        $scope.AddItem = function () {

            if ($scope.SupportPOMandatory && typeof ($scope.ItemDetails.poinfo) != 'undefined' && $scope.ItemDetails.poinfo != null &&
                $scope.ItemDetails.poinfo.length > 0) {
                if ($scope.ItemDetails.excesspo) {
                    $scope.Toaster.Error("Please change the purchase order qty before proceed.");
                    return;
                }
            }


            if ($scope.Invoice.Lrinvoiceitems == undefined ||
                $scope.Invoice.Lrinvoiceitems == null ||
                $scope.Invoice.Lrinvoiceitems.length == 0) {
                $scope.EntryGridConfig();
            }

            if (typeof ($scope.ItemDetails.salediscountpercentage) != 'undefined' && $scope.ItemDetails.salediscountpercentage != null) {
                if (!$scope.GetNumber($scope.ItemDetails.salediscountpercentage)) {
                    if ($scope.ItemDetails.salediscountpercentage.toString().toLowerCase().startsWith('f')) {
                        var finalvalue = $scope.GetNumber($scope.ItemDetails.salediscountpercentage.toString().substring(1));
                        $scope.ItemDetails.salediscount = $scope.ItemDetails.sellingprice - finalvalue;
                        $scope.ItemDetails.salediscountpercentage = ($scope.ItemDetails.salediscount / $scope.ItemDetails.sellingprice * 100).round(2);
                        $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                        $scope.calculatesalestax();
                    } else {
                        $scope.ItemDetails.salediscount = 0;
                        $scope.ItemDetails.salediscountpercentage = 0;
                        $scope.calculatesalestax();
                    }
                }
            }
            else {
                $scope.ItemDetails.salediscount = 0;
                $scope.ItemDetails.salediscountpercentage = 0;
                $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                $scope.calculatesalestax();
            }

            var duplicatesno = $scope.Invoice.Lrinvoiceitems.find(function (item) {
                return !item.IsEditItem && (typeof (item.IsChild) == 'undefined' || !item.IsChild) &&
                    item.sno == $scope.ItemDetails.sno;
            });

            if (typeof (duplicatesno) != 'undefined' && duplicatesno != null) {
                $rootScope.ShowAlert('Please enter different serial number', 'Duplicate serial no');
                return;
            }

            var currentsupplier = $scope.SupplierList.find(function (item) {
                return item.id == $scope.Invoice.supplierid;
            });

            if (currentsupplier && currentsupplier.maxmargin > 0 && currentsupplier.minmargin > 0) {
                var marginpercentage = $scope.GetNumber($scope.ItemDetails.marginpercentage);
                if (currentsupplier.maxmargin < marginpercentage) {
                    $rootScope.ShowAlert(currentsupplier.name + ' Maximum allowed margin ' + currentsupplier.maxmargin + '%', 'Maximum Margin Limit');
                    return;
                }
                if (currentsupplier.minmargin > marginpercentage) {
                    $rootScope.ShowAlert(currentsupplier.name + ' Maximum allowed margin ' + currentsupplier.minmargin + '%', 'Minimum Margin Limit');
                    return;
                }
            }

            $scope.ItemDetails.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.Details.productid = $scope.ProductSearch.ProductID;
            var currentproduct = $scope.AllProductList.find(function (item) {
                return item.id == $scope.ItemDetails.productid;
            });

            if (typeof (currentproduct) == 'undefined' || currentproduct == null) {
                $rootScope.Toaster.Info("Please select the product!");
                return;
            }

            if (currentproduct.maxmargin > 0 && currentproduct.minmargin > 0) {
                var marginpercentage = $scope.GetNumber($scope.ItemDetails.marginpercentage);
                if (currentproduct.maxmargin < marginpercentage) {
                    $rootScope.ShowAlert(currentproduct.name + ' Maximum allowed margin ' + currentproduct.maxmargin + '%', 'Maximum Margin Limit');
                    return;
                }
                if (currentproduct.minmargin > marginpercentage) {
                    $rootScope.ShowAlert(currentproduct.name + ' Maximum allowed margin ' + currentproduct.minmargin + '%', 'Minimum Margin Limit');
                    return;
                }
            }

            if ($scope.system_invcol_pricetag) {
                $scope.UpdatePriceTag($scope.ItemDetails);
            }

            var showalert = false;
            var pricechanged = false;
            $scope.ItemDetails.Details.productname = currentproduct.name;
            if ($scope.ItemDetails != null && $scope.ItemDetails.id &&
                $scope.ItemDetails.id > 0 && $scope.ItemDetails.IsParent &&
                $scope.ItemDetails.Child && $scope.ItemDetails.Child != null &&
                $scope.ItemDetails.Child.length > 0 && $scope.ItemDetails.OLD &&
                ($scope.ItemDetails.OLD.sellingprice != $scope.ItemDetails.sellingprice ||
                    $scope.ItemDetails.OLD.buyingprice != $scope.ItemDetails.buyingprice ||
                    $scope.ItemDetails.OLD.salediscountpercentage != $scope.ItemDetails.salediscountpercentage)) {

                pricechanged = true;
                $.each($scope.ItemDetails.Child, function (childkey, childvalue) {
                    childvalue.basebuyingprice = $scope.ItemDetails.basebuyingprice;
                    childvalue.buyingprice = $scope.ItemDetails.buyingprice;
                    childvalue.purchasediscountpercentage = $scope.ItemDetails.purchasediscountpercentage;
                    childvalue.ptaxid = $scope.ItemDetails.ptaxid;
                    childvalue.ptaxpercentage = $scope.ItemDetails.ptaxpercentage;
                    childvalue.purchasediscount = $scope.ItemDetails.purchasediscount;
                    childvalue.purchasetaxpercentage = $scope.ItemDetails.purchasetaxpercentage;
                    childvalue.purchasetotaldiscount = $scope.ItemDetails.purchasetotaldiscount;
                    childvalue.purchasetax = $scope.ItemDetails.purchasetax;
                    childvalue.salestaxpercentage = $scope.ItemDetails.salestaxpercentage;
                    childvalue.designid = $scope.ItemDetails.designid;
                    childvalue.hsncode = $scope.ItemDetails.hsncode;
                    childvalue.marginpercentage = $scope.ItemDetails.marginpercentage;
                    childvalue.salediscountpercentage = $scope.ItemDetails.salediscountpercentage;

                    if (typeof ($scope.ItemDetails.sellableqty) == 'undefined' || $scope.ItemDetails.sellableqty == 0) {
                        childvalue.mrp = $scope.ItemDetails.mrp;
                        childvalue.dealerprice = $scope.ItemDetails.dealerprice;

                        childvalue.amount = (($scope.Invoice.isinclusivediscount ? $scope.GetNumber(childvalue.basebuyingprice) : $scope.GetNumber(childvalue.buyingprice)) *
                            $scope.GetNumber(childvalue.buyingqty)).round(2);

                        var baseCost = $scope.GetNumber($scope.SupportMarginOnCost ? childvalue.basebuyingprice : childvalue.buyingprice);
                        childvalue.sellingprice = baseCost + (baseCost * $scope.GetNumber(childvalue.marginpercentage) / 100);


                        if ($scope.SupportMarginexcludetax) {
                            var newtaxvalue = $scope.GetNumber(childvalue.sellingprice) * childvalue.salestaxpercentage / 100;
                            var newsellingprice = $scope.RoundPrice(newtaxvalue + childvalue.sellingprice);
                            var taxitem = $scope.CurrentPurchaseTaxConfig;
                            if (taxitem != null) {
                                if (taxitem != null && typeof (taxitem.taxsplit.findTax)) {
                                    taxitem.taxsplit.findTax = $scope.findTax;
                                }
                                var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber(childvalue.sellingprice));
                                if (childvalue.salestaxpercentage < taxconfig.cgst + taxconfig.sgst) {
                                    var newtaxvalue = $scope.GetNumber(childvalue.sellingprice) * (taxconfig.cgst + taxconfig.sgst) / 100;
                                    newsellingprice = $scope.RoundPrice(newtaxvalue + childvalue.sellingprice);
                                }
                            }
                            childvalue.sellingprice = newsellingprice;
                        } else {
                            childvalue.sellingprice = $scope.RoundPrice(childvalue.sellingprice);
                        }


                        childvalue.sellingprice = $scope.RoundPrice(childvalue.sellingprice);
                        childvalue.salediscount = (childvalue.sellingprice * childvalue.salediscountpercentage / 100).round(0);
                        if (!$scope.SupportSalepriceMrpdifference)
                            childvalue.mrp = childvalue.sellingprice;
                        childvalue.finalprice = childvalue.sellingprice - childvalue.salediscount;
                    }
                    else {
                        showalert = true;
                    }
                });
                if (showalert)
                    $rootScope.ShowAlert('Critical Alert', 'Cost Price Changed, But Selling Price/MRP have not been changed for the item having Barcode.');
            }

            if ($scope.CollectLocationInfo && $scope.Invoice.retaillocationid > 0) {
                $scope.ItemDetails.stocklocationid = $scope.Invoice.retaillocationid;
            }
            if ($scope.ItemDetails != null && $scope.ItemDetails.Details != null &&
                $scope.ItemDetails.Details.sizeid == -2) {
                $scope.CreateMultiItems($scope.ItemDetails, true, false, false, pricechanged);
            } else if ($scope.ItemDetails != null && $scope.ItemDetails.Details != null &&
                $scope.ItemDetails.Details.sizeid == -3) {
                $scope.CreateMultiItems($scope.ItemDetails, false, false, false, pricechanged);
            } else if ($scope.ItemDetails != null && $scope.ItemDetails.Details != null &&
                $scope.ItemDetails.Details.sizeid == -4) {
                $scope.CreateMultiItems($scope.ItemDetails, true, false, true, pricechanged);
            } else {
                var productitem = $scope.AllProductList.find(function (item) {
                    return item.id == $scope.ItemDetails.productid;
                });
                if (productitem != null && productitem.sellingmode == 3) {
                    $scope.CreateMultiItems($scope.ItemDetails, false, false, $scope.ItemDetails.Details.sizeid == -4, pricechanged);
                } else {
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
                    } else {

                        if (typeof ($scope.ItemDetails.Child) != 'undefined' && $scope.ItemDetails.Child != null && $scope.ItemDetails.Child.length > 0) {
                            $.each($scope.ItemDetails.Child, function (k, v) {
                                if ($scope.Invoice.Lrinvoiceitems.indexOf(v) >= 0)
                                    $scope.Invoice.Lrinvoiceitems.splice($scope.Invoice.Lrinvoiceitems.indexOf(v), 1);
                            });
                            $scope.ItemDetails.Child = [];
                            $scope.ItemDetails.IsParent = false;
                        }

                        if ($scope.CollectLocationInfo) {
                            if ($scope.Invoice.retaillocationid > 0) {
                                $scope.ItemDetails.stocklocationid = $scope.Invoice.retaillocationid;

                                if (typeof ($scope.ItemDetails.Child) != 'undefined' && $scope.ItemDetails.Child.length > 0) {
                                    $.each($scope.ItemDetails.Child, function (k, v) {
                                        v.stocklocationid = $scope.Invoice.retaillocationid;
                                    });
                                }
                                $scope.AddItemInternal(true);
                                $scope.CreateNew();
                            } else {
                                $scope.ChooseLocation($scope.ItemDetails, function () {
                                    $scope.AddItemInternal(true);
                                    $scope.CreateNew();
                                });
                            }
                        } else {

                            if (typeof ($scope.Mandatory.attributes.uploadbarcode) != 'undefined' && $scope.Mandatory.attributes.uploadbarcode)
                                $scope.UploadBarcodes();
                            else {
                                $scope.AddItemInternal(true);
                                $scope.CreateNew();
                            }
                        }
                    }
                }
            }
        };

        $scope.UpdatePriceTag = function (itemDetail) {
            itemDetail.Details.pricetagid = 0;
            if ($scope.PriceTagList.length > 0) {
                var product = $scope.ProductList.find(function (item) {
                    return item.id == $scope.ItemDetails.productid;
                });
                if (product && product.productgroup &&
                    product.productgroup > 0) {
                    for (var i = 0; i < $scope.PriceTagList.length; i++) {
                        if ($scope.PriceTagList[i].additionalinfo.pricetags &&
                            $scope.PriceTagList[i].additionalinfo.pricetags != null) {
                            var pricetag = $scope.PriceTagList[i].additionalinfo.pricetags.find(function (item) {
                                return item.pgid == product.productgroup &&
                                    (
                                        item.from <= itemDetail.finalprice &&
                                        item.to >= itemDetail.finalprice
                                    );
                            });
                            if (pricetag && pricetag != null) {
                                itemDetail.Details.pricetagid = $scope.PriceTagList[i].id;
                                itemDetail.pricetagid = $scope.PriceTagList[i].id;
                                if (itemDetail.IsParent && itemDetail.Child && itemDetail.Child != null) {
                                    $.each(itemDetail.Child, function (k, v) {
                                        v.Details.pricetagid = itemDetail.pricetagid;
                                    });
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.Invoice', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.Invoice = value;
            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
        };

        $scope.CreateNew = function () {

            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
            $scope.ItemDetails.Child = [];
            $scope.ItemDetails.buyingqty = null;
            $scope.ItemDetails.basebuyingprice = null;
            $scope.ItemDetails.buyingprice = null;
            $scope.ItemDetails.dealerprice = null;
            $scope.ItemDetails.sellingprice = null;
            $scope.ItemDetails.finalprice = null;
            $scope.ItemDetails.mrp = null;
            $scope.ItemDetails.purchasetax = null;
            $scope.ItemDetails.salestax = null;
            $scope.ItemDetails.imgid = '';
            $scope.ItemDetails.poinfo = [];
            $scope.ItemDetails.polist = '';
            $scope.ItemDetails.postatus = 'No Order';
            $scope.ItemDetails.salediscount = 0;
            $scope.ItemDetails.Details.serialno = '';
            $scope.ItemDetails.uan = '';
            $scope.ItemDetails.IsChild = false;
            $scope.ItemDetails.IsParent = false;
            $scope.ItemDetails.barcode = '';
            $scope.ItemDetails.barcodes = '';
            $scope.ItemDetails.barcodeList = [];
            $scope.ItemDetails.sellableqty = 0;
            $scope.ItemDetails.returnqty = 0;
            $scope.ItemDetails.damagedqty = 0;
            if ($scope.Invoice.Lrinvoiceitems.length > 0)
                $scope.ItemDetails.sno = parseInt($scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].sno) + 1;
            else
                $scope.ItemDetails.sno = 1;

            if ($scope.SupportUANPurchase)
                $scope['setFocustserialno']();
            else
                $scope['setFocussno']();

            $scope.LocalContext.Post('$scope.Invoice', $scope.Invoice, false);
        }



        $scope.defaultTaxID = function () {
            if ($scope.Invoice.Lrinvoicetaxitems.length > 0)
                return $scope.Invoice.Lrinvoicetaxitems[0].ptaxid;
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
                if ($scope.LastSelectedItem != null && $scope.LastSelectedItem.productid == $scope.ItemDetails.Details.productid &&
                    $scope.LastSelectedItem.brandid == $scope.ItemDetails.Details.brandid) {
                    $scope.ItemDetails.Details.printingname = $scope.LastSelectedItem.printingname;
                    $scope.ItemDetails.Details.sellingname = $scope.LastSelectedItem.sellingname;
                    $scope.ItemDetails.Details.sourceitemid = $scope.LastSelectedItem.id;
                } else {
                    $scope.ItemDetails.Details.sourceitemid = 0;
                    $scope.ItemDetails.Details.printingname = $scope.SearchRef($scope.ItemDetails.productid, $scope.ProductList, '');
                }
            }

            if (typeof ($scope.Invoice.Lrinvoiceitems) == 'undefined')
                $scope.Invoice.Lrinvoiceitems = [];
            $scope.ItemDetails.buyingqty = qty;
            $scope.fixTax();
            $scope.ItemDetails.amount = (($scope.Invoice.isinclusivediscount ? $scope.GetNumber($scope.ItemDetails.basebuyingprice) : $scope.GetNumber($scope.ItemDetails.buyingprice)) *
                $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);

            for (var i = 0; i < pieces; i++) {
                var newitem = JSON.parse(JSON.stringify($scope.ItemDetails));
                newitem.sno = newitem.sno + (i / 10);
                $scope.Invoice.Lrinvoiceitems.push($scope.SetRowStyle(newitem));
            }
            $scope.CalculateTotal();
        };

        $scope.AddItemInternal = function (computetotal) {

            $scope.ItemDetails.IsEditItem = false;
            $scope.ItemDetails.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.Details.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.searchtext = $scope.ProductSearch.SearchItemName;
            $scope.ItemDetails.sno = parseFloat($scope.ItemDetails.sno);
            $scope.ItemDetails.amount = $scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty);
            $scope.ItemDetails.purchasetotaldiscount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty) * $scope.GetNumber($scope.ItemDetails.purchasediscountpercentage) / 100).round(2);
            $scope.ItemDetails.amount -= $scope.ItemDetails.purchasetotaldiscount;
            $scope.ItemDetails.purchasediscount = ($scope.ItemDetails.purchasetotaldiscount / $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);
            if ($scope.Invoice.IsUnregisteredInvoice)
                $scope.ItemDetails.purchasetaxpercentage = 0;
            else
                $scope.ItemDetails.purchasetax = ($scope.ItemDetails.amount * $scope.ItemDetails.purchasetaxpercentage / 100).round(2);
            if ($scope.SupportDealerPriceWOTax &&
                typeof ($scope.ItemDetails.dealerprice) != 'undefined') {
                var salestaxpercentage = 0;
                var taxitem = $scope.CurrentSalesTaxConfig;
                if (taxitem != null) {
                    var dprice = $scope.GetNumber($scope.ItemDetails.dealerprice);
                    var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.dealerprice));
                    salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
                    $scope.ItemDetails.dealerprice = (dprice + (dprice * salestaxpercentage / 100)).round(4);
                }
            }

            if (typeof ($scope.ItemDetails.Details.sizeid) != 'undefined' && $scope.ItemDetails.Details.sizeid > 0) {
                $scope.ItemDetails.Details.sizename = restService.SearchReference($scope.ItemDetails.Details.sizeid, $scope.SizeList);
            }

            $scope.ItemDetails.searchtext = $scope.ProductSearch.SearchItemName;
            $scope.ItemDetails.sno = parseFloat($scope.ItemDetails.sno);
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
                if (typeof ($scope.ItemDetails.itemid) == 'undefined' || $scope.ItemDetails.itemid == 0) {
                    if ($scope.LastSelectedItem != null && $scope.LastSelectedItem.productid == $scope.ItemDetails.Details.productid &&
                        $scope.LastSelectedItem.brandid == $scope.ItemDetails.Details.brandid) {
                        $scope.ItemDetails.Details.printingname = $scope.LastSelectedItem.printingname;
                        $scope.ItemDetails.Details.sellingname = $scope.LastSelectedItem.sellingname;
                        $scope.ItemDetails.Details.sourceitemid = $scope.LastSelectedItem.id;
                    } else {
                        $scope.ItemDetails.Details.sourceitemid = 0;
                        $scope.ItemDetails.Details.printingname = $scope.SearchRef($scope.ItemDetails.productid, $scope.ProductList, '');
                        $scope.ItemDetails.Details.sellingname = $scope.ItemDetails.Details.printingname;
                    }
                }
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
                            $scope.Invoice.Lrinvoiceitems.splice(index, 0, $scope.SetRowStyle($scope.ItemDetails));
                        else
                            $scope.Invoice.Lrinvoiceitems.push($scope.SetRowStyle($scope.ItemDetails));
                    } else {
                        $scope.Invoice.Lrinvoiceitems.push($scope.SetRowStyle($scope.ItemDetails));
                    }
                } else {
                    $scope.Invoice.Lrinvoiceitems.push($scope.SetRowStyle($scope.ItemDetails));
                }
            }
            if (typeof (computetotal) == 'undefined' || computetotal)
                $scope.CalculateTotal();

            $scope.ItemDetails.amount = (($scope.Invoice.isinclusivediscount ? $scope.GetNumber($scope.ItemDetails.basebuyingprice) : $scope.GetNumber($scope.ItemDetails.buyingprice)) *
                $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);


            if ($scope.system_supportskudesignpurchase && $scope.ProductSearch.CurrentSKUList &&
                $scope.ProductSearch.CurrentSKUList.length > 0) {
                var uanitems = [];
                $.each($scope.ProductSearch.CurrentSKUList, function (kk, vv) {
                    if (vv.sizeid == $scope.ItemDetails.Details.sizeid && vv.colourid == $scope.ItemDetails.Details.colourid) {
                        uanitems.push(vv);
                    }
                });

                if (uanitems.length > 1) {
                    if ($scope.ItemDetails.Details.sleeveid && $scope.ItemDetails.Details.sleeveid != null && $scope.ItemDetails.Details.sleeveid > 0) {
                        var sleeveuanitem = uanitems.find(function (item) {
                            return item.sleeveid == $scope.ItemDetails.Details.sleeveid;
                        });
                        if (sleeveuanitem && sleeveuanitem != null) {
                            uanitems = [sleeveuanitem];
                        }
                    }
                }

                if (uanitems.length > 0) {
                    $scope.ItemDetails.Details.serialno = uanitems[0].uid;
                    $scope.ItemDetails.Details.uan = uanitems[0].uid;
                    $scope.ItemDetails.itemid = uanitems[0].itemid;
                }
            }
            //$scope.CreateNew();
        };

        $scope.SetRowStyle = function (item) {

            if (typeof (item.poinfo) != 'undefined' &&
                item.poinfo != null && item.poinfo.length > 0)
                item.style = {
                    "background-color": "lightgrey"
                };
            else
                item.style = {};
            return item;

        }

        $scope.RoundPrice = function (amount) {

            if (!$scope.SupportRounding) return Math.round(amount, 0).toFixed(0);

            if ($scope.PurchaseRoundingConfig != null && $scope.PurchaseRoundingConfig.length > 0) {
                var roundpos = -1;
                for (var i = 0; i < $scope.PurchaseRoundingConfig.length && roundpos == -1; i++) {
                    if ($scope.PurchaseRoundingConfig[i].start <= amount &&
                        $scope.PurchaseRoundingConfig[i].end >= amount) {
                        roundpos = i;
                    }
                }
                if (roundpos == -1) {
                    if ($scope.PurchaseRoundingConfig[0].start > amount) {
                        roundpos = 0;
                    } else {
                        roundpos = $scope.PurchaseRoundingConfig.length - 1;
                    }
                }
                return (Math.ceil(amount / $scope.PurchaseRoundingConfig[roundpos].round) * $scope.PurchaseRoundingConfig[roundpos].round);
            }

            if ($scope.PurchaseRounding == 12 ||
                $scope.PurchaseRounding == 9 ||
                $scope.PurchaseRounding == 6 ||
                $scope.PurchaseRounding == 1) {
                var roundamount = 25;
                if ($scope.PurchaseRounding == 6)
                    roundamount = 10;
                if ($scope.PurchaseRounding == 1)
                    roundamount = 5;
                if ($scope.PurchaseRounding == 9)
                    roundamount = 1;

                var newamount = amount - (amount % roundamount);
                var reminder = amount % roundamount;
                if (roundamount / 2 < reminder)
                    newamount += roundamount;
                return newamount;
            } else {

                if (amount < 100) {
                    amount = Math.round(amount, 0);
                }

                if (amount > 100 && amount < 1000) {
                    amount = Math.round(amount, 0);
                    var diff = amount - (Math.floor(amount / 10) * 10);
                    if (diff > 5) {
                        if (diff - 5 >= 2.5) {
                            amount = (Math.floor(amount / 10) * 10) + 10;
                        } else {
                            amount = (Math.floor(amount / 10) * 10) + 5;
                        }
                    } else {
                        if (diff >= 2.5) {
                            amount = (Math.floor(amount / 10) * 10) + 5;
                        } else {
                            amount = (Math.floor(amount / 10) * 10);
                        }
                    }

                } else if (amount > 1000) {
                    amount = Math.round(amount, 0);
                    var diff = amount - (Math.floor(amount / 10) * 10);
                    if (diff > 5) {
                        amount = (Math.floor(amount / 10) * 10) + 10;
                    } else {
                        amount = (Math.floor(amount / 10) * 10);
                    }
                }
            }
            return amount;
        }

        $scope.finalpricechanged = function () {
            var fp = $scope.GetNumber($scope.ItemDetails.finalprice);
            $scope.ItemDetails.salediscount = $scope.GetNumber($scope.ItemDetails.sellingprice) - fp;
            if ($scope.ItemDetails.salediscount != 0)
                $scope.ItemDetails.salediscountpercentage = ($scope.ItemDetails.salediscount / $scope.ItemDetails.sellingprice * 100).round(2);
            else
                $scope.ItemDetails.salediscountpercentage = 0;
        }

        $scope.marginChanged = function (excludediscountchange, fromsalediscountchange) {


            if ($scope.system_marginpercentagezero && $scope.ItemDetails.id && $scope.ItemDetails.id > 0) {
                //if ($scope.ItemDetails.OLD && $scope.ItemDetails.OLD.productid != $scope.ItemDetails.productid) {
                return;
                //}
            }

            if (typeof (fromsalediscountchange) == 'undefined')
                fromsalediscountchange = false;

            var baseprice = 0;
            if ($scope.SupportMarginOnCost) {
                baseprice = $scope.GetNumber($scope.ItemDetails.basebuyingprice);
            } else {
                baseprice = $scope.ItemDetails.buyingprice;
            }


            if ($scope.SupportAddlrexptomargion) {

                if ($scope.Invoice.expenseperpiece > 0)
                    baseprice = baseprice + $scope.Invoice.expenseperpiece;

                if ($scope.Invoice.expensepercentage > 0)
                    baseprice = baseprice + (baseprice * $scope.Invoice.expensepercentage / 100).round(2);
            }

            if ($scope.SupportMarginincludetax) {
                var newtaxvalue = baseprice * ($scope.ItemDetails.purchasetaxpercentage) / 100;
                baseprice = newtaxvalue + baseprice;
            }

            var marginpercentage = $scope.GetNumber($scope.ItemDetails.marginpercentage);

            if (typeof ($scope.Invoice.premargin) != 'undefined' && $scope.Invoice.premargin > 0) {
                baseprice = baseprice + (baseprice * $scope.Invoice.premargin / 100).round(2);
            }

            if (typeof ($scope.ItemDetails.rowexpense) != 'undefined' && $scope.ItemDetails.rowexpense > 0) {
                baseprice = baseprice + (baseprice * $scope.ItemDetails.rowexpense / 100).round(2);
            }

            if ($scope.SupportMarkDOWN) {
                var markpercent = 1 + (marginpercentage / (100 - marginpercentage));
                if (markpercent == 100) {
                    var markpercent = (1 + (99 / (100 - 99))) + (1 + (1 / (100 - 1)));
                }
                if (markpercent < 0) Math.abs(markpercent) + 100;
                $scope.ItemDetails.sellingprice = (baseprice * markpercent).round(2);

                //var basemarginpercentage = marginpercentage / 100;
                //$scope.ItemDetails.sellingprice = (baseprice + (baseprice / (1 - basemarginpercentage) * basemarginpercentage)).round(2);

            } else {
                $scope.ItemDetails.sellingprice = baseprice + (baseprice * marginpercentage / 100).round(2);
            }

            if ($scope.SupportMarginexcludetax) {
                if ($scope.GetNumber($scope.ItemDetails.salediscountpercentage) > 0) {
                    $scope.ItemDetails.sellingprice = ($scope.GetNumber($scope.ItemDetails.sellingprice - ($scope.GetNumber($scope.ItemDetails.sellingprice) * $scope.GetNumber($scope.ItemDetails.salediscountpercentage) / 100))).round(2);
                    var taxitem = $scope.CurrentPurchaseTaxConfig;
                    if (taxitem != null && typeof (taxitem.taxsplit.findTax)) {
                        taxitem.taxsplit.findTax = $scope.findTax;
                    }
                    var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.sellingprice));
                    $scope.ItemDetails.salestaxpercentage = (taxconfig.cgst + taxconfig.sgst);
                    var newtaxvalue = $scope.GetNumber($scope.ItemDetails.sellingprice) * ($scope.ItemDetails.salestaxpercentage) / 100;
                    $scope.ItemDetails.sellingprice = (newtaxvalue + $scope.ItemDetails.sellingprice);
                    $scope.ItemDetails.salediscount = ($scope.ItemDetails.sellingprice / (100 - $scope.GetNumber($scope.ItemDetails.salediscountpercentage)) * $scope.GetNumber($scope.ItemDetails.salediscountpercentage));
                    $scope.ItemDetails.finalprice = $scope.GetNumber($scope.ItemDetails.sellingprice);
                    $scope.ItemDetails.sellingprice = $scope.GetNumber($scope.ItemDetails.salediscount) + $scope.ItemDetails.finalprice;
                    $scope.ItemDetails.sellingprice = $scope.RoundPrice($scope.ItemDetails.sellingprice);
                    $scope.ItemDetails.salediscount = $scope.ItemDetails.salediscount.round(0);
                    $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                }
                else {
                    var newtaxvalue = $scope.GetNumber($scope.ItemDetails.sellingprice) * $scope.ItemDetails.salestaxpercentage / 100;
                    var newsellingprice = $scope.RoundPrice(newtaxvalue + $scope.ItemDetails.sellingprice);
                    var taxitem = $scope.CurrentPurchaseTaxConfig;
                    if (taxitem != null) {
                        if (taxitem != null && typeof (taxitem.taxsplit.findTax)) {
                            taxitem.taxsplit.findTax = $scope.findTax;
                        }
                        var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.sellingprice));
                        if ($scope.ItemDetails.salestaxpercentage < taxconfig.cgst + taxconfig.sgst) {
                            var newtaxvalue = $scope.GetNumber($scope.ItemDetails.sellingprice) * (taxconfig.cgst + taxconfig.sgst) / 100;
                            newsellingprice = $scope.RoundPrice(newtaxvalue + $scope.ItemDetails.sellingprice);
                        }
                    }
                    $scope.ItemDetails.sellingprice = newsellingprice;
                }
            } else {
                $scope.ItemDetails.sellingprice = $scope.RoundPrice($scope.ItemDetails.sellingprice);
            }

            //$scope.ItemDetails.mrp = $scope.ItemDetails.sellingprice;
            if (typeof (excludediscountchange) == 'undefined' || !excludediscountchange) {
                if ($scope.SupportDiscountOnCost) {
                    $scope.ItemDetails.tsellingprice = $scope.ItemDetails.sellingprice;
                    $scope.ItemDetails.tmarginpercentage = $scope.ItemDetails.marginpercentage;
                }
                if (!fromsalediscountchange && !$scope.SupportMarginexcludetax)
                    $scope.salediscountChanged(true);
            }
            $scope.checkMRP();
            $scope.ItemDetails.rowexpensevalue = baseprice - $scope.ItemDetails.buyingprice;
            $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
            $scope.RoundingFinalPrice(fromsalediscountchange);
        };


        $scope.salediscountChanged = function (excludediscountchange, allowsellingpricechange) {

            if (typeof (allowsellingpricechange) == 'undefined')
                allowsellingpricechange = true;

            if ((typeof (excludediscountchange) == 'undefined' || !excludediscountchange) &&
                $scope.SupportDiscountOnCost &&
                $scope.GetNumber($scope.ItemDetails.salediscountpercentage) > 0) {

                if (typeof ($scope.ItemDetails.tsellingprice) != 'undefined' && allowsellingpricechange) {
                    $scope.ItemDetails.sellingprice = $scope.ItemDetails.tsellingprice;
                    $scope.ItemDetails.marginpercentage = $scope.ItemDetails.tmarginpercentage;
                } else {
                    $scope.ItemDetails.tsellingprice = $scope.ItemDetails.sellingprice;
                    $scope.ItemDetails.tmarginpercentage = $scope.ItemDetails.marginpercentage;
                }

                //$scope.ItemDetails.mrp = $scope.ItemDetails.sellingprice;
                if ($scope.SupportMarginexcludetax) {
                    if (allowsellingpricechange) {
                        $scope.marginChanged(true, true);
                        $scope.ComputeExcludeMargin();
                    }
                    else {
                        $scope.ComputeExcludeMargin();
                    }
                } else {
                    if (allowsellingpricechange) {
                        $scope.marginChanged(true, true);
                        $scope.ItemDetails.sellingprice = ($scope.GetNumber($scope.ItemDetails.sellingprice) / (100 - $scope.GetNumber($scope.ItemDetails.salediscountpercentage)) * 100);
                        $scope.ItemDetails.sellingprice = $scope.RoundPrice($scope.ItemDetails.sellingprice);
                    }
                    $scope.ComputeIncludeMargin();
                }
            }
            else if ($scope.SupportMarginexcludetax) {
                if (allowsellingpricechange)
                    $scope.marginChanged(true, true);
                if ($scope.SupportMarginexcludetax) {
                    $scope.ComputeExcludeMargin();
                }
                else {
                    $scope.ComputeIncludeMargin();
                }
            }

            $scope.ItemDetails.salediscount = ($scope.GetNumber($scope.ItemDetails.sellingprice) *
                $scope.GetNumber($scope.ItemDetails.salediscountpercentage) / 100).round(0);

            if ($scope.Invoice.IsUnregisteredInvoice)
                $scope.ItemDetails.purchasetax = 0;
            else
                $scope.ItemDetails.purchasetax = ($scope.GetNumber($scope.ItemDetails.buyingqty) * $scope.ItemDetails.buyingprice * $scope.ItemDetails.purchasetaxpercentage / 100).round(2);
            $scope.calculatesalestax();
            $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
            $scope.RoundingFinalPrice(true);
        };

        $scope.calculatesalestax = function () {
            var taxitem = $scope.CurrentSalesTaxConfig;
            if (taxitem == null) {
                $scope.ItemDetails.salestaxpercentage = 0;
            } else {
                var taxconfig = taxitem.taxsplit.findSalesTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }
            $scope.ItemDetails.salestax = ($scope.GetNumber($scope.ItemDetails.buyingqty) * ($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount) / ($scope.ItemDetails.salestaxpercentage + 100) * $scope.ItemDetails.salestaxpercentage).round(2);
        };



        $scope.CalculateTotal = function () {
            $scope.Total = {
                "buyingqty": 0,
                "buyingpiece": 0,
                "discount": 0,
                "purchasediscount": 0,
                "mrp": 0,
                "sellingprice": 0,
                "buyingprice": 0,
                "tax": 0,
                "amount": 0,
                "value": 0
            };

            if (typeof ($scope.ProductList) == 'undefined')
                return;

            $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {

                var productitem = $scope.AllProductList.find(function (item) {
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
                    $scope.Total.sellingprice += (v.sellingprice * v.buyingqty);
                    $scope.Total.buyingqty += v.buyingqty;
                    if (typeof (productitem) != 'undefined' && productitem != null)
                        $scope.Total.buyingpiece += (productitem.sellingmode != 3 ? v.buyingqty : 1);
                    $scope.Total.tax += v.purchasetax;
                    $scope.Total.discount += v.purchasetotaldiscount;
                    $scope.Total.amount += v.amount;

                    //$scope.Total.value = $scope.Total.amount
                    //    + $scope.Total.tax
                    //    + $scope.Invoice.otheramount
                    //    + $scope.Invoice.additionalamount
                    //    + $scope.Invoice.additionaltax;
                }
            });

            $scope.Total.tax = $scope.Total.tax.round(2);
            $scope.Total.amount = $scope.Total.amount.round(2);
            $scope.Total.discount = $scope.Total.discount.round(2);
            $scope.Total.value = $scope.Total.amount - ($scope.Invoice.isinclusivediscount ? $scope.Total.discount : 0) + $scope.Total.tax +
                $scope.Invoice.otheramount + $scope.Invoice.additionalamount +
                $scope.Invoice.additionaltax;
            $scope.Total.mrp = $scope.Total.mrp.round(2);
            $scope.Total.buyingprice = $scope.Total.buyingprice.round(2);
            $scope.Total.sellingprice = $scope.Total.sellingprice.round(2);
            $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);

            $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
        };

        $scope.InvoiceList = [];
        $scope.ShowInvoiceList = false;
        //if ($scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
        //    $scope.ShowInvoiceList = true;
        //    $scope.InvoiceList.push($scope.InvoiceInfo.invoiceid);
        //}


        $scope.InitializeInvoiceList = function (loaddirect) {

            request = {
                "Item": {
                    id: $scope.InvoiceInfo.invoiceid
                },
                "SecurityToken": "String content"
            }

            if (!loaddirect && typeof ($scope.InvoiceInfo.invoiceid) != 'undefined' && $scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
                request.Item.ids = $scope.InvoiceInfo.invoiceid;
                request.Item.id = 0;
            }

            if (typeof (request.Item.ids) != 'undefined')
                ShowInvoiceList = true;
            else
                $scope.ShowSearch = false;

            $http({
                method: 'POST',
                url: "LrinvoiceService/Search",
                data: request
            }).success(function (result) {
                $scope.PurchaseOrderItems = [];

                if (typeof (request.Item.ids) != 'undefined' || ((angular.isArray(result) && result.length > 1))) {
                    $scope.ShowSearch = true;
                    $timeout(function () {
                        $scope.SearchResult = result;
                        $scope.InvoiceEntryConfig();
                    }, 50);
                } else {
                    $scope.DisplayInvoice(result.Items[0]);
                    $scope.Invoice.addtionalcostpercentage = result.addtionalcostpercentage;
                    $scope.Invoice.additionalamount = result.additionalamount;
                    $scope.Invoice.additionaltax = result.additionaltax;
                    $scope.Invoice.premargin = result.premargin;
                    $scope.Invoice.internalbuyer = result.internalbuyer;
                    $scope.Invoice.lrid = result.lrid;
                    $scope.Invoice.lrno = result.lrno;
                    $scope.Invoice.frightcharge = result.frightcharge;
                    $scope.Invoice.loadingcharge = result.loadingcharge;
                    $scope.Invoice.lrentryno = result.lrentryno;
                    $scope.Invoice.invoiceno = result.Items[0].invoiceno;
                    $scope.Invoice.status = result.status;
                    $scope.Invoice.supplierid = result.supplierid;
                    $scope.Invoice.suppliercompanyname = result.suppliercompanyname;
                    $scope.Invoice.suppliergstno = result.suppliergstno;
                    $scope.Invoice.companyname = result.companyname;
                    $scope.Invoice.retaillocationid = result.retaillocationid;
                    $scope.CollectLocationInfo = $scope.Invoice.retaillocationid == -2 || $scope.SupportLocationBased;

                    $scope.Invoice.lrnoInfo = result.lrentryno + " / " + result.lrno;
                    $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
                    if ($scope.Invoice.Lrinvoiceitems.length > 0) {
                        $scope.ItemDetails.sno = $scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].sno + 1;
                    }
                    if (result.modeofdelivery == 1 && result.statusid < 202) {
                        $rootScope.ShowAlert('InBox', "Lorry delivery can't add products until contains issue & receive.");
                    }
                    $scope.Navigation.SetMode('EDIT');

                    $scope.ShowSearch = false;
                    $scope.CalculateTotal();

                }
            }).error(function (result) { });


        };

        if (typeof ($scope.InvoiceInfo.invoiceid) == 'undefined') {
            $scope.SwitchPanel();
        } else
            if ($scope.InvoiceInfo.invoiceid > 0 || $scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
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
                "Item": {
                    brandid: $scope.ItemDetails.Details.brandid,
                    typeid: $scope.ItemDetails.Details.typeid,
                    productid: $scope.ProductSearch.ProductID
                },
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
            }).error(function (result) { });
        };

        $scope.ActualSave = function (movenext) {
            if ($scope.SaveRequest == true) return;

            $scope.SaveRequest = true;

            var cleaninvoice = (JSON.parse(JSON.stringify($scope.Invoice)));
            try {
                var removeList = [];
                $.each(cleaninvoice.Lrinvoiceitems, function (k, v) {
                    if (v.IsParent) {
                        removeList.push(v);
                    } else if (typeof (v.poinfo) != 'undefined' &&
                        v.poinfo != null && v.poinfo.length > 0) {
                        var available = v.buyingqty;
                        $.each(v.poinfo, function (kk, vv) {

                            if (vv.aqty > available) {
                                vv.aqty = available;
                                available = 0;
                            } else {
                                available -= vv.aqty;
                            }

                        });
                    }
                });

                $.each(removeList, function (k, v) {
                    cleaninvoice.Lrinvoiceitems.splice(cleaninvoice.Lrinvoiceitems.indexOf(v), 1);
                });
            } catch (exp) {
                $scope.SaveRequest = false;
                alert(exp);
                return;
            }
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
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    //var invoiceids = result.id;
                    if (result.errormessage != null && result.errormessage != '') {
                        $rootScope.Toaster.Error(result.errormessage);
                        return;
                    }

                    if (result.id > 0 &&
                        $scope.Invoice.id > 0 &&
                        result.id != $scope.Invoice.id) {
                        $location.url('/barcode?invoiceid=' + result.id);
                        $rootScope.Toaster.Info("InvoiceItems Saved Successfully.!");
                        return;
                    }

                    if (movenext) {
                        $location.url('/barcode?invoiceid=' + result.id);
                        $rootScope.Toaster.Info("InvoiceItems Saved Successfully.!");
                    } else {


                        var oldinvoice = (JSON.parse(JSON.stringify($scope.Invoice)));
                        var total = (JSON.parse(JSON.stringify($scope.Total)));
                        $rootScope.Toaster.Info("InvoiceItems Saved Successfully.!");
                        $scope.DisplayInvoice(result);
                        $scope.InvoiceItem = (JSON.parse(JSON.stringify($scope.ItemTemplate)));

                        $scope.Invoice.addtionalcostpercentage = oldinvoice.addtionalcostpercentage;
                        $scope.Invoice.additionalamount = oldinvoice.additionalamount;
                        $scope.Invoice.additionaltax = oldinvoice.additionaltax;
                        $scope.Invoice.lrid = oldinvoice.lrid;
                        $scope.Invoice.lrno = oldinvoice.lrno;
                        $scope.Invoice.lrentryno = oldinvoice.lrentryno;
                        $scope.Invoice.invoiceno = oldinvoice.invoiceno;
                        $scope.Invoice.status = oldinvoice.status;
                        $scope.Invoice.supplierid = oldinvoice.supplierid;
                        $scope.Invoice.suppliercompanyname = oldinvoice.suppliercompanyname;
                        $scope.Invoice.companyname = oldinvoice.companyname;
                        $scope.Invoice.lrnoInfo = oldinvoice.lrnoInfo;
                        $scope.Total.value = total.value;
                    }
                } else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }

        $scope.CheckTransferBeforeSave = function (movenext) {
            if ($scope.Invoice.statusid == 2 && $scope.Invoice.internalbuyer &&
                $scope.Invoice.buyerinvoiceid == 0) {
                $scope.ChooseCompany(function (value) {
                    $scope.Invoice.transfercompanyid = value.companyid;
                    $scope.Invoice.retaillocationid = value.retaillocationid;
                    $scope.Invoice.isvendorbill = true;
                    $scope.ActualSave(movenext);
                });
            } else {
                $scope.ActualSave(movenext);
            }
        }

        $scope.Save = function (movenext) {
            if ($scope.system_supportpricingflow &&
                !$scope.system_supportpricingflowproducts &&
                $scope.InvoiceTotal) {
                $scope.CalculateTotal();
                if ($scope.InvoiceTotal.buyingqty != $scope.Total.buyingqty) {
                    $rootScope.Toaster.Error('Total Qty should be ' + $scope.InvoiceTotal.buyingqty);
                    return;
                }
                if ($scope.GetNumber($scope.InvoiceTotal.taxable).round(0) != $scope.GetNumber($scope.Total.amount).round(0)) {
                    var difference = $scope.GetNumber($scope.InvoiceTotal.taxable).round(0) - $scope.GetNumber($scope.Total.amount).round(0);

                    if (Math.abs(difference) > 2) {
                        $rootScope.Toaster.Error('Taxable value should be ' + $scope.InvoiceTotal.taxable);
                        return;
                    }
                }

            }

            if ($scope.Invoice.totalamount != ($scope.Total.value)) {
                var diffvalue = Math.abs(Math.abs($scope.Invoice.totalamount) - Math.abs($scope.Total.value));
                if (diffvalue > 1) {
                    $rootScope.Confirm('Invoice Value Mismatch', diffvalue + ' amount different in invoice amount do you want to continue', function () {
                        $scope.CheckTransferBeforeSave(movenext);
                    });
                } else {
                    $scope.CheckTransferBeforeSave(movenext);
                }
            } else {
                $scope.CheckTransferBeforeSave(movenext);
            }
        }

        $scope.FilterProductByCompany = function () {

            if ($scope.AllProductList == null) {
                $timeout(function () {
                    $scope.FilterProductByCompany();
                }, 200);
            } else {
                var pl = [];
                $.each($scope.AllProductList, function (k, v) {
                    if (v.new || v.prodcompanyid == 0 || v.prodcompanyid == $scope.Invoice.companyid)
                        pl.push(v);
                });
                $scope.ProductList = pl;
                if ($scope.Invoice.supplierid == 'undefined' || $scope.Invoice.supplierid == null) return;
                restService.Get('SupplierService/FilterConfig/' + $scope.Invoice.supplierid, true).then(function (results) {

                    if (results.products != null && results.products.length > 0) {
                        var pl = [];
                        $.each($scope.AllProductList, function (k, v) {
                            var productid = results.products.find(function (item) {
                                return item == v.id;
                            });
                            if (v.new || (productid && productid != null)) {
                                pl.push(v);
                            }
                        });
                        $scope.ProductList = pl;
                    }

                    if (results.brands != null && results.brands.length > 0 &&
                        $scope.BrandNameList != null) {
                        var pl = [];
                        $.each($scope.BrandNameList, function (k, v) {
                            var brandconfig = results.brands.find(function (item) {
                                return item == v.id;
                            });
                            if (v.new || (brandconfig && brandconfig != null)) {
                                pl.push(v);
                            }
                        });
                        $scope.BrandName = pl;
                        $scope.BrandNameList = pl;
                    } else if ($scope.BrandNameList != null) {
                        var pl = [];
                        $.each($scope.BrandNameList, function (k, v) {
                            pl.push(v);
                        });
                        $scope.BrandName = pl;
                    }
                });
            }
        }

        $scope.DisplayInvoice = function (invoice) {
            $scope.Invoice = invoice;
            $scope.SearchExistingPO();

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
                                amt += ($scope.Invoice.isinclusivediscount ? iv.basebuyingprice : iv.buyingprice) * iv.buyingqty;
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
                        parent.amount = amt.round(2);
                        if (firstIndex < $scope.Invoice.Lrinvoiceitems.length) {
                            $scope.Invoice.Lrinvoiceitems.splice(firstIndex + 1, 0, $scope.SetRowStyle(parent));
                        } else {
                            $scope.Invoice.Lrinvoiceitems.push($scope.SetRowStyle(parent));
                        }

                    });


                    $scope.Invoice.Lrinvoiceitems.sort(function (first, next) {
                        return first.sno == next.sno;
                    });
                }

                $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                    if ($scope.system_customfields) {
                        if (v.CustomFields == null)
                            v.CustomFields = [];

                        $.each($scope.ItemTemplate.CustomFields, function (cf, cv) {
                            var cf = v.CustomFields.find(function (item) {
                                return item.key == cv.key;
                            });
                            if (cf == undefined || cf == null) {
                                v.CustomFields.push({ 'key': cv.key, 'value': null });
                            }
                        });
                    }
                    $scope.SetRowStyle(v);
                    if ($scope.system_marginpercentagezero && v.marginpercentage < 1) {
                        if ((v.sellingprice - (v.buyingprice + (v.purchasetax / v.buyingqty))) > (v.sellingprice * 3 / 100)) {
                            v.marginpercentage = (($scope.GetNumber(v.sellingprice) / v.buyingprice * 100) - 100).round(2);
                            if ($scope.SupportMarkDOWN)
                                v.marginpercentage = (100 - ((v.buyingprice / v.sellingprice) * 100)).round(2);
                        }
                        else {
                            v.sellingprice = 0;
                            v.mrp = 0;
                            v.finalprice = 0;
                            v.salediscount = 0;
                            v.salediscountpercentage = 0;
                        }
                    }
                    if (typeof (v.amount) == 'undefined') {
                        v.amount = (($scope.Invoice.isinclusivediscount ? $scope.GetNumber(v.basebuyingprice) : $scope.GetNumber(v.buyingprice)) *
                            $scope.GetNumber(v.buyingqty)).round(2);
                    }
                });
            }


            $scope.FilterProductByCompany();
            $scope.EntryGridConfig();
            $scope.CalculateTotal();
            $scope.additionaldiscountpercentage = 0;
            var billValue = 0;
            var additionaldiscount = 0;
            var totaltax = 0;
            $.each($scope.Invoice.Lrinvoicetaxitems, function (k, v) {
                if (v.type == 0)
                    billValue += (v.amounton);
                totaltax += v.taxamount;
            });

            if ($scope.system_supportpricingflow) {
                $scope.InvoiceTotal = {
                    'taxable': $scope.Total.amount,
                    'buyingpiece': $scope.Total.buyingpiece,
                    'buyingqty': $scope.Total.buyingqty,
                    'tax': $scope.Total.tax,
                    'discount': $scope.Total.discount
                };
            }
            $scope.Invoice.IsUnregisteredInvoice = totaltax == 0;
            $scope.purchasediscounttaxitem = null;
            var noofdiscounts = 0;
            $.each($scope.Invoice.Lrinvoicetaxitems, function (k, v) {
                if ((v.type == 2 || v.type == 3) || (v.type == 0 && v.ptaxid == 0)) {
                    //$scope.additionaldiscountpercentage += v.discountpercentage;
                    additionaldiscount += v.discount;
                } else if (v.type == 0) {
                    noofdiscounts++;
                }
            });

            if (noofdiscounts == 1) {
                $scope.purchasediscounttaxitem = $scope.Invoice.Lrinvoicetaxitems.find(function (item) {
                    return item.type == 0;
                });
            }

            $scope.additionaldiscountpercentage = parseFloat((additionaldiscount / billValue * 100).toFixed(4));
            if (isNaN($scope.additionaldiscountpercentage))
                $scope.additionaldiscountpercentage = 0;

        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $scope.Invoice.Lrinvoiceitems.splice($scope.Invoice.Lrinvoiceitems.indexOf(item), 1);
                if ($scope.Invoice.Lrinvoiceitems.length == 0) {
                    $scope.ItemDetails.sno = 1;
                    $scope.Total.mrp = 0;
                    $scope.Total.buyingprice = 0;
                    $scope.Total.sellingprice = 0;
                    $scope.Total.discount = 0;
                    $scope.Total.buyingqty = 0;
                    $scope.Total.tax = 0;
                    $scope.Total.amount = 0;
                    $scope.Total.value = 0;
                    return;
                }

                if (typeof (item.IsParent) == 'undefined' || !item.IsParent) {
                    $scope.Total.mrp -= (item.mrp * item.buyingqty);
                    $scope.Total.buyingprice -= (item.buyingprice * item.buyingqty);
                    $scope.Total.sellingprice -= (item.sellingprice * item.buyingqty);
                    $scope.Total.tax -= item.purchasetax;
                    $scope.Total.discount -= item.purchasetotaldiscount;
                    $scope.Total.amount -= item.amount;
                    $scope.Total.value = $scope.Total.amount - ($scope.Invoice.isinclusivediscount ? $scope.Total.discount : 0) + $scope.Total.tax + $scope.Invoice.otheramount +
                        $scope.Invoice.additionalamount + $scope.Invoice.additionaltax;
                }



                if (typeof (item.IsParent) != 'undefined' && item.IsParent && item.Child.length > 0) {
                    item.UAN = '';
                    item.uan = '';
                    item.serialno = '';
                    $.each(item.Child, function (k, v) {
                        $scope.Total.mrp -= (v.mrp * v.buyingqty);
                        $scope.Total.buyingprice -= (v.buyingprice * v.buyingqty);
                        $scope.Total.sellingprice -= (v.sellingprice * v.buyingqty);
                        $scope.Total.buyingqty -= v.buyingqty;
                        $scope.Total.tax -= v.purchasetax;
                        $scope.Total.discount -= v.purchasetotaldiscount;
                        $scope.Total.amount -= v.amount;
                        $scope.Total.value = $scope.Total.amount - ($scope.Invoice.isinclusivediscount ? $scope.Total.discount : 0) + $scope.Total.tax +
                            $scope.Invoice.otheramount + $scope.Invoice.additionalamount + $scope.Invoice.additionaltax;
                        $scope.Invoice.Lrinvoiceitems.splice($scope.Invoice.Lrinvoiceitems.indexOf(v), 1);
                    })
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
                } else {
                    $scope.ItemDetails.sno = $scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].sno;
                    $scope.Total.tax = $scope.Total.tax.round(2);
                    $scope.Total.amount = $scope.Total.amount.round(2);
                    $scope.Total.value = $scope.Total.amount - ($scope.Invoice.isinclusivediscount ? $scope.Total.discount : 0) + $scope.Total.tax + $scope.Invoice.otheramount + $scope.Invoice.additionalamount + $scope.Invoice.additionaltax;
                    $scope.Total.mrp = $scope.Total.mrp.round(2);
                    $scope.Total.buyingprice = $scope.Total.buyingprice.round(2);
                    $scope.Total.discount = $scope.Total.discount.round(2);
                    $scope.Total.sellingprice = $scope.Total.sellingprice.round(2);
                    $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);
                }
            } else if (action == "View") {
                $scope.CreateMultiItems(item, true, true);
            } else if (action == "Edit") {

                item.hidepricing = false;
                if ($scope.system_marginpercentagezero && item.marginpercentage < 1 && $scope.system_hidecostinventory) {
                    item.hidepricing = true;
                }

                if (typeof ($scope.ItemDetails.IsEditItem) != 'undefined' &&
                    $scope.ItemDetails.IsEditItem) {
                    if ($scope.system_hidecostinventory) {
                        $rootScope.Toaster.Error('Please save currently editing item before editing another item');
                    }
                    else {
                        $rootScope.Confirm('Editing', 'you will loose the current editing item', function () {
                            $scope.ItemDetails.IsEditItem = false;
                            $scope.$apply(function () {
                                $scope.EditItem(item);
                                $scope.ItemDetails.barcodecount = false;
                                $scope.ItemDetails.IsEditItem = true;
                                $scope.ItemDetails.barcodecount = 0;
                                if ($scope.ItemDetails.id && $scope.ItemDetails.id > 0) {
                                    $scope.ItemDetails.OLD = {
                                        'productid': $scope.ItemDetails.productid,
                                        'sellingprice': $scope.ItemDetails.sellingprice,
                                        'buyingprice': $scope.ItemDetails.buyingprice,
                                        'salediscountpercentage': $scope.ItemDetails.salediscountpercentage
                                    }
                                }
                                if (typeof (item.barcodes) != 'undefined' && item.barcodes != 'null' &&
                                    item.barcodes != null) {
                                    $scope.ItemDetails.barcodecount = JSON.parse(item.barcodes).length;
                                }
                            });
                        });
                    }

                } else {
                    $scope.EditItem(item);
                    $scope.ItemDetails.IsEditItem = true;
                    //$scope.SearchSKUDesign(false);
                    if ($scope.ItemDetails.id && $scope.ItemDetails.id > 0) {
                        $scope.ItemDetails.OLD = {
                            'productid': $scope.ItemDetails.productid,
                            'sellingprice': $scope.ItemDetails.sellingprice,
                            'buyingprice': $scope.ItemDetails.buyingprice,
                            'salediscountpercentage': $scope.ItemDetails.salediscountpercentage
                        }
                    }
                }

            } else if (action == "EditInvoice") {
                $scope.InvoiceInfo.invoiceid = item.id;
                $scope.InitializeInvoiceList(true);
            }
        }

        $scope.EditItem = function (item) {
            var productitem = $scope.AllProductList.find(function (prd) {
                return prd.id == item.productid;
            });

            if (typeof (item.IsParent) == 'undefined' || !item.IsParent) {
                $scope.Total.mrp -= (item.mrp * item.buyingqty);
                $scope.Total.buyingprice -= (item.buyingprice * item.buyingqty);
                $scope.Total.sellingprice -= (item.sellingprice * item.buyingqty);
                $scope.Total.buyingqty -= item.buyingqty;
                $scope.Total.buyingpiece -= (typeof (productitem) == 'undefined' || productitem.sellingmode != 3 ? item.buyingqty : 1);
                $scope.Total.tax -= item.purchasetax;
                $scope.Total.discount -= item.purchasetotaldiscount;
                $scope.Total.amount -= item.amount;
                $scope.Total.value = $scope.Total.amount - ($scope.Invoice.isinclusivediscount ? $scope.Total.discount : 0) + $scope.Total.tax +
                    $scope.Invoice.otheramount + $scope.Invoice.additionalamount + $scope.Invoice.additionaltax;
            }

            $scope.Invoice.Lrinvoiceitems.splice($scope.Invoice.Lrinvoiceitems.indexOf(item), 1);

            if (typeof (item.IsParent) != 'undefined' && item.IsParent && item.Child.length > 0) {
                item.UAN = '';
                item.uan = '';
                item.Details.serialno = '';
                $.each(item.Child, function (k, v) {
                    $scope.Total.mrp -= (v.mrp * v.buyingqty);
                    $scope.Total.buyingprice -= (v.buyingprice * v.buyingqty);
                    $scope.Total.sellingprice -= (v.sellingprice * v.buyingqty);
                    $scope.Total.buyingqty -= v.buyingqty;
                    $scope.Total.buyingpiece -= (typeof (productitem) == 'undefined' || productitem.sellingmode != 3 ? v.buyingqty : 1);
                    $scope.Total.tax -= v.purchasetax;
                    $scope.Total.discount -= v.purchasetotaldiscount;
                    $scope.Total.amount -= v.amount;
                    $scope.Invoice.Lrinvoiceitems.splice($scope.Invoice.Lrinvoiceitems.indexOf(v), 1);
                })
            }

            $scope.Total.discount = $scope.Total.discount.round(2);
            $scope.Total.tax = $scope.Total.tax.round(2);
            $scope.Total.amount = $scope.Total.amount.round(2);
            $scope.Total.value = $scope.Total.amount - ($scope.Invoice.isinclusivediscount ? $scope.Total.discount : 0) + $scope.Total.tax + $scope.Invoice.otheramount + $scope.Invoice.additionalamount + $scope.Invoice.additionaltax;
            $scope.Total.mrp = $scope.Total.mrp.round(2);
            $scope.Total.buyingprice = $scope.Total.buyingprice.round(2);
            $scope.Total.sellingprice = $scope.Total.sellingprice.round(2);
            $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);
            $scope.ProductSearch.ProductID = item.productid;
            $scope.ItemDetails = item;
            $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
            $scope.taxChange(false);

            if ($scope.SupportDealerPriceWOTax &&
                typeof ($scope.ItemDetails.dealerprice) != 'undefined') {
                var salestaxpercentage = 0;
                var taxitem = $scope.CurrentSalesTaxConfig;
                if (taxitem != null) {
                    var dprice = $scope.GetNumber($scope.ItemDetails.dealerprice);
                    var taxconfig = taxitem.taxsplit.findSalesTax($scope.GetNumber($scope.ItemDetails.dealerprice));
                    salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
                    $scope.ItemDetails.dealerprice = (dprice / (salestaxpercentage + 100) * 100).round(2);
                }
            }

            if ($scope.SupportDesignList) {
                $scope.SearchDesignList(item.designid);
            }
            //$scope.SearchPO();
        }

        $scope.checkMRP = function () {
            if (!$scope.SupportSalepriceMrpdifference)
                $scope.ItemDetails.mrp = $scope.ItemDetails.sellingprice;
        }

        $scope.CheckDealerPrice = function () {

            if (typeof ($scope.ItemDetails.dealerprice) != 'undefined' &&
                $scope.ItemDetails.dealerprice != null &&
                ($scope.ItemDetails.dealerprice.toString().indexOf('%') >= 0 || $scope.ItemDetails.dealerprice.toString().indexOf('*') >= 0)) {

                var marginpercentage = $scope.GetNumber($scope.ItemDetails.dealerprice.toString().replace('%', '').replace('*', ''));
                if (isNaN(marginpercentage))
                    marginpercentage = 0;

                var baseprice = 0;
                if ($scope.SupportMarginOnCost) {
                    baseprice = $scope.GetNumber($scope.ItemDetails.basebuyingprice);
                } else {
                    baseprice = $scope.ItemDetails.buyingprice;
                }

                if ($scope.SupportMarginincludetax) {
                    var newtaxvalue = baseprice * ($scope.ItemDetails.purchasetaxpercentage) / 100;
                    baseprice = newtaxvalue + baseprice;
                }

                if ($scope.SupportAddlrexptomargion) {

                    if ($scope.Invoice.expenseperpiece > 0)
                        baseprice = baseprice + $scope.Invoice.expenseperpiece;

                    if ($scope.Invoice.expensepercentage > 0)
                        baseprice = baseprice + (baseprice * $scope.Invoice.expensepercentage / 100).round(0);
                }

                if (typeof ($scope.Invoice.premargin) != 'undefined' && $scope.Invoice.premargin > 0) {
                    baseprice = baseprice + (baseprice * $scope.Invoice.premargin / 100).round(0);
                }

                if (typeof ($scope.ItemDetails.rowexpense) != 'undefined' && $scope.ItemDetails.rowexpense > 0) {
                    baseprice = baseprice + (baseprice * $scope.ItemDetails.rowexpense / 100).round(0);
                }

                if ($scope.SupportMarkDOWN) {
                    var markpercent = 1 + (marginpercentage / (100 - marginpercentage));
                    $scope.ItemDetails.dealerprice = (baseprice * markpercent).round($scope.System_b2brounding);
                } else {
                    $scope.ItemDetails.dealerprice = baseprice + (baseprice * marginpercentage / 100).round($scope.System_b2brounding);
                }

                if ($scope.SupportMarginexcludetax) {
                    var newtaxvalue = $scope.GetNumber($scope.ItemDetails.dealerprice) * $scope.ItemDetails.salestaxpercentage / 100;
                    var newdealerprice = (newtaxvalue + $scope.ItemDetails.dealerprice).round($scope.System_b2brounding);
                    var taxitem = $scope.CurrentSalesTaxConfig;
                    if (taxitem != null) {
                        var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.dealerprice));
                        if ($scope.ItemDetails.salestaxpercentage < taxconfig.cgst + taxconfig.sgst) {
                            var newtaxvalue = $scope.GetNumber($scope.ItemDetails.dealerprice) * (taxconfig.cgst + taxconfig.sgst) / 100;
                            newdealerprice = (newtaxvalue + $scope.ItemDetails.dealerprice).round($scope.System_b2brounding);
                        }
                    }
                    $scope.ItemDetails.dealerprice = newdealerprice;
                } else {
                    $scope.ItemDetails.dealerprice = $scope.ItemDetails.dealerprice.round($scope.System_b2brounding);
                }
            }

            $scope.setFocussellingprice();
        }

        $scope.mrpEnter = function () {
            if (typeof ($scope.ItemDetails.mrp) != 'undefined' &&
                $scope.ItemDetails.mrp != null &&
                ($scope.ItemDetails.mrp.toString().indexOf('%') >= 0 || $scope.ItemDetails.mrp.toString().indexOf('*') >= 0)) {
                var mrp = $scope.GetNumber($scope.ItemDetails.mrp.toString().replace('%', '').replace('*', ''));
                if (isNaN(mrp))
                    mrp = 0;
                $scope.ItemDetails.mrp = $scope.GetNumber($scope.ItemDetails.sellingprice) +
                    ($scope.GetNumber($scope.ItemDetails.sellingprice) * mrp / 100).round(0);
            }
            if (typeof ($scope.ItemDetails.mrp) != 'undefined' &&
                $scope.ItemDetails.mrp != null &&
                ($scope.ItemDetails.mrp.toString().indexOf('$') >= 0)) {
                var mrp = $scope.GetNumber($scope.ItemDetails.mrp.toString().replace('$', '').replace('*', ''));
                if (isNaN(mrp))
                    mrp = 0;
                var markpercent = 1 + (mrp / (100 - mrp));
                if (markpercent == 100) {
                    var markpercent = (1 + (99 / (100 - 99))) + (1 + (1 / (100 - 1)));
                }
                if (markpercent < 0) Math.abs(markpercent) + 100;
                $scope.ItemDetails.mrp = ($scope.GetNumber($scope.ItemDetails.sellingprice) * markpercent).round(0);
            }
            $scope.setFocussalediscountpercentage();
        }

        $scope.mrpChange = function () {
            if (!$scope.SupportSalepriceMrpdifference) {
                $scope.ItemDetails.sellingprice = $scope.ItemDetails.mrp;
                $scope.sellingpriceChange();
                $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
            }
        }

        $scope.ComputeIncludeMargin = function () {

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


        $scope.ComputeExcludeMargin = function () {
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
            $scope.ItemDetails.rowexpensevalue = buyingprice - $scope.ItemDetails.buyingprice;
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
                $scope.ComputeExcludeMargin();
            } else {
                $scope.ComputeIncludeMargin();
            }

            var taxitem = $scope.CurrentSalesTaxConfig;

            if (taxitem == null) {
                $scope.ItemDetails.salestaxpercentage = 0;
            } else {
                var taxconfig = taxitem.taxsplit.findSalesTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }
            $scope.salediscountChanged(undefined, false);
            $scope.RoundingFinalPrice(false);
        }

        $scope.RoundingFinalPrice = function (fromsalediscountchange,recompute) {
            if ($scope.SupportRoundingDiscount) {
                var nfinalprice = $scope.RoundPrice($scope.ItemDetails.finalprice);
                if ($scope.ItemDetails.finalprice != nfinalprice || recompute) {
                    if ($scope.ItemDetails.finalprice < $scope.ItemDetails.sellingprice) {
                        $scope.ItemDetails.finalprice = nfinalprice;
                        $scope.ItemDetails.salediscount = $scope.ItemDetails.sellingprice - $scope.ItemDetails.finalprice;
                        if (!fromsalediscountchange)
                            $scope.ItemDetails.salediscountpercentage = ($scope.ItemDetails.salediscount / $scope.ItemDetails.sellingprice * 100).round(2);
                    }
                    else {
                        $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice;
                        $scope.ItemDetails.salediscount = 0;
                        $scope.ItemDetails.salediscountpercentage = 0;
                    }
                    var salestaxitem = $scope.CurrentSalesTaxConfig;
                    if (salestaxitem == null) {
                        $scope.ItemDetails.salestaxpercentage = 0;
                        $scope.ItemDetails.salediscountpercentage = 0;
                    } else {
                        var taxconfig = salestaxitem.taxsplit.findSalesTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                        $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
                    }
                } 
            }
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
                if ((typeof ($scope.SearchCondition.lrid) == 'undefined' || $scope.SearchCondition.lrid == 0) || ((angular.isArray(result) && result.length > 1))) {
                    $scope.Navigation.SetMode('SEARCH');
                    $scope.ShowSearch = true;
                    $timeout(function () {
                        $scope.SearchResult = result;
                        $scope.InvoiceEntryConfig();
                    }, 50);
                }
                else if (typeof ($scope.SearchCondition.lrid) != 'undefined' && $scope.SearchCondition.lrid > 0
                    && result != null && result.Items != null && result.Items.length > 1) {
                    $scope.Navigation.SetMode('SEARCH');
                    $scope.ShowSearch = true;
                    $timeout(function () {
                        $scope.SearchResult = result.Items;
                        $scope.InvoiceEntryConfig();
                    }, 50);
                }
                else {
                    $scope.SearchResult = result.Items;
                    if ($scope.SearchResult.length == 1) {
                        $scope.ManageSearchItem('EditInvoice', $scope.SearchResult[0]);
                    }
                    $scope.SearchCondition.lrid = 0;
                }
            }).error(function (result) { });
        }

        $scope.InvoiceEntryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);
            //igrid.AddReferenceColumn('supplierid', 'Supplier', 200, $scope.SupplierList);

            igrid.AddSimpleColumn('suppliercompany', 'Supplier Company', 120);
            igrid.AddSimpleColumn('lrentryno', 'Entry No', 100);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 120);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('noofbundels', 'Bundles', 50);
            igrid.AddFormatedColumn('baseamount', 'Base Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('discount', 'Discount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax Amount', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalamount', 'TotalAmount', 125, 'N2', 2).total = true;
            igrid.AddSimpleColumn('createduser', 'Created By', 80);
            igrid.AddFormatedColumn('createdon', 'Created On', 100, 'dd-MM-yyyy hh:mm');
            igrid.AddSimpleColumn('modifieduser', 'Modified By', 80);
            igrid.AddFormatedColumn('modifiedon', 'Modified On', 100, 'dd-MM-yyyy hh:mm');

            var commands = "";
            if ($rootScope.UserInfo.p_LRPRODUCT.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;";
            if ($rootScope.UserInfo.p_LRPRODUCT.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>";

            if ($rootScope.UserInfo.p_LRPRODUCT.modify || $rootScope.UserInfo.p_LRPRODUCT.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 130, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('EditInvoice', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('DeleteInvoice', args.item);
                    });
                });

            }
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

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
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.ItemDetails.Details = data.value;
                        $scope.ItemDetails.Details.productid = $scope.ProductSearch.ProductID;
                        $scope.productChange();
                        $scope.findLastPrice();
                    }
                }
            });
        }

        $scope.findLastPrice = function (updatefocus) {

            if (updatefocus) {
                if ($scope.SupportDesignList) {
                    $scope.setFocusdesignidlst();
                }
                else {
                    $scope.setFocusdesignid();
                }
            }

            if (typeof ($scope.ItemDetails.glnprice) != 'undefined' &&
                $scope.ItemDetails.glnprice) return;

            var existing = $scope.Invoice.Lrinvoiceitems.find(function (item) {
                return item.productid == $scope.ItemDetails.productid &&
                    (typeof (item.id) == 'undefined' || item.id <= 0);
            });

            if (typeof (existing) != 'undefined' || existing != null) {

                if (existing.marginprice && existing.marginprice > 0) {
                    var cnfgSellingPrice = $scope.ItemDetails.buyingprice + existing.marginprice;
                    $scope.ItemDetails.marginprice = existing.marginprice;
                    $scope.ItemDetails.sellingprice = cnfgSellingPrice;
                    $scope.ItemDetails.mrp = $scope.ItemDetails.sellingprice;

                    if ($scope.SupportMarginOnCost) {
                        if ($scope.SupportMarginexcludetax) {
                            $scope.ComputeExcludeMargin();
                        } else {
                            $scope.ItemDetails.marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / $scope.GetNumber($scope.ItemDetails.basebuyingprice) * 100) - 100).round(2);
                            $scope.ItemDetails.marginpercentage = $scope.ItemDetails.marginpercentage > 0 ? $scope.ItemDetails.marginpercentage : null;
                        }
                    } else {
                        if ($scope.SupportMarginexcludetax) {
                            $scope.ComputeExcludeMargin();
                        } else {
                            $scope.ItemDetails.marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / $scope.GetNumber($scope.ItemDetails.buyingprice) * 100) - 100).round(2);
                            $scope.ItemDetails.marginpercentage = $scope.ItemDetails.marginpercentage > 0 ? $scope.ItemDetails.marginpercentage : null;
                        }
                    }
                } else {
                    $scope.ItemDetails.marginpercentage = existing.marginpercentage;
                    $scope.ItemDetails.salediscountpercentage = existing.salediscountpercentage;
                }
                $scope.TryIntelligentMargin(function () {
                    $scope.salediscountChanged();
                    if ($scope.ItemDetails.marginpercentage == existing.marginpercentage)
                        $scope.marginChanged();
                });
                return;
            }

            var request = {
                "item": JSON.parse(JSON.stringify($scope.ItemDetails.Details)),
                "securitytoken": "String content"
            }
            request.item.price = $scope.ItemDetails.basebuyingprice;
            request.item.supplierid = $scope.Invoice.supplierid;
            request.item.productid = $scope.ItemDetails.productid;

            var prditem = $scope.AllProductList.find(function (item) {
                return (item.id == request.item.productid);
            });

            if (typeof (prditem) != null && prditem != null)
                request.item.sectionid = prditem.salescounterid;

            request.item.suppliercompanyid = $scope.Invoice.suppliercompanyid;
            request.item.companyid = $scope.Invoice.companyid;
            request.item.fetchprices = true;
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PricecontrolService/Search",
                data: srequest
            }).success(function (result) {
                $scope.Price = null;
                $scope.LastPrice = null;
                if (result.lastprice != null) {
                    $scope.LastPrice = result.lastprice;
                }
                if (result.items != null && result.items.length > 0) {
                    $scope.Price = result.items[0];
                    $scope.FitSellingPrice();
                    $scope.salediscountChanged();
                    $scope.marginChanged();
                } else if (result.lastprice != null) {

                    $scope.ItemDetails.marginpercentage = $scope.LastPrice.marginpercentage > 0 ? $scope.LastPrice.marginpercentage : null;
                    $scope.ItemDetails.salediscountpercentage = $scope.LastPrice.salediscountpercentage > 0 ? $scope.LastPrice.salediscountpercentage : null;
                    try {
                        var taxitem = $scope.CurrentPurchaseTaxConfig;
                        if (taxitem == null && $scope.ItemDetails.ptaxid > 0) {
                            $scope.taxChange();
                            taxitem = $scope.CurrentPurchaseTaxConfig;
                        }
                        //if ($scope.ItemDetails.ptaxid == $scope.LastPrice.ptaxid)
                        $scope.TryIntelligentMargin(function () {
                            $scope.salediscountChanged();
                            $scope.marginChanged();
                        });
                    } catch (e) {
                        $scope.salediscountChanged();
                        $scope.marginChanged();
                    }

                } else if ($scope.Invoice.Lrinvoiceitems.length > 0) {
                    $scope.ItemDetails.marginpercentage = $scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].marginpercentage;
                    $scope.ItemDetails.salediscountpercentage = $scope.Invoice.Lrinvoiceitems[$scope.Invoice.Lrinvoiceitems.length - 1].salediscountpercentage;

                    $scope.ItemDetails.marginpercentage = $scope.ItemDetails.marginpercentage > 0 ? $scope.ItemDetails.marginpercentage : null;
                    $scope.ItemDetails.salediscountpercentage = $scope.ItemDetails.salediscountpercentage > 0 ? $scope.ItemDetails.salediscountpercentage : null;

                    $scope.salediscountChanged();
                    $scope.marginChanged();
                }
            }).error(function (result) { });
        };

        $scope.purchasediscountpercentagechanged = function () {
            $scope.ItemDetails.purchasediscount = ($scope.GetNumber($scope.ItemDetails.purchasediscountpercentage) * $scope.GetNumber($scope.ItemDetails.basebuyingprice) / 100).round(2);
            $scope.fixTax(false);
        };

        $scope.purchasediscountchanged = function () {
            $scope.ItemDetails.purchasediscountpercentage = ($scope.ItemDetails.purchasediscount / $scope.GetNumber($scope.ItemDetails.basebuyingprice) * 100).round(2);
            $scope.fixTax(false);
        };


        $scope.TryIntelligentMargin = function (callback) {
            if ($scope.CurrentSalesTaxConfig != null &&
                $scope.CurrentSalesTaxConfig.taxsplit != null &&
                $scope.CurrentSalesTaxConfig.taxsplit.length > 1) {

                var sellingprice = 0;
                if ($scope.SupportMarginOnCost) {

                    var buyingprice = $scope.GetNumber($scope.ItemDetails.basebuyingprice);
                    if ($scope.SupportMarginincludetax) {
                        var newtaxvalue = buyingprice * ($scope.ItemDetails.purchasetaxpercentage) / 100;
                        buyingprice = newtaxvalue + buyingprice;
                    }
                    sellingprice = buyingprice + (buyingprice * $scope.GetNumber($scope.ItemDetails.marginpercentage) / 100).round(2);

                } else {

                    var buyingprice = $scope.GetNumber($scope.ItemDetails.buyingprice);
                    if ($scope.SupportMarginincludetax) {
                        var newtaxvalue = buyingprice * ($scope.ItemDetails.purchasetaxpercentage) / 100;
                        buyingprice = newtaxvalue + buyingprice;
                    }

                    sellingprice = buyingprice + (buyingprice * $scope.GetNumber($scope.ItemDetails.marginpercentage) / 100).round(2);
                }

                if ($scope.GetNumber($scope.ItemDetails.salediscountpercentage) > 0)
                    sellingprice = ($scope.GetNumber(sellingprice) / (100 - $scope.GetNumber($scope.ItemDetails.salediscountpercentage)) * 100);

                var taxconfig = $scope.CurrentSalesTaxConfig.taxsplit.findSalesTax(sellingprice);
                $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }
            if (typeof (callback) != 'undefined')
                callback();
        }

        $scope.SearchSKUDesign = function (loadPrice) {
            if (!$scope.system_supportskudesignpurchase) {
                if (loadPrice)
                    $scope.setFocusbuyingqty();
                return;
            }
            var request = {
                "Item": {
                    'code': $scope.ItemDetails.designid,
                    'uancode': $scope.ItemDetails.Details.serialno,
                    'productid': $scope.ProductSearch.ProductID,
                    'colourid': $scope.ItemDetails.Details.colourid,
                    'sizeid': $scope.ItemDetails.Details.sizeid,
                    'sleeveid': $scope.ItemDetails.Details.sleeveid
                },
                "securitytoken": "String content"
            }
            $scope.ProductSearch.CurrentSKUList = [];
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgdesignService/SearchUAN",

                data: srequest
            }).success(function (resultList) {
                if (angular.isArray(resultList)) {
                    $scope.ProductSearch.CurrentSKUList = resultList;
                    results = resultList[0];
                    $scope.SelectItemFromMfgSKU(resultList);
                    return;
                }
                else {
                    results = resultList;
                    $scope.ProductSearch.CurrentSKUList = [results];
                }

                if (loadPrice && results && results != null) {

                    if ($scope.ItemDetails.Details.serialno &&
                        $scope.ItemDetails.Details.serialno == results.uid &&
                        $scope.ProductSearch.CurrentSKUList.length == 1) {
                        $scope.ProductSearch.ProductID = results.productid;
                        $scope.ItemDetails.Details.productid = results.productid;
                        $scope.ItemDetails.Details.brandid = results.brandid;
                        $scope.ItemDetails.Details.designid = results.code;
                        $scope.ItemDetails.designid = results.code;
                        $scope.ItemDetails.Details.sleeveid = results.sleeveid;
                        $scope.ItemDetails.Details.patternid = results.patternid;
                        $scope.ItemDetails.Details.materialid = results.materialid;
                        $scope.ItemDetails.Details.sizeid = results.sizeid;
                        $scope.ItemDetails.Details.colourid = results.colourid;
                        $scope.ItemDetails.Details.styleid = results.styleid;
                        $scope.ItemDetails.Details.typeid = results.typeid;
                        $scope.ItemDetails.Details.itemid = results.itemid;
                    }

                    $scope.ItemDetails.mrp = results.mrp;
                    $scope.ItemDetails.sellingprice = results.wsrate;
                    $scope.ItemDetails.Details.serialno = results.uid;
                    $scope.ItemDetails.Details.uan = results.uid;
                    $scope.salediscountChanged();
                    $scope.calculatesalestax();
                    $scope.purchasediscountpercentagechanged();
                    $scope.sellingpriceChange();
                    $scope.marginChanged();
                    $scope.ItemDetails.mrp = results.mrp;
                    $scope.ItemDetails.sellingprice = results.wsrate;
                    $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                    $scope.setFocusbuyingqty();
                }
            });
        }

        $scope.SelectItemFromMfgSKU = function (list) {
            var groupedlist = [];

            $.each(list, function (k, v) {
                var grp = groupedlist.find(function (item) {
                    return item.colourid == v.colourid;
                });
                if (grp == undefined || grp == null) {
                    var colour = $scope.ALLColorList.find(function (item) {
                        return item.id == v.colourid;
                    });

                    if (colour && colour != null) {
                        grp = JSON.parse(JSON.stringify(v));
                        grp.display = colour.name + ' - ' + v.name;
                        grp.items = [];
                        grp.id = groupedlist.length + 1;
                        groupedlist.push(grp);
                    }
                    else {
                        return;
                    }
                }
                v.sno = grp.items.length + 1;
                grp.items.push(v);
            })

            var dialog = ngDialog.open({
                template: 'ChooseMfgSKU',
                data: {
                    'GroupList': groupedlist,
                    'ColourList': $scope.ALLColorList,
                    'SizeList': $scope.ALLSizeList,
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.ColourList = $scope.ngDialogData.ColourList;
                    $scope.SizeList = $scope.ngDialogData.SizeList;
                    $scope.GroupList = $scope.ngDialogData.GroupList;
                    $scope.GroupItems = [];
                    $scope.ColourChanged = function () {
                        if ($scope.grid != null)
                            $scope.grid.destroy();
                        var group = $scope.GroupList.find(function (item) {
                            return item.id == $scope.groupid;
                        });
                        if (group && group != null) {
                            $scope.GroupItems = group.items;
                            var igrid = new InfyGrid('grid', '200px', '100%', true, false, true);
                            igrid.options.showHeaderRow = true;
                            igrid.AddSimpleColumn('sno', 'S.No', 40);
                            igrid.AddSimpleColumn('uid', 'UAN', 100);
                            igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.ColourList)
                            igrid.AddReferenceColumn('sizeid', 'Size', 70, $scope.SizeList)
                            var column = igrid.AddColumn({
                                id: "buyingqty",
                                name: "Qty",
                                field: "buyingqty",
                                width: 60,
                                cssClass: "cell-title",
                                editor: Slick.Editors.Text,
                                'sortable': true
                            });
                            igrid.options.autoEdit = true;
                            igrid.options.showHeaderRow = true;
                            igrid.Initialize();
                            $scope.grid = igrid;
                            $scope.grid.UpdateData($scope.GroupItems);
                        }
                    }

                    $scope.Selected = function () {
                        var selectedlist = [];
                        $scope.basebuyingprice = parseFloat($scope.buyingprice);
                        if (isNaN($scope.basebuyingprice))
                            $scope.basebuyingprice = 0;

                        if ($scope.basebuyingprice == 0) {
                            $scope.Toaster.Error("Please enter the cost of item");
                            return;
                        }

                        $.each($scope.GroupItems, function (k, v) {
                            if (v.buyingqty && v.buyingqty > 0) {
                                selectedlist.push(v);
                                v.basebuyingprice = $scope.basebuyingprice;
                                v.purchasediscountpercentage = 0;
                                v.purchasediscount = 0;
                                if ($scope.purchasediscount && $scope.purchasediscount != null) {
                                    var purchasediscount = $scope.purchasediscount.toString();
                                    var value = parseFloat(purchasediscount.replace('%', ''));
                                    if (isNaN(value))
                                        value = 0;
                                    if (value > 0) {
                                        if (purchasediscount.endsWith('%')) {
                                            v.purchasediscountpercentage = value;
                                            v.purchasediscount = ($scope.basebuyingprice * value / 100).round(2);
                                        }
                                        else {
                                            v.purchasediscountpercentage = (value / $scope.basebuyingprice * 100).round(2);
                                            v.purchasediscount = value;
                                        }
                                    }
                                }
                                v.buyingprice = v.basebuyingprice - v.purchasediscount;
                            }
                        });

                        $scope.closeThisDialog(selectedlist.length == 0 ? null : selectedlist);
                    };

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        var results = data.value[0];

                        $scope.ProductSearch.ProductID = results.productid;
                        $scope.ItemDetails.Details.productid = results.productid;
                        $scope.ItemDetails.Details.brandid = results.brandid;
                        $scope.ItemDetails.Details.designid = results.code;
                        $scope.ItemDetails.designid = results.code;
                        $scope.ItemDetails.purchasetotaldiscount = 0;
                        $scope.ItemDetails.purchasediscountpercentage = $scope.GetNumber(results.purchasediscountpercentage);
                        $scope.ItemDetails.purchasediscount = $scope.GetNumber(results.purchasediscount);
                        $scope.ItemDetails.basebuyingprice = $scope.GetNumber(results.basebuyingprice);
                        $scope.ItemDetails.buyingprice = $scope.GetNumber(results.buyingprice);
                        $scope.ItemDetails.Details.sleeveid = results.sleeveid;
                        $scope.ItemDetails.Details.patternid = results.patternid;
                        $scope.ItemDetails.Details.materialid = results.materialid;
                        $scope.ItemDetails.Details.sizeid = results.sizeid;
                        $scope.ItemDetails.Details.colourid = results.colourid;
                        $scope.ItemDetails.Details.styleid = results.styleid;
                        $scope.ItemDetails.Details.typeid = results.typeid;
                        $scope.ItemDetails.Details.itemid = results.itemid;
                        $scope.ItemDetails.mrp = results.mrp;
                        $scope.ItemDetails.sellingprice = results.wsrate;
                        $scope.ItemDetails.Details.serialno = results.uid;
                        $scope.ItemDetails.Details.uan = results.uid;
                        $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                        var cloneDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                        cloneDetails.id = 0;

                        $scope.ItemDetails = (JSON.parse(JSON.stringify(cloneDetails)));
                        $scope.ItemDetails.IsParent = true;
                        $scope.ItemDetails.IsChild = false;
                        $scope.ItemDetails.IsEditItem = false;
                        $scope.AddItemInternal(false);
                        var parent = $scope.ItemDetails;
                        parent.Child = [];
                        parent.amount = 0;
                        var taxitem = $scope.CurrentPurchaseTaxConfig;
                        var salestaxitem = $scope.CurrentSalesTaxConfig;
                        $.each(data.value, function (k, v) {
                            try {
                                $scope.ItemDetails = (JSON.parse(JSON.stringify(cloneDetails)));
                                $scope.ItemDetails.IsEditItem = false;
                                $scope.ItemDetails.Details.sizeid = v.sizeid;
                                $scope.ItemDetails.buyingqty = $scope.GetNumber(v.buyingqty);
                                $scope.ItemDetails.purchasediscountpercentage = $scope.GetNumber(v.purchasediscountpercentage);
                                $scope.ItemDetails.purchasediscount = $scope.GetNumber(v.purchasediscount);
                                $scope.ItemDetails.basebuyingprice = $scope.GetNumber(v.basebuyingprice);
                                $scope.ItemDetails.buyingprice = $scope.GetNumber(v.buyingprice);
                                $scope.ItemDetails.sellingprice = $scope.GetNumber(v.wsrate);
                                $scope.ItemDetails.salediscount = $scope.GetNumber(v.salediscount);

                                $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                                $scope.ItemDetails.mrp = $scope.GetNumber(v.mrp);
                                $scope.ItemDetails.uan = v.uid;
                                $scope.ItemDetails.Details.serialno = v.uid;
                                $scope.ItemDetails.Details.uan = v.uid;
                                $scope.ItemDetails.dealerprice = v.dealerprice;
                                $scope.ItemDetails.wsrate = v.wsrate;
                                if (taxitem == null) {
                                    $scope.ItemDetails.purchasetaxpercentage = 0;
                                } else {
                                    var taxconfig = taxitem.taxsplit.findTax($scope.ItemDetails.buyingprice);
                                    $scope.ItemDetails.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
                                }

                                if (salestaxitem == null) {
                                    $scope.ItemDetails.salestaxpercentage = 0;
                                } else {
                                    var taxconfig = salestaxitem.taxsplit.findSalesTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                                    $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
                                }

                                $scope.ItemDetails.amount = $scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty);
                                $scope.ItemDetails.purchasetotaldiscount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty) * $scope.ItemDetails.purchasediscountpercentage / 100).round(2);
                                $scope.ItemDetails.amount -= $scope.ItemDetails.purchasetotaldiscount;
                                if ($scope.Invoice.IsUnregisteredInvoice)
                                    $scope.ItemDetails.purchasetax = 0;
                                else
                                    $scope.ItemDetails.purchasetax = ($scope.ItemDetails.amount * $scope.ItemDetails.purchasetaxpercentage / 100).round(2);
                                $scope.ItemDetails.purchasediscount = ($scope.ItemDetails.purchasetotaldiscount / $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);

                                $scope.ItemDetails.salediscountpercentage = $scope.GetNumber(v.salediscountpercentage);
                                $scope.ItemDetails.salestax = ($scope.GetNumber($scope.ItemDetails.buyingqty) * ($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount) / ($scope.ItemDetails.salestaxpercentage + 100) * $scope.ItemDetails.salestaxpercentage).round(2);
                                $scope.ItemDetails.IsChild = true;
                                $scope.ItemDetails.IsParent = false;

                                parent.amount += (($scope.Invoice.isinclusivediscount ? $scope.GetNumber($scope.ItemDetails.basebuyingprice) : $scope.GetNumber($scope.ItemDetails.buyingprice)) *
                                    $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);
                                parent.amount += $scope.GetNumber(v.buyingqty);


                                parent.Child.push($scope.ItemDetails);
                                $scope.AddItemInternal(false);
                            } catch (e) {
                                e = null;
                            }
                        });

                        parent.purchasediscount = $scope.GetNumber(parent.Child[0].purchasediscount);
                        parent.purchasetotaldiscount += $scope.GetNumber(parent.Child[0].purchasetotaldiscount);
                        parent.marginpercentage = $scope.GetNumber(parent.Child[0].marginpercentage);
                        parent.margin = $scope.GetNumber(parent.Child[0].margin);

                        parent.sellingprice = $scope.GetNumber(parent.Child[0].sellingprice);
                        parent.salediscount = $scope.GetNumber(parent.Child[0].salediscount);
                        parent.buyingprice = $scope.GetNumber(parent.Child[0].buyingprice);

                        parent.amount = parent.amount.round(2);
                        $scope.CalculateTotal();
                        $scope.ItemDetails = (JSON.parse(JSON.stringify(cloneDetails)));
                        $scope.ItemDetails.IsEditItem = false;
                        $scope.CreateNew();
                        $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                    }
                }
            });

        }


        $scope.DesignList = [{ 'id': 'ADD DESIGN', 'name': 'ADD DESIGN' }];
        //$scope.ShowingDesign = false;
        $scope.AddNewDesign = function () {
            //if ($scope.ShowingDesign) return;
            $scope.ItemDetails.designid = '';
            //$scope.ShowingDesign = true;
            var dialog = ngDialog.open({
                template: 'ShowAddDesign',
                data: [$scope],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.ParentScope = $scope.ngDialogData[0];
                    $scope.Save = function () {
                        $scope.closeThisDialog({ 'design': $scope.design, 'autonumber': $scope.autonumber });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        if (data.value.design && data.value.design != null && data.value.design != '') {
                            var srequest = JSON.stringify({
                                'Item': {
                                    'supplierid': $scope.Invoice.supplierid,
                                    'productid': $scope.ProductSearch.ProductID,
                                    'designid': data.value.design,
                                    'autonumber': data.value.autonumber,
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
                                        $scope.ItemDetails.designid = result.designid;
                                    }, 20);
                                    $scope.SearchSKUDesign(true);
                                }
                            });
                        }

                    }
                }
            });

            //$rootScope.Prompt('Design', 'Design Name', function (design) {
            //    $scope.ShowingDesign = false;
            //    if (design && design != null && design != '') {
            //        var srequest = JSON.stringify({
            //            'Item': {
            //                'supplierid': $scope.Invoice.supplierid,
            //                'productid': $scope.ProductSearch.ProductID,
            //                'designid': design
            //            }
            //        });
            //        $http({
            //            method: 'POST',
            //            url: "ProductdesignService/Save",
            //            data: srequest
            //        }).success(function (result) {
            //            if (result && result.id > 0) {
            //                var dlist = $scope.DesignList;
            //                dlist.splice(dlist.length - 1, 1);
            //                $scope.DesignList = null;
            //                dlist.push({'name': result.designid, 'id': result.designid});
            //                dlist.push({ 'id': 'ADD DESIGN', 'name': 'ADD DESIGN' });
            //                $timeout(function () {
            //                    $scope.DesignList = dlist;
            //                    $scope.ItemDetails.designid = result.designid;
            //                }, 20);
            //                $scope.SearchSKUDesign(true);
            //            }
            //        });
            //    }
            //}, '', undefined, function () {
            //    $scope.ShowingDesign = false
            //    $scope.SearchSKUDesign(true);
            //});
        }

        $scope.ValidateNewDesign = function () {
            if ($scope.ItemDetails.designid == 'ADD DESIGN') {
                $scope.AddNewDesign();
            }
            else {
                $scope.SearchSKUDesign(true);
            }
        }

        $scope.SearchDesignList = function (selecteddesignid) {
            if ($scope.ProductSearch == undefined || $scope.ProductSearch == null ||
                $scope.ProductSearch.ProductID == undefined || $scope.ProductSearch.ProductID == null)
                return;
            var request = {
                "Item": {
                    'supplierid': $scope.Invoice.supplierid,
                    'productid': $scope.ProductSearch.ProductID
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

                if ($scope.Invoice.Lrinvoiceitems && $scope.Invoice.Lrinvoiceitems != null) {
                    $.each($scope.Invoice.Lrinvoiceitems, function (k, v) {
                        if (v.productid == $scope.ItemDetails.productid &&
                            v.designid && v.designid != null && v.designid != '') {
                            var existdesign = designlist.find(function (item) {
                                return item.id == v.designid;
                            });
                            if (existdesign == undefined || existdesign == null) {
                                designlist.push({ 'id': v.designid, 'name': v.designid });
                            }
                        }
                    });
                }

                designlist.push({ 'id': 'ADD DESIGN', 'name': 'ADD DESIGN' });
                $scope.DesignList = designlist;
                if (selecteddesignid && selecteddesignid != null) {
                    $scope.ItemDetails.designid = null;
                    $timeout(function () {
                        $scope.ItemDetails.designid = selecteddesignid;
                    },20)
                }
            })
        }

        $scope.SearchDesign = function () {
            var request = {
                "Item": {
                    'suppliercompanyid': $scope.Invoice.suppliercompanyid,
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
                if (result != null && result.Details && result.Details != null) {
                    $scope.ItemDetails.Details = result.Details;
                    $scope.ItemDetails.buyingprice = result.buyingprice;
                    $scope.ItemDetails.basebuyingprice = result.basebuyingprice;
                    $scope.ItemDetails.purchasediscountpercentage = result.purchasediscountpercentage;
                    $scope.ItemDetails.salediscountpercentage = result.salediscountpercentage;
                    $scope.ItemDetails.marginpercentage = result.marginpercentage;
                    $scope.ItemDetails.marginpercentage = $scope.ItemDetails.marginpercentage > 0 ? $scope.ItemDetails.marginpercentage : null;
                    $scope.ItemDetails.salediscountpercentage = $scope.ItemDetails.salediscountpercentage > 0 ? $scope.ItemDetails.salediscountpercentage : null;
                    $scope.ItemDetails.mrp = result.mrp;
                    $scope.ItemDetails.sellingprice = result.sellingprice;
                    $scope.ItemDetails.createdon = result.createdon;
                    $scope.salediscountChanged();
                    $scope.calculatesalestax();
                    $scope.purchasediscountpercentagechanged();
                    $scope.sellingpriceChange();
                    $scope.marginChanged();
                    $scope.ItemDetails.mrp = result.mrp;
                    $scope.ItemDetails.sellingprice = result.sellingprice;
                    $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                }
                $scope.DesignList = result.designlist;
            });
        };

        $scope.FitSellingPrice = function () {
            var lastSellingPrice = 0;
            var lastDiscount = 0;
            var lastMargin = 0;
            var cnfgSellingPrice = 0;
            var cnfgDiscount = 0;
            var cnfgMarginPercentage = 0;
            var cnfgName = '';
            $scope.ItemDetails.priceinfo = {};
            if ($scope.LastPrice != null) {
                //$scope.ItemDetails.designid = $scope.LastPrice.designid;
                $scope.ItemDetails.lastsellingprice = $scope.LastPrice.sellingprice;
                $scope.ItemDetails.lastbuyingprice = $scope.LastPrice.buyingprice;
                $scope.ItemDetails.lastdiscount = $scope.LastPrice.discount;
                lastSellingPrice = $scope.LastPrice.sellingprice;
                lastDiscount = $scope.LastPrice.discount;
                lastMargin = $scope.LastPrice.marginpercentage;

            }
            if ($scope.Price != null) {

                var cnfgcost = $scope.GetNumber($scope.ItemDetails.basebuyingprice);
                if ($scope.Price.costmode == 1) {
                    cnfgcost -= $scope.GetNumber($scope.ItemDetails.purchasediscount)
                } else if ($scope.Price.costmode == 2) {
                    cnfgcost = $scope.GetNumber($scope.ItemDetails.buyingprice) - $scope.GetNumber($scope.ItemDetails.purchasetax);
                }
                else if ($scope.Price.costmode == 3) {
                    cnfgcost = $scope.GetNumber($scope.ItemDetails.buyingprice);
                }

                if ($scope.Price.pricingmode == 1) {
                    cnfgName = 'Margin @' + $scope.Price.pricingvalue + '%';
                    cnfgSellingPrice = $scope.RoundPrice(cnfgcost + (cnfgcost * $scope.Price.pricingvalue / 100).round(2));
                    cnfgMarginPercentage = $scope.Price.pricingvalue;
                }
                else if ($scope.Price.pricingmode == 2) {
                    cnfgName = 'Margin @' + $scope.Price.pricingvalue + '%';
                    cnfgMarginPercentage = $scope.Price.pricingvalue;
                    var markpercent = 1 + ($scope.Price.pricingvalue / (100 - $scope.Price.pricingvalue));
                    if (markpercent == 100) {
                        var markpercent = (1 + (99 / (100 - 99))) + (1 + (1 / (100 - 1)));
                    }
                    if (markpercent < 0) Math.abs(markpercent) + 100;
                    cnfgSellingPrice = (baseprice * markpercent).round(2);
                }
                else if ($scope.Price.pricingmode == 3) {
                    cnfgName = 'Margin @' + $scope.Price.pricingvalue;
                    cnfgSellingPrice = cnfgcost + $scope.Price.pricingvalue;
                    $scope.ItemDetails.marginprice = $scope.Price.pricingvalue;
                }
                else if ($scope.Price.pricingmode == 4) {
                    cnfgName = 'Fixed Price @' + $scope.Price.pricingvalue;
                    cnfgSellingPrice = $scope.Price.pricingvalue;
                }

                if ($scope.Price.marginmode == 1) {
                    var sellingprice = $scope.GetNumber(cnfgSellingPrice);
                    var salestaxpercentage = $scope.ItemDetails.salestaxpercentage;
                    var taxitem = $scope.CurrentSalesTaxConfig;
                    if (taxitem != null) {
                        var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber(sellingprice));
                        salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
                    }
                    cnfgSellingPrice = sellingprice + (sellingprice * salestaxpercentage / 100);
                    cnfgMarginPercentage = ((cnfgSellingPrice / $scope.GetNumber($scope.ItemDetails.buyingprice) * 100) - 100).round(2);
                    if ($scope.SupportMarkDOWN)
                        cnfgMarginPercentage = (100 - (($scope.GetNumber($scope.ItemDetails.buyingprice) / cnfgSellingPrice) * 100)).round(2);
                }

                if ($scope.Price.discountmode == 3) {
                    cnfgDiscount = $scope.Price.discountvalue;
                }
                else if ($scope.Price.discountmode == 2 && cnfgSellingPrice > 0) {
                    cnfgDiscount = ($scope.Price.discountvalue / cnfgSellingPrice * 100).round(2);
                }
            }


            if (cnfgMarginPercentage != 0) {
                $scope.ItemDetails.marginpercentage = cnfgMarginPercentage;
                $scope.ItemDetails.marginpercentage = $scope.ItemDetails.marginpercentage > 0 ? $scope.ItemDetails.marginpercentage : null;
                $scope.marginChanged();
            } else if (lastSellingPrice > 0 && cnfgSellingPrice == 0) {
                $scope.ItemDetails.sellingprice = lastSellingPrice;
            } else if (lastSellingPrice == 0 && cnfgSellingPrice > 0) {
                $scope.ItemDetails.sellingprice = cnfgSellingPrice;
            } else if (lastSellingPrice == cnfgSellingPrice) {
                $scope.ItemDetails.sellingprice = lastSellingPrice;
            } else if (lastSellingPrice != cnfgSellingPrice) {
                $scope.ItemDetails.sellingprice = cnfgSellingPrice;
                $scope.ItemDetails.mrp = $scope.ItemDetails.sellingprice;

                if ($scope.SupportMarginOnCost) {
                    if ($scope.SupportMarginexcludetax) {
                        $scope.ComputeExcludeMargin();
                    } else {
                        $scope.ItemDetails.marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / $scope.GetNumber($scope.ItemDetails.basebuyingprice) * 100) - 100).round(2);
                        $scope.ItemDetails.marginpercentage = $scope.ItemDetails.marginpercentage > 0 ? $scope.ItemDetails.marginpercentage : null;
                    }
                } else {
                    if ($scope.SupportMarginexcludetax) {
                        $scope.ComputeExcludeMargin();
                    } else {
                        $scope.ItemDetails.marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / $scope.GetNumber($scope.ItemDetails.buyingprice) * 100) - 100).round(2);
                        $scope.ItemDetails.marginpercentage = $scope.ItemDetails.marginpercentage > 0 ? $scope.ItemDetails.marginpercentage : null;
                    }
                }

                $scope.salediscountChanged();
                /*$scope.ShowPricingOption(
                {
                    'Mechanism': cnfgName,
                    'Value': cnfgSellingPrice,
                    'Discount': cnfgDiscount,
                    'LastPrice': lastSellingPrice,
                    'LastDiscount': lastDiscount
                });*/
            } else if (cnfgName != '') {
                $rootScope.Toaster.Error(cnfgName);
            }

            if (cnfgDiscount > 0) {
                $scope.ItemDetails.salediscountpercentage = cnfgDiscount;
                $scope.salediscountChanged();
            }
            $scope.ItemDetails.priceinfo = {
                'margin': cnfgMarginPercentage,
                'discount': cnfgDiscount,
                'lastprice': $scope.LastPrice != null ? $scope.LastPrice.basebuyingprice : 0,
                'lastmargin': $scope.LastPrice != null ? $scope.LastPrice.marginpercentage : 0
            };
            $scope.ItemDetails.priceinfo = {
                'lastprice': lastSellingPrice,
                'lastdiscount': lastDiscount,
                'margin': cnfgMarginPercentage,
                'discount': cnfgDiscount,
                'lastmargin': $scope.LastPrice != null ? $scope.LastPrice.marginpercentage : 0
            };
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
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.ItemDetails.sellingprice = data.value[0];
                        $scope.ItemDetails.salediscount = data.value[1];
                        $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                    }
                }
            });
        };



        $scope.ShowLastPriceInfo = function () {
            var dialog = ngDialog.open({
                template: 'LastPriceInfo',
                data: [$scope.LastPrice],
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.LastPrice = $scope.ngDialogData[0];
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                { }
            });
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
                    $scope.Purchase = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
                        $scope.Purchase = results;
                    });
                    $scope.Sales = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
                        $scope.Sales = results;
                    });
                    $scope.ProductType = [{ 'id': 0, 'name': 'Textile' },
                    { 'id': 1, 'name': 'FMCG Products' },
                    { 'id': 2, 'name': 'Groceries' },
                    { 'id': 3, 'name': 'Vegitable/Fruites' },
                    { 'id': 6, 'name': 'Mobile' },
                    { 'id': 7, 'name': 'Accessories' },
                    { 'id': 8, 'name': 'Jewellery' },
                    ];
                    $scope.BarcodeMode = [{ 'id': 2, 'name': 'Only Number' }, { 'id': 0, 'name': 'Unique' },
                    { 'id': 1, 'name': 'CAPS and Number' },
                    { 'id': 3, 'name': 'UAN' }, { 'id': 4, 'name': 'IEMI' }];

                    $scope.SellingMode = [{ 'id': 2, 'name': 'Pack' }, { 'id': 1, 'name': 'Piece' }, { 'id': 3, 'name': 'Cut' }];

                    $scope.AttributeList = [{
                        'id': 'PRODUCT',
                        'name': 'Product'
                    },
                    {
                        'id': 'BRAND',
                        'name': 'Brand'
                    },
                    {
                        'id': 'SIZE',
                        'name': 'Size'
                    },
                    {
                        'id': 'COLOUR',
                        'name': 'Colour'
                    },
                    {
                        'id': 'STYLE',
                        'name': 'Style'
                    },
                    {
                        'id': 'PATTERN',
                        'name': 'Pattern'
                    },
                    {
                        'id': 'MATERIAL',
                        'name': 'Material'
                    },
                    {
                        'id': 'TYPE',
                        'name': 'Type'
                    },
                    {
                        'id': 'SLEEVE',
                        'name': 'Sleeve'
                    },
                    {
                        'id': 'FIT',
                        'name': 'Fit'
                    }
                    ]


                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'type': $scope.type,
                            'code': $scope.code,
                            'name': $scope.name,
                            'sellingmode': $scope.sellingmode,
                            'barcodemode': $scope.barcodemode,
                            'salestaxid': $scope.salestaxid,
                            'purchasetaxid': $scope.purchasetaxid,
                            'producttype': $scope.producttype
                        });
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

                            if (result != null && result.id == 0) {
                                $rootScope.Toaster.Error(result.ErrorMessage);
                                return;
                            }

                            var item = {
                                'id': result.id,
                                'type': result.type,
                                'code': result.code,
                                'name': result.name,
                                'sellingmode': data.value.sellingmode,
                                'barcodemode': data.value.barcodemode,
                                'salestaxid': data.value.salestaxid,
                                'purchasetaxid': data.value.purchasetaxid,
                                'new': true
                            };

                            switch (result.type) {
                                case 'PRODUCT':
                                    $scope.AllProductList = $scope.CloneArray($scope.AllProductList, item);
                                    break;
                                case 'BRAND':
                                    $scope.BrandNameList = $scope.CloneArray($scope.BrandNameList, item);
                                    break;
                                case 'SIZE':
                                    $scope.ALLSizeList = $scope.CloneArray($scope.ALLSizeList, item);
                                    break;
                                case 'COLOUR':
                                    $scope.ALLColorList = $scope.CloneArray($scope.ALLColorList, item);
                                    break;
                                case 'STYLE':
                                    $scope.ALLStyleList = $scope.CloneArray($scope.ALLStyleList, item);
                                    break;
                                case 'PATTERN':
                                    $scope.ALLPatternList = $scope.CloneArray($scope.ALLPatternList, item);
                                    break;
                                case 'MATERIAL':
                                    $scope.ALLMaterialList = $scope.CloneArray($scope.ALLMaterialList, item);
                                    break;
                                case 'TYPE':
                                    $scope.ALLTypeList = $scope.CloneArray($scope.ALLTypeList, item);
                                    break;
                                case 'SLEEVE':
                                    $scope.AllSleeveList = $scope.CloneArray($scope.AllSleeveList, item);
                                    break;
                                case 'FIT':
                                    $scope.AllFitList = $scope.CloneArray($scope.AllFitList, item);
                                    break;
                            }

                            if (result.type == 'PRODUCT' || result.type == 'BRAND') {
                                $scope.FilterProductByCompany();
                            } else {
                                $scope.ProductAttributeFilter(null);
                            }

                        }).error(function (result) { });

                    }
                }
            });
        }

        $scope.CloneArray = function (array, item) {
            array.push(item);
            return array.map(function (i) {
                return i;
            });
        }

        $scope.ChooseCompany = function (callback) {
            var dialog = ngDialog.open({
                template: 'ChooseCompany',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.RetialLocations = $scope.ngDialogData.scope.RetialLocations;
                    $scope.BuyingCompany = $scope.ngDialogData.scope.BuyingCompany;

                    $timeout(function () {
                        $scope.retaillocationid = $scope.RetialLocations[0].id;
                        $scope.companyid = $scope.BuyingCompany[0].id;
                    }, 100);

                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'stocklocationid': $scope.retaillocationid,
                            'companyid': $scope.companyid
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        if (typeof (callback) != 'undefined' && callback != null)
                            callback(data.value);
                    }
                }
            });
        }

        $scope.ChooseLocation = function (ItemDetails, callback) {
            var dialog = ngDialog.open({
                template: 'ChooseLocation',
                data: {
                    'ItemDetails': ItemDetails,
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.retaillocationid = $scope.ngDialogData.ItemDetails.stocklocationid;
                    $scope.RetialLocations = $scope.ngDialogData.scope.RetialLocations;
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'stocklocationid': $scope.retaillocationid
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        ItemDetails.stocklocationid = data.value.stocklocationid;

                        if (typeof (ItemDetails.Child) != 'undefined' && ItemDetails.Child.length > 0) {
                            $.each($scope.ItemDetails.Child, function (k, v) {
                                v.stocklocationid = data.value.stocklocationid;
                            })
                        }
                    }
                    if (typeof (callback) != 'undefined' && callback != null)
                        callback();
                }
            });
        }

        $scope.SearchGLN = function (nextfield) {
            if ($scope.ItemDetails.Details.serialno == undefined ||
                $scope.ItemDetails.Details.serialno == null ||
                $scope.ItemDetails.Details.serialno == '') {
                $scope['setFocussno']();
                return;
            }

            if ($scope.system_supportskudesignpurchase) {
                $scope.SearchSKUDesign(true);
                return;
            }

            var request = {
                "Item": {
                    'gln': $scope.ItemDetails.Details.serialno,
                    "skudesign": $scope.system_supportskudesignpurchase
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {
                    $scope.ItemDetails.Details = result[0];
                    $scope.ProductSearch.Itemid = result[0].id;
                    $scope.ItemDetails.Details.serialno = request.Item.gln;
                    $scope.ItemDetails.uan = request.Item.gln;
                    $scope.ItemDetails.designid = $scope.ItemDetails.Details.glndesignid;
                    $scope.ItemDetails.productid = $scope.ItemDetails.Details.productid;
                    $scope.ProductSearch.ProductID = $scope.ItemDetails.Details.productid;
                    var sizeid = result[0].sizeid;
                    $scope.productChange();
                    $scope.ItemDetails.Details.sizeid = sizeid;
                    $scope.ItemDetails.sellingprice = result[0].salerate;
                    $scope.ItemDetails.mrp = result[0].mrp;
                    $scope.ItemDetails.dealerprice = result[0].dealerprice;
                    $scope.ItemDetails.basebuyingprice = result[0].purchaserate;
                    $scope.ItemDetails.buyingprice = result[0].purchaserate;

                    if ($scope.ItemDetails.sellingprice > 0) {
                        $scope.ItemDetails.glnprice = true;
                    }

                    if ($scope.SupportSalepriceMrpdifference &&
                        result[0].mrp > result[0].salerate) {
                        $timeout(function () {
                            $scope.ItemDetails.sellingprice = result[0].mrp;
                            $scope.ItemDetails.salediscount = $scope.ItemDetails.mrp - result[0].salerate;
                            $scope.ItemDetails.salediscountpercentage = ($scope.ItemDetails.salediscount / $scope.ItemDetails.sellingprice * 100).round(2);
                            $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                            if ($scope.SupportMarginexcludetax) {
                                $scope.ComputeExcludeMargin();
                            } else {
                                $scope.ComputeIncludeMargin();
                            }
                        }, 10);
                    } else {
                        $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                        if ($scope.SupportMarginexcludetax) {
                            $scope.ComputeExcludeMargin();
                        } else {
                            $scope.ComputeIncludeMargin();
                        }
                    }

                    $scope.SetFocusByFlow(nextfield);
                }
                else {
                    $scope['setFocussno']();
                }
            });

        }

        $scope.SearchItems = function (itemid) {
            var dialog = ngDialog.open({
                template: 'ClientSide/Views/Item.html?' + new Date(),
                data: [$scope, itemid],
                width: '950px',
                controller: 'ItemController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                    return;

                if (!data.value.IsEdit) {
                    $scope.ItemNameList.push(data.value);
                    $scope.ItemList.push(data.value);
                }
            });
        }

        $scope.UploadBarcodes = function () {
            var dialog = ngDialog.open({
                template: 'UploadBarcode',
                data: {
                    'barcodes': typeof ($scope.ItemDetails.barcodes) == 'undefined' ? null : $scope.ItemDetails.barcodes,
                    'ParentScope': $scope
                },
                width: '600px',
                controller: ['$scope', '$timeout', '$parse', '$http', '$rootScope', function ($scope, $timeout, $parse, $http, $rootScope) {
                    $scope.BarcodeList = [];
                    $scope.BarcodeListColumns = [];
                    $scope.ParentScope = $scope.ngDialogData.ParentScope;
                    $scope.Buyingqty = $scope.ParentScope.GetNumber($scope.ParentScope.ItemDetails.buyingqty);
                    $scope.BlankStyle = {};
                    $scope.FilledStle = {
                        "background-color": "lightgrey"
                    };

                    $scope.BarcodeList = [];
                    $scope.BarcodeListColumns = [{
                        'sno': 1,
                        'name': 'Barcode',
                        'selected': true,
                        'id': 0
                    }];
                    if (typeof ($scope.ngDialogData.barcodes) != 'undefined' &&
                        $scope.ngDialogData.barcodes != null) {
                        var list = JSON.parse($scope.ngDialogData.barcodes);
                        if (list.length > 0) {
                            $scope.BarcodeListColumns = [];
                            $.each(list, function (k, v) {
                                $scope.BarcodeList.push({
                                    'sno': $scope.BarcodeList.length + 1,
                                    'codes': [v],
                                    id: $scope.BarcodeList.length
                                });
                            });
                        }
                    } else if (typeof ($scope.ParentScope.LastBarcodeFile) != 'undefined') {
                        $scope.BarcodeList = $scope.ParentScope.LastBarcodeFile.BarcodeList;
                        $scope.BarcodeListColumns = $scope.ParentScope.LastBarcodeFile.BarcodeListColumns;
                    }

                    $scope.AddBarcode = function () {
                        $scope.BarcodeList.push({
                            'sno': $scope.BarcodeList.length + 1,
                            'codes': [$scope.serialno],
                            id: $scope.BarcodeList.length,
                            'selected': true
                        });
                        $scope.serialno = '';
                    }

                    $scope.ItemSelected = function (item, list) {
                        var index = item.id;
                        $.each(list, function (k, v) {
                            v.selected = false;
                            if (k >= index &&
                                k < $scope.Buyingqty + index) {
                                v.selected = true;
                            }
                        });
                    }

                    $scope.fileuploadedcomplete = function (value) {
                        $scope.barcodefile = null;
                        $scope.BarcodeList = [];
                        $scope.BarcodeListColumns = [];
                        $.each(value.columns, function (k, v) {
                            $scope.BarcodeListColumns.push({
                                'sno': k + 1,
                                'name': v,
                                'selected': true,
                                'id': k
                            });
                        });

                        $.each(value.rows, function (k, v) {
                            $scope.BarcodeList.push({
                                'sno': k + 1,
                                'codes': v.values,
                                id: k
                            });
                        });
                        $scope.ParentScope.LastBarcodeFile = {
                            'BarcodeList': $scope.BarcodeList,
                            'BarcodeListColumns': $scope.BarcodeListColumns
                        };
                    }

                    $scope.DeleteBarcode = function (item) {
                        $scope.BarcodeList.splice($scope.BarcodeList.indexOf(item), 1);
                        $.each($scope.BarcodeList, function (k, v) {
                            v.sno = k + 1;
                        });
                    }

                    $scope.Save = function () {
                        var list = [];
                        var duplicatebarcode = null;
                        $.each($scope.BarcodeList, function (k, v) {
                            if (v.selected) {
                                v.fillted = true;
                                var code = '';
                                $.each($scope.BarcodeListColumns, function (kk, vv) {
                                    if (vv.selected)
                                        code = v.codes[kk];
                                });
                                if (list.indexOf(code) < 0)
                                    list.push(code);
                                else
                                    duplicatebarcode = code;
                            }
                        });

                        if (duplicatebarcode != null) {
                            $rootScope.Toaster.Info(duplicatebarcode + ' barcode contains multiple time');
                            return;
                        }

                        if (list.length != $scope.Buyingqty) {
                            $rootScope.Toaster.Error('Please selected minimum ' + $scope.Buyingqty + ' barcode');
                            return;
                        }
                        $scope.closeThisDialog(list);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$document' && data.value != '$escape') {
                        $scope.ItemDetails.barcodes = JSON.stringify(data.value);
                    }
                    $scope.AddItemInternal(true);
                    $scope.CreateNew();
                }
            });
        }

        $scope.SearchProductRef = function (invoiceitem) {
            if (invoiceitem.Details.sellingname == '' || invoiceitem.Details.sellingname == null) {
                if (invoiceitem.Details.productname != null && invoiceitem.Details.productname != '')
                    return invoiceitem.Details.productname;
            }
            if (invoiceitem.Details.sellingname != null || invoiceitem.Details.sellingname != '') {

            }
            return invoiceitem.Details.sellingname;
        }

        $scope.qtyenterkey = function (issuccess, sfocus, ffocus) {
            if ($scope.ItemDetails.hidepricing) {
                $scope.TryAddItem();
            }
            else {
                $scope[issuccess ? 'setFocus' + sfocus : 'setFocus' + ffocus]();
            }
        }

        $scope.focusconditiononenter = function (issuccess, sfocus, ffocus) {
            $scope[issuccess ? 'setFocus' + sfocus : 'setFocus' + ffocus]();
        }


        $scope.ShowImageDialog = function (callback, currentimage) {
            var dialog = ngDialog.open({
                template: 'UpdateImage',
                data: [$scope, currentimage],
                width: '660px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.Billitemid = 0;
                    $scope.ShowSnap = false;
                    $scope.DisplayingCurrentImage = false;
                    $scope.CurrentImage = typeof ($scope.ngDialogData[1]) == 'undefined' ? '' : $scope.ngDialogData[1];

                    $scope.Capture = function (closewindow) {
                        var canvas = document.getElementById('canvas');
                        var context = canvas.getContext('2d');
                        var video = document.getElementById('video');
                        context.drawImage(video, 0, 0, 640, 480);
                        $scope.ShowSnap = true;
                        if (closewindow) {
                            $scope.Save();
                        }
                    }

                    $scope.Save = function () {
                        if ($scope.DisplayingCurrentImage) {
                            $scope.closeThisDialog($scope.CurrentImage);
                            return;
                        }
                        var canvas = document.getElementById('canvas');
                        var imgData = canvas.toDataURL();
                        var request = {
                            "Item": {
                                'image': imgData,
                                'type': 'IMAGE'
                            },
                            "SecurityToken": "String content"
                        }

                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "StockService/StoreImage",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    }

                    $scope.LaunchVideo = function () {
                        $scope.ShowSnap = false;
                        $scope.DisplayingCurrentImage = false;
                        $scope.CurrentImage = '';
                        var video = document.getElementById('video');

                        // Get access to the camera!
                        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                            // Not adding `{ audio: true }` since we only want video now
                            navigator.mediaDevices.getUserMedia({
                                video: true
                            }).then(function (stream) {
                                //video.src = window.URL.createObjectURL(stream);
                                video.srcObject = stream;
                                video.play();
                            });
                        } else if (navigator.getUserMedia) { // Standard
                            navigator.getUserMedia({
                                video: true
                            }, function (stream) {
                                video.src = stream;
                                video.play();
                            }, errBack);
                        } else if (navigator.webkitGetUserMedia) { // WebKit-prefixed
                            navigator.webkitGetUserMedia({
                                video: true
                            }, function (stream) {
                                video.src = window.webkitURL.createObjectURL(stream);
                                video.play();
                            }, errBack);
                        } else if (navigator.mozGetUserMedia) { // Mozilla-prefixed
                            navigator.mozGetUserMedia({
                                video: true
                            }, function (stream) {
                                video.srcObject = stream;
                                video.play();
                            }, errBack);
                        }
                    }
                    $timeout(function () {
                        if ($scope.CurrentImage != '') {
                            $scope.ShowSnap = true;
                            $scope.DisplayingCurrentImage = true;
                            var canvas = document.getElementById('canvas');
                            var context = canvas.getContext('2d');
                            var myImg = new Image();
                            myImg.onload = function () {
                                context.drawImage(myImg, 0, 0, 640, 480);
                                $scope.ShowSnap = true;
                            };
                            myImg.src = 'File/Show/' + $scope.UserInfo.OrganizationID + '/' + $scope.CurrentImage;
                        }
                        else {
                            $scope.LaunchVideo();
                            $scope.setFocuscapture();
                        }

                    }, 100);
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$document' &&
                        data.value != '$escape' && data.value != '') {
                        callback(data.value);
                    } else {
                        callback();
                    }
                }
            });
        }

        $scope.CreateMultiItems = function (ItemDetails, isJump, isView, isSizeGroup, isShowChange) {
            var dialog = ngDialog.open({
                template: 'MultiSize',
                data: {
                    'ItemDetails': ItemDetails,
                    'SearchRef': $scope.SearchRef,
                    'SizeList': $scope.SizeList,
                    'isJump': isJump,
                    'isView': isView,
                    'scope': $scope,
                    'isSizeGroup': isSizeGroup,
                    'system_hidecostinventory': $scope.system_hidecostinventory
                },
                width: '750px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.SupportUANPurchase = $scope.ngDialogData.scope.SupportUANPurchase;
                    $scope.ProductSearch = $scope.ngDialogData.scope.ProductSearch;
                    $scope.IsJump = $scope.ngDialogData.isJump;
                    $scope.SupportSalepriceMrpdifference = $scope.ngDialogData.scope.SupportSalepriceMrpdifference;
                    $scope.isSizeGroup = $scope.ngDialogData.isSizeGroup;
                    $scope.sizeInfo = '';
                    $scope.SizeGroupList = $scope.ngDialogData.scope.SizeGroupList;
                    $scope.system_hidecostinventory = $scope.ngDialogData.scope.system_hidecostinventory;
                    $scope.isView = $scope.ngDialogData.isView;
                    $scope.RetialLocations = $scope.ngDialogData.scope.RetialLocations;
                    $scope.CollectLocationInfo = $scope.ngDialogData.scope.CollectLocationInfo;
                    $scope.stocklocationid = $scope.ngDialogData.ItemDetails.stocklocationid;
                    $scope.Created = 0;
                    $scope.ParentScope = $scope.ngDialogData.scope;
                    $scope.SupportFinalPriceChange = $scope.ParentScope.SupportFinalPriceChange;
                    $scope.SupportRoundingDiscount = $scope.ParentScope.SupportRoundingDiscount;
                    $scope.AllowChangePrice = typeof (isShowChange) != 'undefined' && isShowChange;
                    $scope.CoreItem = $scope.ngDialogData.ItemDetails;
                    $scope.TotalAmount = 0;
                    $scope.Total = $scope.CoreItem.buyingqty;
                    $scope.PopupList = [];
                    $scope.CanClear = true;
                    $timeout(function () {
                        if ($scope.IsJump) {
                            $scope['setFocussizeInfo']();
                        } else {
                            $scope['setFocussizeInfoBit']();
                        }
                    }, 100);


                    $scope.finalpricechanged = function (item) {
                        var fp = $scope.ParentScope.GetNumber(item.finalprice);
                        item.salediscount = $scope.ParentScope.GetNumber(item.sellingprice) - fp;
                        if (item.salediscount != 0)
                            item.salediscountpercentage = (item.salediscount / item.sellingprice * 100).round(2);
                        else
                            item.salediscountpercentage = 0;
                    }

                    $scope.salesdiscountchanged = function (item) {
                        var sd = $scope.ParentScope.GetNumber(item.salediscount);
                        if (sd != 0)
                            item.salediscountpercentage = (item.salediscount / item.sellingprice * 100).round(2);
                        else
                            item.salediscountpercentage = 0;
                        item.finalprice = item.sellingprice - $scope.ParentScope.GetNumber(item.salediscount);
                    }

                    $scope.salesdiscountpercentagechanged = function (item) {
                        var sd = $scope.ParentScope.GetNumber(item.salediscountpercentage);
                        if (sd != 0)
                            item.salediscount = ((item.sellingprice * sd) / 100).round(2);
                        else
                            item.salediscount = 0;
                        item.finalprice = item.sellingprice - $scope.ParentScope.GetNumber(item.salediscount);
                    }

                    $scope.IncreasePrice = function () {
                        if ($scope.isHideIncrese) return;
                        $scope.isHideIncrese = true;
                        var priceincrements = $scope.priceincrement.split('-');

                        $.each($scope.PopupList, function (k, item) {
                            if ($scope.ParentScope.GetNumber(item.mrp) <= 0)
                                item.mrp = item.sellingprice;
                        });

                        if (priceincrements.length == 1) {
                            var start = 0;
                            var lastPrice = parseFloat($scope.PopupList[0].sellingprice);
                            $.each($scope.PopupList, function (k, item) {
                                var baseCost = (!$scope.ngDialogData.scope.SupportMarginOnCost ? item.buyingprice : item.basebuyingprice);
                                item.sellingprice = lastPrice;
                                item.sellingprice += start;
                                item.mrp = $scope.ParentScope.GetNumber(item.mrp) + $scope.ParentScope.GetNumber(start);
                                item.marginpercentage = ((item.sellingprice / baseCost * 100).round(2) - 100).round(2);
                                item.salediscount = (item.sellingprice * item.salediscountpercentage / 100).round(0);
                                start += parseFloat(priceincrements[0]);
                                item.finalprice = item.sellingprice - item.salediscount;
                            });
                        }
                        if (priceincrements.length == 2) {
                            var start = 0;
                            $.each($scope.PopupList, function (k, item) {
                                var baseCost = (!$scope.ngDialogData.scope.SupportMarginOnCost ? item.buyingprice : item.basebuyingprice);
                                if (item.sizename == priceincrements[0]) {
                                    start = parseFloat(priceincrements[1]);
                                } else if (start > 0) {
                                    item.sellingprice = parseFloat(item.sellingprice);
                                    item.sellingprice += start;
                                    item.mrp = $scope.ParentScope.GetNumber(item.mrp) + $scope.ParentScope.GetNumber(start);
                                    item.marginpercentage = ((item.sellingprice / baseCost * 100).round(2) - 100).round(2);
                                    item.salediscount = (item.sellingprice * item.salediscountpercentage / 100).round(0);
                                    start += parseFloat(priceincrements[1]);
                                    item.finalprice = item.sellingprice - item.salediscount;
                                }
                            });
                        } else if (priceincrements.length == 3) {
                            var start = 0;
                            var startindex = -1;
                            $.each($scope.PopupList, function (k, item) {
                                var baseCost = (!$scope.ngDialogData.scope.SupportMarginOnCost ? item.buyingprice : item.basebuyingprice);
                                if (item.sizename == priceincrements[1]) {
                                    start = parseFloat(priceincrements[2]);
                                    startindex = k;
                                } else if (start > 0) {
                                    item.sellingprice = parseFloat(item.sellingprice);
                                    item.sellingprice += start;
                                    item.mrp = $scope.ParentScope.GetNumber(item.mrp) + $scope.ParentScope.GetNumber(start);
                                    item.marginpercentage = ((item.sellingprice / baseCost * 100).round(2) - 100).round(2);
                                    item.salediscount = (item.sellingprice * item.salediscountpercentage / 100).round(0);
                                    start += parseFloat(priceincrements[2]);
                                    item.finalprice = item.sellingprice - item.salediscount;
                                }
                            });
                            if (startindex > 0) {
                                var start = 0;
                                for (var index = startindex - 1; index >= 0; index--) {
                                    start += parseFloat(priceincrements[0]);
                                    var item = $scope.PopupList[index];
                                    var baseCost = (!$scope.ngDialogData.scope.SupportMarginOnCost ? item.buyingprice : item.basebuyingprice);
                                    item.sellingprice = parseFloat(item.sellingprice);
                                    item.sellingprice -= start;
                                    item.mrp = $scope.ParentScope.GetNumber(item.mrp) - $scope.ParentScope.GetNumber(start);
                                    item.marginpercentage = ((item.sellingprice / baseCost * 100).round(2) - 100).round(2);
                                    item.salediscount = (item.sellingprice * item.salediscountpercentage / 100).round(0);
                                    item.finalprice = item.sellingprice - item.salediscount;
                                }
                            }
                        }

                        if ($scope.SupportRoundingDiscount) {
                            $.each($scope.PopupList, function (k, item) {
                                if (item.salediscount > 0) {
                                    var nfinalprice = $scope.ngDialogData.scope.RoundPrice(item.finalprice);
                                    if (item.finalprice != nfinalprice && item.finalprice < item.sellingprice) {
                                        item.finalprice = nfinalprice;
                                        item.salediscount = item.sellingprice - item.finalprice;
                                    }
                                }
                            });
                        }
                    }

                    $scope.CreateItems = function ($event) {
                        $scope.isHideIncrese = false;
                        if (!$scope.isSizeGroup &&
                            (typeof ($scope.sizeInfo) == 'undefined' || $scope.sizeInfo == null || $scope.sizeInfo.toString() == '')) {
                            if (!$scope.Selected()) {
                                if ($scope.PopupList.length > 0) {
                                    $scope.setFocusmcell_firstrow();
                                } else {
                                    if ($scope.IsJump) {
                                        $scope['setFocussizeInfo']();
                                    } else {
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
                                        'sizename': 'Piece',
                                        'sizeid': $scope.CoreItem.sizeid,
                                        'qty': meter,
                                        'uan': $scope.CoreItem.uan,
                                        'sizegroup': $scope.sizeInfo,
                                        'sellingprice': $scope.GetNumber($scope.CoreItem.sellingprice),
                                        'mrp': $scope.GetNumber($scope.CoreItem.mrp),
                                        'dealerprice': $scope.GetNumber($scope.CoreItem.dealerprice),
                                        'buyingprice': $scope.GetNumber($scope.CoreItem.buyingprice),
                                        'marginpercentage': $scope.GetNumber($scope.CoreItem.marginpercentage),
                                        'basebuyingprice': $scope.GetNumber($scope.CoreItem.basebuyingprice),
                                        'salediscount': $scope.GetNumber($scope.CoreItem.salediscount),
                                        'salediscountpercentage': $scope.GetNumber($scope.CoreItem.salediscountpercentage),
                                        'purchasediscountpercentage': $scope.GetNumber($scope.CoreItem.purchasediscountpercentage),
                                        'purchasediscount': $scope.GetNumber($scope.CoreItem.purchasediscount)
                                    }
                                    nitem.finalprice = nitem.sellingprice - nitem.salediscount;
                                    $scope.TotalAmount += (nitem.qty * nitem.buyingprice).round(2);

                                    if ($scope.PopupList.length == 0) {
                                        nitem.cellid = 'mcell_firstrow';
                                    }
                                    $scope.PopupList.push(nitem);
                                    nitem.cellid = 'mcell_' + (cellid++);
                                    $scope.Created += meter;
                                }
                                $scope.Created = $scope.Created.round(2);
                                $scope.sizeInfo = null; //($scope.CoreItem.buyingqty - $scope.Created).toString();
                            }
                            $scope.sizeInfo = null; //($scope.CoreItem.buyingqty - $scope.Created).toString();
                        } else {

                            var idList = [];
                            if ($scope.isSizeGroup) {

                                if ($scope.PopupList != 'undefined' && $scope.PopupList != null && $scope.PopupList.length > 0)
                                    return;
                                if (typeof ($scope.sizegroupid) == 'undefined' || $scope.sizegroupid == null)
                                    return;
                                var sizegroup = $scope.SizeGroupList.find(function (item) {
                                    return item.id == $scope.sizegroupid;
                                });
                                if (typeof (sizegroup) == 'undefined' || sizegroup == null)
                                    return;

                                $scope.sizeInfo = sizegroup.name;
                                $.each(sizegroup.list, function (k, v) {
                                    var size = $scope.ngDialogData.SizeList.find(function (item) {
                                        return item.id == v.id;
                                    });
                                    idList.push({
                                        'id': size.id,
                                        'name': size.name,
                                        'qty': 0,
                                        'index': k
                                    });
                                });

                            } else {
                                var sizeidList = $scope.sizeInfo.toString().split(',');
                                var sizeList = sizeidList[0].split('-');
                                if (sizeList.length == 2 && /^[a-zA-Z2-6]*$/.test(sizeList[0])) {

                                    var sizeIndex = '3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL'.split(',');
                                    var startsize = sizeIndex.indexOf(sizeList[0]);
                                    var endsize = sizeIndex.indexOf(sizeList[1]);
                                    if (startsize < 0 || endsize < 0) {
                                        sizeIndex = '3XS,XXS,XS,S,M,L,XL,2XL,3XL,4XL,5XL,6XL,7XL'.split(',');
                                        startsize = sizeIndex.indexOf(sizeList[0]);
                                        endsize = sizeIndex.indexOf(sizeList[1]);
                                    }
                                    sizeList = [];
                                    for (var index = startsize; index <= endsize; index++) {
                                        sizeList.push(sizeIndex[index]);
                                    }

                                } else if (sizeList.length >= 3) {

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

                                $.each($scope.ngDialogData.SizeList, function (k, v) {
                                    $.each(sizeList, function (kk, vv) {
                                        if (vv == v.name) {
                                            if (idList.find(function (item) {
                                                return item.name == v.name;
                                            }) == null) {
                                                idList.push({
                                                    'id': v.id,
                                                    'name': v.name,
                                                    'qty': 0,
                                                    'index': kk
                                                });
                                            }
                                        }
                                    });
                                });
                            }

                            if (idList.length > 0) {

                                idList.sort(function (a, b) {
                                    return a.index - b.index;
                                });

                                var qty = Math.floor(($scope.CoreItem.buyingqty - $scope.Created) / idList.length);
                                if (!isNaN(qty)) {
                                    $.each(idList, function (k, v) {
                                        var uan = $scope.CoreItem.uan;
                                        if ($scope.SupportUANPurchase &&
                                            $scope.ProductSearch.CurrentSKUList &&
                                            $scope.ProductSearch.CurrentSKUList != null &&
                                            $scope.ProductSearch.CurrentSKUList.length > 0) {
                                            var uanitem = $scope.ProductSearch.CurrentSKUList.find(function (sizeitem) {
                                                return sizeitem.sizeid == v.id;
                                            });
                                            if (uanitem && uanitem != null) {
                                                uan = uanitem.uid;
                                            }
                                        }

                                        var nitem = {
                                            'sizename': v.name,
                                            'sizeid': v.id,
                                            'qty': qty,
                                            'uan': uan,
                                            'sizegroup': $scope.sizeInfo,
                                            'sellingprice': $scope.GetNumber($scope.CoreItem.sellingprice),
                                            'mrp': $scope.GetNumber($scope.CoreItem.mrp),
                                            'dealerprice': $scope.GetNumber($scope.CoreItem.dealerprice),
                                            'buyingprice': $scope.GetNumber($scope.CoreItem.buyingprice),
                                            'marginpercentage': $scope.GetNumber($scope.CoreItem.marginpercentage),
                                            'basebuyingprice': $scope.GetNumber($scope.CoreItem.basebuyingprice),
                                            'salediscount': $scope.GetNumber($scope.CoreItem.salediscount),
                                            'salediscountpercentage': $scope.GetNumber($scope.CoreItem.salediscountpercentage),
                                            'purchasediscountpercentage': $scope.GetNumber($scope.CoreItem.purchasediscountpercentage),
                                            'purchasediscount': $scope.GetNumber($scope.CoreItem.purchasediscount)
                                        }
                                        nitem.finalprice = nitem.sellingprice - nitem.salediscount;
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

                            if (sizeidList != 'undefined' && sizeidList != null && sizeidList[0].split('-').length == 4) {
                                $scope.AllowChangePrice = true;
                                var index = Math.floor($scope.PopupList.length / 2);
                                $scope.priceincrement = sizeidList[0].split('-')[3] + '-' + $scope.PopupList[index].sizename + '-' + sizeidList[0].split('-')[3];
                                $scope.IncreasePrice();
                            }
                            $scope.sizeInfo = null;
                        }
                    };


                    $scope.GetNumber = function (value) {
                        if (typeof (value) == 'number')
                            return value;

                        if (typeof (value) == 'undefined' || value == '')
                            return 0;

                        nvalue = parseFloat(value);
                        if (isNaN(nvalue))
                            return 0;

                        return nvalue;
                    };


                    $scope.PriceChanged = function (item, newbasebuyingprice, oldbasebuyingprice) {
                        var oldvalue = parseInt(oldbasebuyingprice);
                        var newvalue = parseInt(newbasebuyingprice);

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

                        if (item.purchasetaxpercentage > 0 && $scope.ngDialogData.scope.Invoice.isinclusivetax) {
                            item.buyingprice = item.buyingprice / (100 + item.purchasetaxpercentage) * 100;
                        }

                        item.amount += (($scope.ngDialogData.scope.Invoice.isinclusivediscount ? $scope.GetNumber(item.basebuyingprice) : $scope.GetNumber(item.buyingprice)) *
                            $scope.GetNumber(item.buyingqty)).round(2);

                        var baseCost = $scope.GetNumber($scope.ngDialogData.scope.SupportMarginOnCost ? item.basebuyingprice : item.buyingprice);
                        item.sellingprice = baseCost + (baseCost * $scope.GetNumber(item.marginpercentage) / 100);

                        if ($scope.ngDialogData.scope.SupportMarginexcludetax) {
                            var newtaxvalue = $scope.ngDialogData.scope.GetNumber(item.sellingprice) * item.salestaxpercentage / 100;
                            var newsellingprice = $scope.ngDialogData.scope.RoundPrice(newtaxvalue + item.sellingprice);
                            var taxitem = $scope.ngDialogData.scope.CurrentPurchaseTaxConfig;
                            if (taxitem != null) {
                                if (taxitem != null && typeof (taxitem.taxsplit.findTax)) {
                                    taxitem.taxsplit.findTax = $scope.ngDialogData.scope.findTax;
                                }
                                var taxconfig = taxitem.taxsplit.findTax($scope.ngDialogData.scope.GetNumber(item.sellingprice));
                                if (item.salestaxpercentage < taxconfig.cgst + taxconfig.sgst) {
                                    var newtaxvalue = $scope.ngDialogData.scope.GetNumber(item.sellingprice) * (taxconfig.cgst + taxconfig.sgst) / 100;
                                    newsellingprice = $scope.ngDialogData.scope.RoundPrice(newtaxvalue + item.sellingprice);
                                }
                            }
                            item.sellingprice = newsellingprice;
                        } else {
                            item.sellingprice = $scope.ngDialogData.scope.RoundPrice(item.sellingprice);
                        }



                        item.sellingprice = $scope.ngDialogData.scope.RoundPrice(item.sellingprice);
                        item.salediscount = (item.sellingprice * item.salediscountpercentage / 100).round(0);
                        if (!$scope.SupportSalepriceMrpdifference)
                            item.mrp = item.sellingprice;
                        item.finalprice = item.sellingprice - item.salediscount;
                    }

                    $scope.MRPChanged = function (item) {
                        if ($scope.SupportSalepriceMrpdifference) return;

                        item.manualprice = true;
                        var value = $scope.GetNumber(item.mrp);
                        item.sellingprice = value;
                        if (typeof (item.buyingprice) == 'undefined' || item.buyingprice == null || isNaN(item.buyingprice)) {
                            item.buyingprice = item.basebuyingprice;
                            if (item.purchasetaxpercentage > 0 && $scope.Invoice.isinclusivetax) {
                                item.buyingprice = item.buyingprice / (100 + item.purchasetaxpercentage) * 100;
                            }
                        }
                        var baseCost = (!$scope.ngDialogData.scope.SupportMarginOnCost ? item.buyingprice : item.basebuyingprice);
                        item.marginpercentage = ((item.sellingprice / baseCost * 100).round(2) - 100).round(2);
                        item.salediscount = (item.sellingprice * item.salediscountpercentage / 100).round(0);
                        item.finalprice = item.sellingprice - item.salediscount;
                    }

                    $scope.SalePriceChanged = function (item) {
                        item.manualprice = true;
                        var value = $scope.GetNumber(item.sellingprice);
                        if (typeof (item.buyingprice) == 'undefined' || item.buyingprice == null || isNaN(item.buyingprice)) {
                            item.buyingprice = item.basebuyingprice;
                            if (item.purchasetaxpercentage > 0 && $scope.Invoice.isinclusivetax) {
                                item.buyingprice = item.buyingprice / (100 + item.purchasetaxpercentage) * 100;
                            }
                        }
                        var baseCost = (!$scope.ngDialogData.scope.SupportMarginOnCost ? item.buyingprice : item.basebuyingprice);

                        if ($scope.ngDialogData.scope.SupportMarginexcludetax) {
                            $scope.ComputeExcludeMargin(item);
                        } else {
                            $scope.ComputeIncludeMargin(item);
                        }

                        item.salediscount = (value * item.salediscountpercentage / 100).round(0);
                        item.finalprice = value - item.salediscount;
                        
                    }


                    $scope.ComputeIncludeMargin = function (ItemDetails) {

                        var buyingprice = $scope.ngDialogData.scope.SupportMarginOnCost ? $scope.GetNumber(ItemDetails.basebuyingprice) : $scope.GetNumber(ItemDetails.buyingprice);

                        if ($scope.ngDialogData.scope.SupportMarginincludetax) {
                            var newtaxvalue = buyingprice * (ItemDetails.purchasetaxpercentage) / 100;
                            buyingprice = newtaxvalue + buyingprice;
                        }

                        if ($scope.ngDialogData.scope.SupportAddlrexptomargion) {

                            if ($scope.ngDialogData.scope.Invoice.expenseperpiece > 0)
                                buyingprice = buyingprice + $scope.ngDialogData.scope.Invoice.expenseperpiece;

                            if ($scope.ngDialogData.scope.Invoice.expensepercentage > 0)
                                buyingprice = buyingprice + (buyingprice * $scope.ngDialogData.scope.Invoice.expensepercentage / 100).round(0);
                        }

                        if (typeof ($scope.ngDialogData.scope.Invoice.premargin) != 'undefined' && $scope.ngDialogData.scope.Invoice.premargin > 0) {
                            buyingprice += (buyingprice * $scope.ngDialogData.scope.Invoice.premargin / 100).round(0);
                        }

                        if (typeof ($scope.ItemDetails.rowexpense) != 'undefined' && ItemDetails.rowexpense > 0) {
                            buyingprice = buyingprice + (buyingprice * ItemDetails.rowexpense / 100).round(0);
                        }

                        var marginpercentage = (($scope.GetNumber(ItemDetails.sellingprice) / buyingprice * 100) - 100).round(2);

                        ItemDetails.marginpercentage = marginpercentage;
                        if ($scope.ngDialogData.scope.SupportMarkDOWN)
                            marginpercentage = (100 - ((buyingprice / ItemDetails.sellingprice) * 100)).round(2);

                        ItemDetails.marginpercentage = marginpercentage;
                        ItemDetails.rowexpensevalue = buyingprice - ItemDetails.buyingprice;
                    }


                    $scope.ComputeExcludeMargin = function (ItemDetails) {
                        var salestaxpercentage = ItemDetails.salestaxpercentage;


                        var taxitem = $scope.ngDialogData.scope.CurrentSalesTaxConfig;
                        if ($scope.ngDialogData.scope.SupportMarginexcludetax)
                            taxitem = $scope.ngDialogData.scope.CurrentPurchaseTaxConfig;
                        if (taxitem != null) {
                            var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber(ItemDetails.sellingprice));
                            salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
                        }

                        var buyingprice = $scope.ngDialogData.scope.SupportMarginOnCost ? $scope.GetNumber(ItemDetails.basebuyingprice) : $scope.GetNumber(ItemDetails.buyingprice);

                        if ($scope.ngDialogData.scope.SupportMarginincludetax) {
                            var newtaxvalue = buyingprice * (ItemDetails.purchasetaxpercentage) / 100;
                            buyingprice = newtaxvalue + buyingprice;
                        }

                        if ($scope.ngDialogData.scope.SupportAddlrexptomargion) {

                            if ($scope.ngDialogData.scope.Invoice.expenseperpiece > 0)
                                buyingprice = buyingprice + $scope.ngDialogData.scope.Invoice.expenseperpiece;

                            if ($scope.ngDialogData.scope.Invoice.expensepercentage > 0)
                                buyingprice = buyingprice + (buyingprice * $scope.ngDialogData.scope.Invoice.expensepercentage / 100).round(0);
                        }

                        if (typeof ($scope.ngDialogData.scope.Invoice.premargin) != 'undefined' && $scope.ngDialogData.scope.Invoice.premargin > 0) {
                            buyingprice += (buyingprice * $scope.ngDialogData.scope.Invoice.premargin / 100).round(0);
                        }

                        if (typeof (ItemDetails.rowexpense) != 'undefined' && ItemDetails.rowexpense > 0) {
                            buyingprice = buyingprice + (buyingprice * ItemDetails.rowexpense / 100).round(0);
                        }

                        var asellingprice = $scope.GetNumber(ItemDetails.sellingprice) / (salestaxpercentage + 100) * 100;

                        var marginpercentage = ((asellingprice - buyingprice) / buyingprice * 100).round(2);
                        if ($scope.ngDialogData.scope.SupportMarkDOWN)
                            marginpercentage = (100 - ((buyingprice / asellingprice) * 100)).round(2);


                        ItemDetails.marginpercentage = marginpercentage;
                        ItemDetails.rowexpensevalue = buyingprice - ItemDetails.buyingprice;
                    }

                    $scope.QtyChanged = function (item, newVal, oldVal) {

                        var oldvalue = parseFloat(oldVal);
                        var newvalue = parseFloat(newVal);

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
                        //$scope.PriceChanged(item);
                    };

                    $scope.Validate = function () {
                        $.each($scope.PopupList, function (k, item) {
                            if (item.purchasediscount == 0 && item.purchasediscountpercentage > 0) {
                                item.purchasediscount = ($scope.GetNumber(item.basebuyingprice) * item.purchasediscountpercentage / 100).round(2);
                                if (typeof (item.purchasediscount) == 'undefined' || item.purchasediscount == null || isNaN(item.purchasediscount))
                                    item.purchasediscount = 0;

                                item.buyingprice = item.basebuyingprice - item.purchasediscount;
                                if (item.purchasetaxpercentage > 0 && $scope.Invoice.isinclusivetax) {
                                    item.buyingprice = item.buyingprice / (100 + item.purchasetaxpercentage) * 100;
                                }

                            }
                        })
                    };

                    $scope.Delete = function (item) {
                        if (typeof (item.sellableqty) != 'undefined' && item.sellableqty > 0) {
                            $rootScope.Toaster.Error("Barcode already generated for this item, so can't delete");
                            return;
                        }
                        $scope.PopupList.splice($scope.PopupList.indexOf(item), 1);
                        $scope.TotalAmount -= (item.qty * item.buyingprice).round(2)
                        $scope.Created -= item.qty;
                        $scope.Created = $scope.Created.round(2);
                    };

                    $scope.Clear = function () {
                        $scope.PopupList = [];
                        $scope.Created = 0;
                        $scope.TotalAmount = 0;
                    }

                    $scope.Selected = function () {

                        if ($scope.Created != $scope.Total || $scope.Created == 'undefined' || $scope.Created == null) {
                            $rootScope.Toaster.Error("Check Qty..!");

                            return ($scope.Created == $scope.Total);
                        } else {
                            $scope.closeThisDialog({
                                'list': $scope.PopupList,
                                'stocklocationid': $scope.stocklocationid
                            });
                            return true;
                        }
                    };


                    if (typeof ($scope.CoreItem.Child) != 'undefined' &&
                        $scope.CoreItem.Child.length > 0) {
                        $.each($scope.CoreItem.Child, function (k, v) {
                            v.dealerprice = parseFloat($scope.CoreItem.dealerprice);
                            v.buyingqty = parseFloat(v.buyingqty);
                            v.qty = typeof (v.qty) == 'undefined' ? v.buyingqty : parseFloat(v.qty);
                            v.buyingprice = parseFloat(v.buyingprice);
                            v.basebuyingprice = parseFloat(v.basebuyingprice);
                            if (isNaN(v.qty)) v.qty = 0.0;
                            if (isNaN(v.buyingqty)) v.buyingqty = 0.0;
                            if (isNaN(v.buyingprice)) v.buyingprice = 0.0;
                            if (isNaN(v.basebuyingprice)) v.basebuyingprice = 0.0;

                            v.sizeid = v.Details.sizeid;
                            v.sizename = restService.SearchReference(v.sizeid, $scope.ngDialogData.SizeList);
                            v.Details.sizename = v.sizename;
                            v.finalprice = v.sellingprice - v.salediscount;
                            if (typeof (v.id) != 'undefined' && v.id > 0)
                                $scope.CanClear = false;
                            $scope.TotalAmount += (v.qty * v.buyingprice).round(2);
                            $scope.PopupList.push((JSON.parse(JSON.stringify(v))));
                            $scope.Created += v.buyingqty;
                            $scope.Created = $scope.Created.round(2);
                        });

                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var stocklocationid = 0;
                        if ($scope.CollectLocationInfo)
                            stocklocationid = data.value.stocklocationid;
                        var children = $scope.ItemDetails.Child;
                        $scope.ItemDetails.Child = [];
                        var currentPOInfo = [];
                        if ($scope.ItemDetails.poinfo != null && $scope.ItemDetails.poinfo.length > 0) {
                            $.each($scope.ItemDetails.poinfo, function (k, v) {
                                currentPOInfo.push(v);
                            });
                            $scope.ItemDetails.poinfo = [];
                        }
                        else if (children != null && children.length > 0) {
                            $.each(children, function (kk, vv) {
                                $.each(vv.poinfo, function (k, v) {
                                    currentPOInfo.push(v);
                                });
                                vv.poinfo = [];
                            });
                        }

                        var cloneDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
                        cloneDetails.id = 0;
                        cloneDetails.stocklocationid = stocklocationid;

                        $scope.ItemDetails = (JSON.parse(JSON.stringify(cloneDetails)));
                        $scope.ItemDetails.IsParent = true;
                        $scope.ItemDetails.IsChild = false;
                        $scope.ItemDetails.IsEditItem = false;
                        $scope.AddItemInternal(false);
                        var parent = $scope.ItemDetails;
                        parent.Child = [];
                        parent.amount = 0;
                        var taxitem = $scope.CurrentPurchaseTaxConfig;
                        var salestaxitem = $scope.CurrentSalesTaxConfig;
                        $.each(data.value.list, function (k, v) {
                            try {
                                $scope.ItemDetails = (JSON.parse(JSON.stringify(cloneDetails)));
                                $scope.ItemDetails.IsEditItem = false;
                                $scope.ItemDetails.Details.sizeid = v.sizeid;
                                $scope.ItemDetails.Details.sizegroup = v.sizegroup;
                                $scope.ItemDetails.buyingqty = $scope.GetNumber(v.qty);
                                $scope.ItemDetails.purchasediscountpercentage = $scope.GetNumber(v.purchasediscountpercentage);
                                $scope.ItemDetails.purchasediscount = $scope.GetNumber(v.purchasediscount);
                                $scope.ItemDetails.basebuyingprice = $scope.GetNumber(v.basebuyingprice);
                                $scope.ItemDetails.buyingprice = $scope.GetNumber(v.buyingprice);
                                $scope.ItemDetails.sellingprice = $scope.GetNumber(v.sellingprice);
                                $scope.ItemDetails.salediscount = $scope.GetNumber(v.salediscount);
                                $scope.ItemDetails.finalprice = $scope.ItemDetails.sellingprice - $scope.ItemDetails.salediscount;
                                $scope.ItemDetails.mrp = $scope.GetNumber(v.mrp);
                                $scope.ItemDetails.barcodes = v.barcodes;
                                $scope.ItemDetails.uan = v.uan;
                                $scope.ItemDetails.sellableqty = v.sellableqty;
                                $scope.ItemDetails.damagedqty = v.damagedqty;
                                $scope.ItemDetails.actualqty = v.actualqty;
                                $scope.ItemDetails.dealerprice = v.dealerprice;
                                $scope.ItemDetails.wsrate = v.wsrate;
                                $scope.ItemDetails.itemid = 0;

                                $scope.ItemDetails.id = v.id;
                                if (taxitem == null) {
                                    $scope.ItemDetails.purchasetaxpercentage = 0;
                                } else {
                                    var taxconfig = taxitem.taxsplit.findTax($scope.ItemDetails.buyingprice);
                                    $scope.ItemDetails.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
                                }

                                if (salestaxitem == null) {
                                    $scope.ItemDetails.salestaxpercentage = 0;
                                } else {
                                    var taxconfig = salestaxitem.taxsplit.findSalesTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                                    $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
                                }

                                $scope.ItemDetails.amount = $scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty);
                                $scope.ItemDetails.purchasetotaldiscount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty) * $scope.ItemDetails.purchasediscountpercentage / 100).round(2);
                                $scope.ItemDetails.amount -= $scope.ItemDetails.purchasetotaldiscount;
                                if ($scope.Invoice.IsUnregisteredInvoice)
                                    $scope.ItemDetails.purchasetax = 0;
                                else
                                    $scope.ItemDetails.purchasetax = ($scope.ItemDetails.amount * $scope.ItemDetails.purchasetaxpercentage / 100).round(2);
                                $scope.ItemDetails.purchasediscount = ($scope.ItemDetails.purchasetotaldiscount / $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);

                                $scope.ItemDetails.salediscountpercentage = $scope.GetNumber(v.salediscountpercentage);
                                $scope.ItemDetails.salestax = ($scope.GetNumber($scope.ItemDetails.buyingqty) * ($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount) / ($scope.ItemDetails.salestaxpercentage + 100) * $scope.ItemDetails.salestaxpercentage).round(2);
                                $scope.ItemDetails.IsChild = true;
                                $scope.ItemDetails.IsParent = false;

                                parent.amount += (($scope.Invoice.isinclusivediscount ? $scope.GetNumber($scope.ItemDetails.basebuyingprice) : $scope.GetNumber($scope.ItemDetails.buyingprice)) *
                                    $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);

                                parent.Child.push($scope.ItemDetails);
                                $scope.AddItemInternal(false);
                            } catch (e) {
                                e = null;
                            }
                        });
                        parent.amount = parent.amount.round(2);
                        $.each(parent.Child, function (k, v) {
                            var require = v.buyingqty;
                            v.poinfo = [];
                            var skippedList = [];
                            for (var i = 0; i < currentPOInfo.length && require > 0; i++) {
                                if (currentPOInfo[i].aqty > 0) {
                                    if (currentPOInfo[i].sizeid > 0 && v.Details.sizeid != currentPOInfo[i].sizeid) {
                                        skippedList.push(currentPOInfo[i])
                                        continue;
                                    }
                                    var newPOInfo = (JSON.parse(JSON.stringify(currentPOInfo[i])));
                                    if (currentPOInfo[i].aqty >= require) {
                                        newPOInfo.aqty = require;
                                    }
                                    v.poinfo.push(newPOInfo);
                                    currentPOInfo[i].aqty -= newPOInfo.aqty;
                                    require -= newPOInfo.aqty;
                                }
                            }
                            if (skippedList.length > 0 && require > 0) {
                                for (var i = 0; i < skippedList.length && require > 0; i++) {
                                    if (skippedList[i].aqty > 0) {
                                        var newPOInfo = (JSON.parse(JSON.stringify(skippedList[i])));
                                        if (skippedList[i].aqty >= require) {
                                            newPOInfo.aqty = require;
                                        }
                                        v.poinfo.push(newPOInfo);
                                        skippedList[i].aqty -= newPOInfo.aqty;
                                        require -= newPOInfo.aqty;
                                    }
                                }
                            }
                        });
                        $scope.CalculateTotal();
                        $scope.ItemDetails = (JSON.parse(JSON.stringify(cloneDetails)));
                        $scope.ItemDetails.IsEditItem = false;
                        $scope.CreateNew();
                    }
                }
            });
        };

        if (typeof ($location.search().id) != 'undefined' &&
            $location.search().id.length > 0) {
            request = {
                "Item": {
                    'lrid': $location.search().id
                },
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
                } else {
                    $scope.SearchResult = result;
                }
            }).error(function (result) { });
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

        $scope.SNOFormater = function (row, cell, value, columnDef, dataContext) {
            var spacer = "<span style='display:inline-block;height:1px;width:15px'></span>";
            var idx = columnDef.dataGrid.dataView.getIdxById(dataContext.id);
            if (dataContext.IsParent) {
                if (dataContext._collapsed) {
                    return spacer + " <span class='toggle expand'></span>&nbsp;" + value;
                } else {
                    return spacer + " <span class='toggle collapse'></span>&nbsp;" + value;
                }
            } else {
                return spacer + " <span class='toggle'></span>&nbsp;" + value;
            }
        };

        $scope.EntryGridConfig = function () {
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('gridList', '450px', '100%', true, false, true, 120);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);
            var column = igrid.AddCustomColumn('Name', 'Product', 120, function (item, col) {
                return $scope.SearchProductRef(item);
            });
            if ($scope.system_invcol_brand) {
                var column = igrid.AddCustomColumn('brandid', 'Brand', 70, function (item, col) {
                    return $scope.SearchRef(item.Details.brandid, $scope.BrandName);
                });
            }
            if ($scope.system_invcol_style) {
                var column = igrid.AddCustomColumn('styleid', 'Style', 70, function (item, col) {
                    return $scope.SearchRef(item.Details.styleid, $scope.StyleList);
                });
            }
            if ($scope.system_invcol_type) {
                var column = igrid.AddCustomColumn('typeid', 'Type', 100, function (item, col) {
                    return $scope.SearchRef(item.Details.typeid, $scope.TypeList);
                });
            }
            if ($scope.system_invcol_size) {
                var column = igrid.AddCustomColumn('sizeid', 'Size', 70, function (item, col) {
                    if (item.Details.sizeid < 0 && item.IsParent && item.Child && item.Child.length > 0) {
                        return $scope.SearchRef(item.Child[0].Details.sizeid, $scope.SizeList) + "-" +
                            $scope.SearchRef(item.Child[item.Child.length - 1].Details.sizeid, $scope.SizeList);
                    }
                    return $scope.SearchRef(item.Details.sizeid, $scope.SizeList);
                });
            }
            if ($scope.system_invcol_colour) {
                var column = igrid.AddCustomColumn('colourid', 'Colour', 70, function (item, col) {
                    return $scope.SearchRef(item.Details.colourid, $scope.ColorList);
                });
            }

            var column = igrid.AddColumn({
                id: "designid",
                name: "Design Id",
                field: "designid",
                width: 80,
                cssClass: "cell-title",
                editor: Slick.Editors.Text,
                'sortable': true
            });
            var column = igrid.AddColumn({
                id: "hsncode",
                name: "HSN",
                field: "hsncode",
                width: 60,
                cssClass: "cell-title",
                editor: Slick.Editors.Text,
                'sortable': true
            });



            if ($scope.system_supportskudesignpurchase) {
                var column = igrid.AddSimpleColumn('UAN', 'UAN', 120);
                column.editor = Slick.Editors.Text;
                column.getValue = function (item, col) {
                    if (item.Details && item.Details != null) {
                        return item.Details.serialno;
                    }
                    return '-';
                };
                column.formatter = function (row, cell, searchValue, columnDef, dataContext) {
                    if (dataContext.Details && dataContext.Details != null) {
                        dataContext['UAN'] = dataContext.Details.serialno;
                        return dataContext.Details.serialno;
                    }
                    return '-';
                }
                var column = igrid.AddCustomColumn('sizeid', 'Size', 50, function (item, col) {
                    return $scope.SearchRefDirect(item.Details.sizeid, $scope.SizeList);
                });
            }

            var column = igrid.AddColumn({
                id: "buyingqty",
                name: "Qty",
                field: "buyingqty",
                width: 60,
                cssClass: "cell-title",
                editor: Slick.Editors.Text,
                'sortable': true
            });
            column.cssClass = 'rightalign';

            if (!$scope.system_hidecostinventory) {
                igrid.AddFormatedColumn('basebuyingprice', 'Cost', 60, 'N2', 2);
                igrid.AddFormatedColumn('purchasediscount', 'P.Dis', 50, 'N2', 2);
                igrid.AddFormatedColumn('marginpercentage', 'Margin%', 50, 'N2', 2);

                var column = igrid.AddCustomColumn('Margin', 'Margin', 50, function (item, col) {
                    return $filter('IndianNumber')(item.sellingprice - item.salediscount - (item.buyingprice + $scope.GetNumber(item.rowexpensevalue)), 2);
                });
                column.cssClass = 'rightalign';
                if ($scope.system_invcol_txpercent) {
                    igrid.AddFormatedColumn('purchasetaxpercentage', 'Tax %', 50, 'N2', 2).cssClass = 'rightalign';
                }
            }

            igrid.AddFormatedColumn('mrp', 'MRP', 60, 'N2', 2).total = true;
            if ($scope.system_hidecostinventory) {
                var column = igrid.AddCustomColumn('price', 'Price', 60, function (item, col) {
                    return $filter('IndianNumber')(item.sellingprice);
                });
                igrid.AddFormatedColumn('salediscountpercentage', 'Dis%', 60, 'N2', 2).total = true;
                column.cssClass = 'rightalign';
                var column = igrid.AddCustomColumn('salediscount', 'Discount', 60, function (item, col) {
                    return $filter('IndianNumber')(item.salediscount);
                });
                column.cssClass = 'rightalign';
                var column = igrid.AddCustomColumn('price', 'Price', 60, function (item, col) {
                    return $filter('IndianNumber')(item.sellingprice - item.salediscount, 2);
                });
                column.cssClass = 'rightalign';
            }
            else {
                var column = igrid.AddCustomColumn('price', 'Price', 60, function (item, col) {
                    return $filter('IndianNumber')(item.sellingprice - item.salediscount, 2);
                });
                column.cssClass = 'rightalign';
            }

            if ($scope.system_hidecostinventory) {
                igrid.AddFormatedColumn('dealerprice', 'D.Price', 60, 'N2', 2);
            }

            else if ($scope.SupportDealerPrice) {
                igrid.AddFormatedColumn('dealerprice', 'D.Price', 60, 'N2', 2);
                var column = igrid.AddCustomColumn('DMargin', 'D.Margin', 50, function (item, col) {
                    return $filter('IndianNumber')(item.dealerprice - (item.buyingprice + $scope.GetNumber(item.rowexpensevalue)), 2);
                });
                column.cssClass = 'rightalign';
            }


            if ($scope.system_invcol_pricetype &&
                $scope.PriceTags.length > 0) {
                $.each($scope.PriceTags, function (k, v) {
                    column = igrid.AddFormatedColumn('pt_' + v.id, v.name, 60, 'N2', 2);
                    column.editor = Slick.Editors.Text;
                    column.cssClass = 'rightalign';
                    column.internal = v;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        if (item.Details.pricelist && item.Details.pricelist.length > 0) {
                            var value = item.Details.pricelist.find(function (item) {
                                return item.id == col.internal.id;
                            });
                            if (typeof (value) == 'undefined' || value == null)
                                return 0;
                            return value.price;
                        }
                        return 0;
                    };
                    column.formatter = function (row, cell, searchValue, columnDef, dataContext) {
                        if (dataContext.Details.pricelist && dataContext.Details.pricelist.length > 0) {
                            var value = dataContext.Details.pricelist.find(function (item) {
                                return item.id == columnDef.internal.id;
                            });
                            if (typeof (value) == 'undefined' || value == null)
                                return 0;
                            return value.price;
                        }
                        return 0;
                    }
                });
            }

            if ($scope.system_invcol_pricetag) {
                var column = igrid.AddColumn(
                    {
                        id: "pricetagid", name: "Tag", field: "pricetagid",
                        width: 80, cssClass: "cell-title", editor: Slick.Editors.Select2Editor,
                        'sortable': true, formatter: function (row, cell, searchValue, columnDef, dataContext) {
                            //if (typeof (searchValue) != 'undefined') {
                            //    var found = columnDef.dataSource.find(function (arritem) {
                            //        return arritem.id == searchValue;
                            //    });
                            //    if (typeof (found) != 'undefined' && found != null) {
                            //        //dataContext.Details.pricetagid = found.id;
                            //        //if (dataContext.IsParent && dataContext.Child && dataContext.Child != null) {
                            //        //    $.each(dataContext.Child, function (k, v) {
                            //        //        v.Details.pricetagid = found.id;
                            //        //    });
                            //        //}
                            //        return found.name;
                            //    }
                            //}
                            //else 
                            if (columnDef.dataSource && columnDef.dataSource != null) {
                                if (dataContext.pricetagid == undefined ||
                                    dataContext.pricetagid == 0) {
                                    if (dataContext.IsParent && dataContext.Child && dataContext.Child != null) {
                                        dataContext.pricetagid = dataContext.Child[0].Details.pricetagid
                                    }
                                    else {
                                        dataContext.pricetagid = dataContext.Details.pricetagid;
                                    }
                                    var found = columnDef.dataSource.find(function (arritem) {
                                        return arritem.id == dataContext.pricetagid;
                                    });
                                    if (typeof (found) != 'undefined' && found != null)
                                        return found.name;
                                } else {
                                    var found = columnDef.dataSource.find(function (arritem) {
                                        return arritem.id == dataContext.pricetagid;
                                    });
                                    if (typeof (found) != 'undefined' && found != null)
                                        return found.name;
                                }
                            }
                            return 'None';
                        }
                    }
                );
                $scope.PriceTagColumn = column;
                column.dataSource = $scope.PriceTagList;
                column.onselectchange = function (selectedvalue, item,
                    colunm, grid) {
                    item.Details.pricetagid = selectedvalue;
                    item.pricetagid = selectedvalue;

                    if (item.IsParent && item.Child && item.Child != null) {
                        $.each(item.Child, function (k, v) {
                            v.Details.pricetagid = selectedvalue;
                            v.pricetagid = selectedvalue;
                        });
                    }
                };
            }

            if (!$scope.system_hidecostinventory) {
                igrid.AddFormatedColumn('amount', 'Amount', 80, 'N2', 2).total = true;
            }

            if (!$scope.system_hidecostinventory) {
                igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            }
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-info btn-xs'><i class='fa fa-pencil' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.AddButtonColumn('View', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='View' class='btn btn-info btn-xs'><i class='fa fa-plus-circle' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.AddCustomColumn('ChooseLocation', '', 35, function (item) {
                if (item.stocklocationid > 0)
                    return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='ChooseLocation' class='btn btn-success btn-xs'><i class='fa fa-map-marker' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
                return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='ChooseLocation' class='btn btn-light btn-xs'><i class='fa fa-map-marker' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
            });

            igrid.AddCustomColumn('AllocatePOItems', '', 35, function (item) {
                if (item.poinfo && item.poinfo != null && item.poinfo.length > 0)
                    return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='ChooseLocation' class='btn btn-success btn-xs'><i class='fa fa-envelope-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
                return "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='ChooseLocation' class='btn btn-light btn-xs'><i class='fa fa-envelope-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>";
            });


            igrid.options.autoEdit = true;
            igrid.options.showHeaderRow = true;
            igrid.filter = function (item, args) {
                return !(typeof (item.IsChild) != 'undefined' && item.IsChild);
            }
            igrid.Initialize();
            $scope.egrid = igrid;
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;

                $timeout(function () {
                    if (command == 'Delete' || command == 'Edit' || command == 'View') {
                        $scope.ManageSearchItem(command, item);
                        $scope.egrid.UpdateData($scope.Invoice.Lrinvoiceitems, '_id');
                    } else if (command == 'ChooseLocation')
                        $scope.ChooseLocation(item, function (value) {
                            igrid.grid.invalidateRow(cell.row);
                            igrid.grid.render();
                        });
                    else if (command == 'AllocatePOItems') {
                        $scope.AllocatePOItems(item, true, function () {
                            igrid.grid.invalidateRow(cell.row);
                            igrid.grid.render();
                        });
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
                }
                else if (args.grid.getColumns()[args.cell].id == 'pricetagid') {
                    //item.Details.serialno = item[args.grid.getColumns()[args.cell].id];
                    var selectedvalue = item[args.grid.getColumns()[args.cell].id];
                    item.Details.pricetagid = selectedvalue;
                    item.pricetagid = selectedvalue;

                    if (item.IsParent && item.Child && item.Child != null) {
                        $.each(item.Child, function (k, v) {
                            v.Details.pricetagid = selectedvalue;
                            v.pricetagid = selectedvalue;
                        });
                    }

                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
                else if (args.grid.getColumns()[args.cell].id.startsWith('pt_')) {

                    if (typeof (item.Details.pricelist) == 'undefined' || item.Details.pricelist == null) {
                        item.Details.pricelist = [];
                        $.each($scope.PriceTags, function (k, v) {
                            item.Details.pricelist.push({ 'id': v.id, 'price': item.Details.sellingprice });
                        })
                    }

                    var pricetagid = args.grid.getColumns()[args.cell].internal.id;
                    var priceitem = item.Details.pricelist.find(function (si) {
                        return si.id == pricetagid
                    });
                    priceitem.price = item[args.grid.getColumns()[args.cell].id];

                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }
            });

            $scope.egrid.UpdateData([]);
        }

        $scope.LoadingRounding = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadingRounding, 100);
            } else {
                if ($rootScope.UserInfo.appsettings.system_editfinalprice || false)
                    $scope.SupportFinalPriceChange = false;
                $scope.HideSupplier = UserDetails.Info.p_HIDESUPPLIER.view;

                $scope.SupportUANPurchase = $rootScope.UserInfo.appsettings.system_uanpurchase || false;
                $scope.SupportAddlrexptomargion = $rootScope.UserInfo.appsettings.system_addlrexptomargion || false;
                $scope.SupportPOMandatory = $rootScope.UserInfo.appsettings.system_pomandatory || false;
                $scope.SupportRounding = $rootScope.UserInfo.appsettings.system_supportrounding || false;
                $scope.PurchaseRounding = $rootScope.UserInfo.appsettings.system_purchaserounding || false;
                $scope.system_supportpricingflow = $rootScope.UserInfo.appsettings.system_supportpricingflow || false;
                $scope.system_supportpricingflowproducts = $rootScope.UserInfo.appsettings.system_supportpricingflowproducts || false;
                $scope.SupportDesignList = $rootScope.UserInfo.appsettings.system_disabledesigneditable || false;

                if ($scope.SupportRounding && $rootScope.UserInfo.appsettings.system_purchaserounding && $rootScope.UserInfo.appsettings.system_purchaserounding == 11 &&
                    $rootScope.UserInfo.appsettings.system_purchasecustomrounding && $rootScope.UserInfo.appsettings.system_purchasecustomrounding != null) {
                    $scope.PurchaseRoundingConfig = [];
                    $.each($rootScope.UserInfo.appsettings.system_purchasecustomrounding.split(','), function (k, v) {
                        var configparts = v.split('=');
                        if (configparts.length == 2) {
                            $scope.PurchaseRoundingConfig.push({ 'end': parseFloat(configparts[1]), 'round': parseInt(configparts[0]) });
                        }
                    });
                    $scope.PurchaseRoundingConfig.sort(function (a, b) {
                        return a.end - b.end;
                    });
                    var start = 0;
                    $.each($scope.PurchaseRoundingConfig, function (k, v) {
                        v.start = start;
                        start = v.end;
                    });
                }

                $scope.SupportDiscountOnCost = $rootScope.UserInfo.appsettings.system_discountoncost || false;
                $scope.SupportMarginOnCost = $rootScope.UserInfo.appsettings.system_marginoncost || false;
                $scope.SupportMarkDOWN = $rootScope.UserInfo.appsettings.system_marginonmarkup || $rootScope.UserInfo.appsettings.system_marginonmarkdown || false;
                $scope.SupportAdditionalOnCost = $rootScope.UserInfo.appsettings.system_costwithadditional || false;
                $scope.SupportLocationBased = $rootScope.UserInfo.appsettings.system_locationbasedbarcodes || false;
                $scope.SupportMarginexcludetax = $rootScope.UserInfo.appsettings.system_marginexcludetax || false;
                $scope.SupportMarginincludetax = $rootScope.UserInfo.appsettings.system_marginincludetax || false;

                $scope.SupportDealerPrice = $rootScope.UserInfo.appsettings.system_dealerprice || false;
                $scope.SupportDealerPriceWOTax = $rootScope.UserInfo.appsettings.system_dealerpricewotax || false;
                $scope.SupportImageRecording = $rootScope.UserInfo.appsettings.system_supportimagerecord || false;
                $scope.system_invcol_colour = $rootScope.UserInfo.appsettings.system_invcol_colour || false;
                $scope.system_invcol_size = $rootScope.UserInfo.appsettings.system_invcol_size || false;
                $scope.system_invcol_brand = $rootScope.UserInfo.appsettings.system_invcol_brand || false;
                $scope.system_invcol_style = $rootScope.UserInfo.appsettings.system_invcol_style || false;
                $scope.system_invcol_type = $rootScope.UserInfo.appsettings.system_invcol_type || false;
                $scope.system_invcol_expmargin = $rootScope.UserInfo.appsettings.system_invcol_expmargin || false;
                $scope.system_invcol_dppercent = $rootScope.UserInfo.appsettings.system_invcol_dppercent || false;
                $scope.system_invcol_txpercent = $rootScope.UserInfo.appsettings.system_invcol_txpercent || false;
                $scope.system_invcol_pricetag = $rootScope.UserInfo.appsettings.system_invcol_pricetag || false;
                $scope.system_invcol_pricetype = $rootScope.UserInfo.appsettings.system_invcol_pricetype || false;
                $scope.system_marginpercentagezero = $rootScope.UserInfo.appsettings.system_marginpercentagezero || false;
                $scope.system_designidautoid = $rootScope.UserInfo.appsettings.system_designidautoid || false;
                $scope.system_hidecostinventory = $rootScope.UserInfo.appsettings.system_hidecostinventory || false;
                $scope.system_supportskudesignpurchase = $rootScope.UserInfo.appsettings.system_supportskudesignpurchase || false;
                $scope.SupportItemwiseEntry = $rootScope.UserInfo.appsettings.system_supportitemwiseentry || false;

                if (false || $scope.UserInfo.appsettings.system_b2brounding)
                    $scope.System_b2brounding = 4;

                $scope.SupportSalepriceMrpdifference = $rootScope.UserInfo.appsettings.system_salepricemrpdifference || false;
                $scope.CollectLocationInfo = $scope.SupportLocationBased;
                $scope.SupportAttributeFilter = $rootScope.UserInfo.appsettings.system_supportattributefilter || false;
                $scope.SupportRoundingDiscount = $rootScope.UserInfo.appsettings.system_supportroundingdiscount || false;

                if ($scope.SupportRoundingDiscount) {
                    $scope.SupportFinalPriceChange = false;
                }

                $scope.system_customfields = $rootScope.UserInfo.appsettings.system_customfields || false;
                if ($scope.system_invcol_pricetype) {
                    $scope.PriceTags = [];
                    restService.Get('ReferencelistService/PRICETAGS', true).then(function (results) {
                        $.each(results, function (k, v) {
                            if (v.additionalinfo.billdiscountinclude)
                                $scope.PriceTags.push(v);
                        });
                        if ($scope.PriceTags.length > 0)
                            $scope.EntryGridConfig();
                    });
                }

                if ($scope.system_customfields) {
                    restService.Get('ReferencelistService/FULL-CUSTOMFIELDS', true).then(function (results) {
                        var fields = [];
                        $scope.ItemTemplate.CustomFields = [];
                        $scope.ItemDetails.CustomFields = [];
                        $.each(results, function (k, v) {

                            if (v.additionalinfo.customfield.cfbuilt) {
                                $scope.ItemTemplate.CustomFields.push({
                                    'key': v.additionalinfo.customfield.cfname,
                                    'value': '',
                                });
                                $scope.ItemDetails.CustomFields.push({
                                    'key': v.additionalinfo.customfield.cfname,
                                    'value': '',
                                });
                                var fieldname = 'ItemDetails.CustomFields[' + k + '].value';
                                var topdiv = '<div class="col-md-6 no-padding" ng-show="!Mandatory.attributefilter || Mandatory.attributes.' + v.additionalinfo.customfield.cfname + '">';
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
                                        fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in ALLColorList track by item.id"></select></div>');
                                        break;
                                    case 6:
                                        fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in ALLSizeList track by item.id"></select></div>');
                                        break;
                                    case 7:
                                        fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in FitList track by item.id"></select></div>');
                                        break;
                                    case 8:
                                        fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in ALLTypeList track by item.id"></select></div>');
                                        break;
                                    case 9:
                                        fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in ALLMaterialList track by item.id"></select></div>');
                                        break;
                                    case 10:
                                        fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in ALLStyleList track by item.id"></select></div>');
                                        break;
                                    case 11:
                                        fields.push(topdiv + '<label>' + $('<div/>').text(v.name).html() + '</label>');
                                        fields.push('<select class="form-control" select2="" style="width:100%;" ng-model="' + fieldname + '" options="item.id as item.name for item in ALLPatternList track by item.id"></select></div>');
                                        break;
                                }
                            }
                        });
                        angular.element($("#customefields")).append($compile(fields.join(''))($scope));
                    });
                }
            }
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == "lrinvoice")
                subtitle = 'INVOICE ENTRY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy');

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

        $scope.LoadingRounding();

        $scope.UpdateCustomReferenceList = function (referenceitem, scopename) {
            restService.Get('ReferencelistService/' + referenceitem.code, true).then(function (results) {
                $scope[scopename] = results;
            });
        }
    }
]);