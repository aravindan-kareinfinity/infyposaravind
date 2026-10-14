var app = angular.module('InfinityLab');

app.controller('JobOrderController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, cntxService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.SetMode('SEARCH');
        $scope.LocalContext = cntxService.CreatePageContext('lrItemDetailss');
        $scope.AutoCalculation = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = true;
        $scope.EnableReadonly = false;
        $scope.showtax = false;
        $scope.SaveRequest = false;
        $scope.SearchResult = [];
        $scope.ErrorMessage = '';
        $scope.JobOrderTaxItem = {};
        $scope.additionaldiscountpercentage = 0;
        $scope.Total = {
            "buyingqty": 0,
            "mrp": 0,
            "sellingprice": 0,
            "buyingprice": 0,
            "tax": 0,
            "buyingpiece": 0
        };

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }
        $scope.ShowTax = function () {
            $scope.showtax = !$scope.showtax;
        }
        $scope.SearchCondition = {
            companyid: 0,
            supplierid: 0,
            JobOrderno: "",
            addedfrom: null,
            addedto: null,
            lrentryno: ""
        }

        $scope.WarehouseLocations = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WarehouseLocations = results;
        });

        $scope.Transport = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transport = results;
        });

        $scope.JobCharges = restService.Get('ReferencelistService/FULL-JOBCHARGE', true).then(function (results) {
            $scope.JobCharges = results;
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

            $scope.PurchaseTax.push({
                'id': -1, 'name': 'Rounding', 'taxpercentage': 'Rounding', 'type': 9
            });

            $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
                $.each(results, function (k, v) {

                    var taxitem = $scope.purchaseTaxActual.find(function (item) {
                        return item.id == v.referenceid;
                    });

                    var taxitem = (JSON.parse(JSON.stringify(taxitem)));
                    taxitem.name = v.name + ' @ ' + taxitem.taxpercentage + '%';
                    taxitem.code = 'Ad_' + taxitem.id + '_' + 1;
                    if (taxitem.taxpercentage > 0)
                        taxitem.taxsplit = { 'items': [{ 'cgst': taxitem.taxpercentage / 2, 'sgst': taxitem.taxpercentage / 2 }] };
                    else
                        taxitem.taxsplit = { 'items': [{ 'cgst': 0, 'sgst': 0 }] };
                    taxitem.type = 8;
                    $scope.PurchaseTax.push(taxitem);
                });

            });
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER/1', true).then(function (results) {
            $scope.SupplierList = results;
        });
        $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY/1', true).then(function (results) {
            $scope.SupplierCompany = results;
        });

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.BOMAllList = restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMAllList = results;
        });

        $scope.BOMCategories = restService.Get('ReferencelistService/FULL-BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategories = results;
        });

        $scope.JobReasons = restService.Get('ReferencelistService/MFRJOBREASON', true).then(function (results) {
            $scope.JobReasons = results;
        });

        $scope.ColourList = [];
        restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.AllColorList = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });

        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });


        $scope.ItemInTypes = [{ id: 1, name: 'Input BOM' }, { id: 2, name: 'Input Product' }];

        $scope.ItemOutTypes = [{ id: 3, name: 'Output BOM' },
        { id: 4, name: 'Output Product' },
        { id: 5, name: 'Return BOM' },
        { id: 6, name: 'Return Product' }];

        $scope.MaterialType = [{ id: 1, name: 'Component' }, { id: 2, name: 'Product' }];

        $scope.StatusList = [{ id: 1, name: 'Created' },
        { id: 2, name: 'Outward' },
        { id: 3, name: 'Progress' },
        { id: 8, name: 'Cancelled' },
        { id: 9, name: 'Completed' }];

        $scope.ItemTemplate = {
            "oqty": null,
            "sellingprice": null,
            "cost": null,
            "tax": 0,
            "taxpercentage": 0,
            "taxid": null,
            "isactive": true,
            "sno": 1,
        }

        $scope.JobOrderTemplate = {
            "companyid": 0, "JobOrderno": null, "JobOrderdate": new Date(),
            "inwardeddate": new Date(),
            "expectedate": new Date(), "noofpieces": 0, "noofbundle": 0, "baseamount": 0.0, "itemid": 0,
            "grossamount": 0.0, "totalamount": 0.0, "lrid": 0, "id": 0, "otheramount": 0.0, "modifiedon": "", "createdon": "",
            "createdby": 0, "modifiedby": 0, "version": 1, "suppliercompanyid": 0, "isactive": true, "inwardjob": false,
            "supportexpqty": true, bomJobOrdertaxitems: [], uniqueid: 0, Mfgjoborderitems: []
        }

        $scope.LastPrice = null;
        $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
        $scope.JobOrder = (JSON.parse(JSON.stringify($scope.JobOrderTemplate)));

        $scope.categoryChange = function () {
            if (typeof ($scope.ItemDetails.bomcategoryid) != 'undefined' && $scope.ItemDetails.bomcategoryid != null) {
                var BOMList = [];
                $.each($scope.BOMAllList, function (k, v) {
                    if (v.categoryid == $scope.ItemDetails.bomcategoryid)
                        BOMList.push(v);
                });
                $scope.BOMList = BOMList;
            }
        }

        $scope.bomChanged = function () {
            if ($scope.ItemDetails.bomid != null) {
                var prditem = $scope.BOMAllList.find(function (item) {
                    return (item.id == $scope.ItemDetails.bomid);
                });
                if (prditem || prditem != null) {
                    $scope.ItemDetails.taxid = prditem.taxid;
                    $scope.SetColourList(prditem);
                }
            };
        }

        $scope.focusColour = function () {
            $timeout(function () {
                $scope.setFocuscolourid();
            }, 20);
        }

        $scope.colourChanged = function () {
            if ($scope.ItemDetails.colourid != null) {
                var prditem = $scope.ColourList.find(function (item) {
                    return (item.id == $scope.ItemDetails.colourid);
                });
                if (prditem && prditem != null) {
                    $scope.ItemDetails.bominwardcode = prditem.bominwardcode;
                }
            }
        }

        $scope.SetColourList = function (prditem) {
            $scope.ItemDetails.colourid = null;
            $scope.ItemDetails.bominwardcode = '';
            if (prditem.colourmode == 2) {
                $scope.ColourList = [];
                $.each(prditem.colourlist, function (k, v) {
                    var colour = $scope.AllColorList.find(function (item) {
                        return item.id == v.id;
                    });
                    if (colour && colour.id > 0) {
                        colour.bominwardcode = v.name;
                        colour.imageid = v.imageid;
                        $scope.ColourList.push(colour);
                    }
                });
            } else if (prditem.colourmode == 1) {
                $scope.ColourList = [];
                var colour = $scope.AllColorList.find(function (item) {
                    return item.id == prditem.colorid;
                });
                if (colour && colour.id > 0) {
                    colour.bominwardcode = v.name;
                    colour.imageid = v.imageid;
                    $scope.ColourList.push(colour);
                }
            }
            else {
                $scope.ColourList = [];
                $scope.ItemDetails.bominwardcode = prditem.code;
                $scope.ItemDetails.imageid = prditem.imageid;
            }
        }


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

        $scope.AddItem = function (from) {

            if (typeof ($scope.JobOrder.Mfgjoborderitems) == 'undefined')
                $scope.JobOrder.Mfgjoborderitems = [];

            var duplicatesno = $scope.JobOrder.Mfgjoborderitems.find(function (item) {
                return item.sno == $scope.ItemDetails.sno;
            });

            if (typeof (duplicatesno) != 'undefined' && duplicatesno != null) {
                $rootScope.ShowAlert('Please enter different serial number', 'Duplicate serial no');
                return;
            };

            var bomcategory = $scope.BOMCategories.find(function (item) {
                return item.id == $scope.ItemDetails.bomcategoryid;
            });

            $scope.ItemDetails.isfabric = bomcategory.additionalinfo.parentid == 1;
            $scope.ItemDetails.bomname = $scope.SearchRef($scope.ItemDetails.bomid, $scope.BOMAllList);
            if ($scope.ItemDetails.isfabric) {
                if (!$scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == 1) {
                    var existitem = $scope.JobOrder.mfgworkorder.Mfgworkorderitems.find(function (item) {
                        return item.fabricid == $scope.ItemDetails.bomid &&
                            item.colourid == $scope.ItemDetails.colourid;
                    });
                    if (existitem && existitem != null) {
                        $scope.ItemDetails.isbom = true;
                        $scope.ItemDetails.mfgworkorderitemsid = existitem.id;
                    }
                }
            }
            $scope.AddItemInternal(true);
            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
            $scope.ItemDetails.sno = $scope.JobOrder.Mfgjoborderitems[$scope.JobOrder.Mfgjoborderitems.length - 1].sno + 1;
            $scope.ItemDetails.bominvoiceitemimage = '';
            $scope.ItemDetails.bominwardcode = '';
            $scope.ItemDetails.materialname = '';
            $scope.ItemDetails.cost = null;
            $scope.ItemDetails.oqty = null;
            $scope.ItemDetails.sellingprice = null;
            if (from == 'right') {
                if ($scope.ItemDetails.isbom) {
                    $scope.setFocusbominwardcode();
                }
                else
                    $scope.setFocusproductid();
            }
            else if ($scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == -1) {
                $scope.setFocusbomcategoryid();

            } else {
                $scope.setFocusbominwardcode();
            }
        };

        $scope.focusfirst = function () {


            if ($scope.ItemDetails.materialtypeid == 1 || $scope.ItemDetails.materialtypeid == 3 ||
                $scope.ItemDetails.materialtypeid == 5) {
                $scope.ItemDetails.isbom = true;
            }
            else {
                $scope.ItemDetails.isbom = false;
            }

            $timeout(function () {
                if ($scope.ItemDetails.isbom)
                    $scope.setFocusbominwardcode();
                else
                    $scope.setFocusproductid();
            }, 20);
        }

        $scope.searchbominwardcode = function (from) {
            if ($scope.ItemDetails.bominwardcode &&
                $scope.ItemDetails.bominwardcode != null &&
                $scope.ItemDetails.bominwardcode.length > 0) {

                restService.Get('BominvoiceitemsService/SearchCode/' + $scope.ItemDetails.bominwardcode, false).then(function (results) {
                    if (from == 'right') {
                        if (results == null) {
                            $scope.setFocusbomcategoryid();
                        }
                        else {
                            $scope.ItemDetails.bomcategoryid = results.bomcategoryid;
                            $scope.ItemDetails.bominvoiceitemid = results.id;
                            $scope.ItemDetails.bominvoiceitemimage = results.imageid;
                            $scope.categoryChange();
                            $timeout(function () {
                                $scope.ItemDetails.bomid = results.bomid;
                                $scope.setFocusoqty();
                            }, 50)
                        }
                    } else {
                        if (results == null) {
                            $rootScope.Toaster.Info("Given code not found!");
                        }
                        else {
                            $scope.ItemDetails.bomcategoryid = results.bomcategoryid;
                            $scope.ItemDetails.bominvoiceitemid = results.id;
                            $scope.ItemDetails.bominvoiceitemimage = results.imageid;
                            $scope.ItemDetails.bomid = results.bomid;
                            $scope.ItemDetails.materialname = $scope.SearchRef(results.bomid, $scope.BOMAllList);
                            $scope.ItemDetails.materialtypeid = 5;
                            $scope.ItemDetails.isbom = true;
                            $scope.categoryChange();
                            $timeout(function () {
                                $scope.setFocusdoqty();
                            }, 50)
                        }
                    }
                });

            }
            else {
                $scope.setFocusbomcategoryid();
            }
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.JobOrder', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.JobOrder = value;
        };

        $scope.New = function () {
            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
            $scope.JobOrder = (JSON.parse(JSON.stringify($scope.JobOrderTemplate)));
            $scope.JobOrder.expectedate = new Date();
            $scope.JobOrder.joborderdate = new Date();
            $scope.JobOrder.bomJobOrdertaxitems = [];
            $scope.JobOrder.Lrentrydetails = [];
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

        $scope.AddItemInternal = function (computetotal) {
            //if ($scope.ItemDetails.materialtypeid == 1 || $scope.ItemDetails.materialtypeid == 3 ||
            //    $scope.ItemDetails.materialtypeid == 5) {
            $scope.ItemDetails.isbom = true;
            $scope.ItemDetails.taxid = $scope.BOMList.find(function (item) {
                return item.id == $scope.ItemDetails.bomid;
            }).taxid;
            $scope.ItemDetails.typedisplay = $scope.SearchRef($scope.ItemDetails.bomcategoryid, $scope.BOMCategories);
            var bom = $scope.BOMAllList.find(function (item) { return item.id == $scope.ItemDetails.bomid })
            $scope.ItemDetails.itemdisplay = (bom && bom != null) ? bom.title : '';
            //}
            //else {
            //    $scope.ItemDetails.isbom = false;
            //    $scope.ItemDetails.taxid = $scope.Products.find(function (item) {
            //        return item.id == $scope.ItemDetails.productid;
            //    }).taxid;
            //    $scope.ItemDetails.typedisplay = 'Product';
            //    $scope.ItemDetails.itemdisplay = $scope.SearchRef($scope.ItemDetails.productid, $scope.Products);
            //    var colour = $scope.SearchRef($scope.ItemDetails.colourid, $scope.ColourList, '');
            //    if (colour != '')
            //        $scope.ItemDetails.itemdisplay += '/' + colour;
            //}

            var taxitem = $scope.purchaseTaxActual.find(function (pitem) {
                return (pitem.id == $scope.ItemDetails.taxid);
            });

            if (taxitem || taxitem != null) {
                taxitem.taxsplit.findTax = $scope.findTax;
                var taxconfig = taxitem.taxsplit.findTax($scope.ItemDetails.sellingprice);
                if (taxconfig.cgst == 0) {
                    taxconfig = { 'cgst': 0, 'sgst': 0 };
                }
                $scope.ItemDetails.taxpercentage = taxconfig.cgst + taxconfig.sgst;
            }

            $scope.ItemDetails.sno = parseFloat($scope.ItemDetails.sno);

            if ($scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == -1) {
                $scope.ItemDetails.gross = $scope.GetNumber($scope.ItemDetails.sellingprice) * $scope.GetNumber($scope.ItemDetails.rqty);
            }
            else {
                $scope.ItemDetails.gross = $scope.GetNumber($scope.ItemDetails.sellingprice) * $scope.GetNumber($scope.ItemDetails.oqty);
            }

            $scope.ItemDetails.tax = ($scope.ItemDetails.gross * $scope.ItemDetails.taxpercentage / 100).round(4);
            $scope.ItemDetails.amount = Math.round($scope.ItemDetails.gross + $scope.ItemDetails.tax);

            if (isNaN($scope.ItemDetails.sno)) {
                if ($scope.JobOrder.Mfgjoborderitems.length == 0)
                    $scope.ItemDetails.sno = 1;
                else
                    $scope.ItemDetails.sno = $scope.JobOrder.Mfgjoborderitems[0].sno + 1;
            }
            $scope.ItemDetails.isactive = true;
            var currentdate = new Date();
            $scope.ItemDetails.uniqueid = currentdate.getTime();
            if ($scope.JobOrder.Mfgjoborderitems.indexOf($scope.ItemDetails) < 0) {
                if ($scope.JobOrder.Mfgjoborderitems.length > 0) {
                    var index = 0;
                    $.each($scope.JobOrder.Mfgjoborderitems, function (k, v) {
                        if (v.sno < $scope.ItemDetails.sno) {
                            index = k;
                        }
                    });
                    if (index + 1 < $scope.JobOrder.Mfgjoborderitems.length) {
                        if ($scope.JobOrder.Mfgjoborderitems[index + 1].sno == $scope.ItemDetails.sno) {
                            index += 1;
                            while (index < $scope.JobOrder.Mfgjoborderitems.length &&
                                $scope.JobOrder.Mfgjoborderitems[index].sno == $scope.ItemDetails.sno) {
                                index += 1;
                            }
                        }
                        if (index < $scope.JobOrder.Mfgjoborderitems.length)
                            $scope.JobOrder.Mfgjoborderitems.splice(index + 1, 0, $scope.SetRowStyle($scope.ItemDetails));
                        else
                            $scope.JobOrder.Mfgjoborderitems.push($scope.SetRowStyle($scope.ItemDetails));
                    }
                    else {
                        $scope.JobOrder.Mfgjoborderitems.push($scope.SetRowStyle($scope.ItemDetails));
                    }
                }
                else {
                    $scope.JobOrder.Mfgjoborderitems.push($scope.ItemDetails);
                }
            }
            if (typeof (computetotal) == 'undefined' || computetotal)
                $scope.CalculateTotal();
        };

        $scope.SetRowStyle = function (item) {
            if (typeof (item.poinfo) != 'undefined' &&
                item.poinfo != null && item.poinfo.length > 0)
                item.style = { "background-color": "lightgrey" };
            else
                item.style = {};
            return item;

        }

        $scope.RoundPrice = function (amount) {
            amount = parseFloat(amount);
            if (isNaN(amount)) amount = 0;
            return parseFloat(amount).round(0);

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



        $scope.CalculateTotal = function () {
            $scope.Total = {
                "taxable": 0,
                "buyingqty": 0,
                "tax": 0,
                "amount": 0,
                "actualqty": 0,
                "issuedqty": 0,
                "planqty": 0,
                "receivedqty": 0,
                "wasteqty": 0,
                "fabricqty": 0,
                "expectedqty": 0,
                "availableqty": 0
            };

            $.each($scope.JobOrder.Mfgjoborderitems, function (k, v) {

                rqty = parseFloat(v.rqty);
                cqty = parseFloat(v.cqty);
                sqty = parseFloat(v.sqty);
                oqty = parseFloat(v.oqty);
                cost = parseFloat(v.cost);
                aqty = parseFloat(v.aqty);
                woqty = parseFloat(v.woqty);

                sellingprice = parseFloat(v.sellingprice);

                if (isNaN(cost)) cost = 0.0;
                if (isNaN(oqty)) oqty = 0.0;
                if (isNaN(rqty)) rqty = 0.0;
                if (isNaN(cqty)) cqty = 0.0;
                if (isNaN(sqty)) sqty = 0.0;
                if (isNaN(aqty)) aqty = 0.0;
                if (isNaN(woqty)) woqty = 0.0;

                if ($scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == 1) {
                    if (!v.isbom) {
                        $scope.Total.expectedqty += oqty;
                        $scope.Total.actualqty += (isNaN(rqty) ? 0 : rqty);
                        $scope.Total.wasteqty += (isNaN(cqty) ? 0 : cqty);
                    } else if (v.isfabric) {
                        $scope.Total.fabricqty += oqty;
                    }
                }
                else if ($scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == -1) {
                    if (rqty > 0 && (!$scope.JobOrder.isbomprocess || v.supportbominvoice)) {
                        $scope.Total.issuedqty += oqty;
                        $scope.Total.receivedqty += rqty;
                        v.gross = (cost * rqty);
                        $scope.Total.taxable += (cost * rqty);
                        $scope.Total.tax += v.tax;
                        $scope.Total.amount += v.amount;
                    }
                }
                else if (!$scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == 1) {

                    v.gross = (sellingprice * oqty);
                    $scope.Total.taxable += v.gross;
                    $scope.Total.buyingqty += oqty;
                    $scope.Total.tax += v.tax;
                    $scope.Total.amount += v.amount;
                }
                else if (!$scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == 2) {
                    $scope.Total.expectedqty += aqty;
                    $scope.Total.planqty += woqty;
                    $scope.Total.actualqty += oqty;
                }
                else if ($scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == 2) {
                    $scope.Total.issuedqty += oqty;
                    $scope.Total.receivedqty += aqty;
                    $scope.Total.actualqty += (isNaN(rqty) ? 0 : rqty);
                    $scope.Total.wasteqty += (isNaN(cqty) ? 0 : cqty);
                }
                else if ($scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == 3) {
                    $scope.Total.expectedqty += oqty;
                    $scope.Total.receivedqty += aqty;
                    $scope.Total.actualqty += (isNaN(rqty) ? 0 : rqty);
                    $scope.Total.wasteqty += (isNaN(sqty) ? 0 : sqty);
                }
                else if (!$scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == 3) {
                    $scope.Total.expectedqty += aqty;
                    $scope.Total.actualqty += oqty;
                }
                else {
                    v.gross = (cost * oqty);
                    $scope.Total.taxable += (cost * oqty);
                    $scope.Total.buyingqty += oqty;
                    $scope.Total.tax += v.tax;
                    $scope.Total.amount += v.amount;
                }

            });

            $scope.Total.tax = $scope.Total.tax.round(2);
            $scope.Total.amount = $scope.Total.amount.round(2);
            $scope.Total.taxable = $scope.Total.taxable.round(2);
            $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);

            $scope.JobOrder.taxable = $scope.Total.taxable.round(2);
            $scope.JobOrder.tax = $scope.Total.tax.round(2);
            $scope.JobOrder.total = $scope.JobOrder.taxable + $scope.JobOrder.tax;
        };

        $scope.JobOrderList = [];
        $scope.ShowJobOrderList = false;

        $scope.focusconditiononenter = function (issuccess, sfocus, ffocus) {
            $scope[issuccess ? 'setFocus' + sfocus : 'setFocus' + ffocus]();
        }

        $scope.ImportBOM = function () {

            if ($scope.JobOrder.bomrequestid &&
                $scope.JobOrder.bomrequestid > 0) {
                $rootScope.Toaster.Info("Already BOM Request Imported from " + $scope.JobOrder.bomentryno);
                return;
            }
            
            var request = {
                "Item": {
                    mfgworkorderid: $scope.JobOrder.mfgworkorderid,
                    mfgjoborderid: $scope.JobOrder.id,
                    mfgstageid: $scope.JobOrder.mfgstageid,
                    consume:true,
                    mode: ($scope.JobOrder.id &&
                        $scope.JobOrder.id > 0) ? 2 : 1
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BomrequestService/Import4Job",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.id >0) {
                    $scope.JobOrder.bomrequestid = result.bomrequestid;
                    $scope.JobOrder.bomentryno = result.bomentryno;
                    $.each(result.BOMConsumptionitems, function (kk, vv) {
                        var taxid = $scope.BOMAllList.find(function (item) {
                            return item.id == vv.bomid;
                        }).taxid;
                        var typedisplay = $scope.SearchRef(vv.bomcategoryid, $scope.BOMCategories);
                        var bom = $scope.BOMAllList.find(function (item) { return item.id == vv.bomid })
                        var itemdisplay = (bom && bom != null) ? bom.title : '';
                        item = {
                            'bominvoiceitemimage': vv.image,
                            'typedisplay': typedisplay,
                            'itemdisplay': itemdisplay,
                            'bominwardcode': vv.bomcode,
                            'bomconsumptionitemid':vv.id,
                            'taxid': taxid ,
                            'isbom':true,
                            'materialname': vv.materialname,
                            'oqty': vv.qty,
                            'sno': $scope.JobOrder.Mfgjoborderitems.length+1,
                            'bomcategoryid': vv.bomcategoryid,
                            'bomid': vv.bomid,
                            'colourid': vv.colourid,
                            'width': vv.width,
                            'sellingprice': vv.rate,
                            'gross': vv.amount,
                            'amount': vv.amount
                        };
                        var taxitem = $scope.purchaseTaxActual.find(function (pitem) {
                            return (pitem.id == taxid);
                        });
                        if (taxitem || taxitem != null) {
                            taxitem.taxsplit.findTax = $scope.findTax;
                            var taxconfig = taxitem.taxsplit.findTax(vv.rate);
                            if (taxconfig.cgst == 0) {
                                taxconfig = { 'cgst': 0, 'sgst': 0 };
                            }
                            item.taxpercentage = taxconfig.cgst + taxconfig.sgst;
                        }
                        item.tax = (item.amount * item.taxpercentage / 100).round(4);
                        item.amount = Math.round(item.amount + item.tax);
                        $scope.JobOrder.Mfgjoborderitems.push(item);
                    });

                    $scope.CalculateTotal();
                    if ($scope.JobOrder.Mfgjoborderitems.length > 0) {
                        $scope.ItemDetails.sno = $scope.JobOrder.Mfgjoborderitems[$scope.JobOrder.Mfgjoborderitems.length - 1].sno + 1;
                    }
                    $rootScope.Toaster.Info("BOM Request Imported from " + $scope.JobOrder.bomentryno);
                }
                else if (result.length == 0) {
                    $rootScope.Toaster.Info("Can't find valid BOM Request to import");
                }
                else {
                    $rootScope.Toaster.Info("Can't import because of Multiple BOM Request");
                }
            });
        }

        $scope.InitializeJobOrderList = function (loaddirect) {

            request = {
                "Item": { id: $scope.JobOrderInfo.JobOrderid },
                "SecurityToken": "String content"
            }

            if (!loaddirect && typeof ($scope.JobOrderInfo.JobOrderid) != 'undefined' && $scope.JobOrderInfo.JobOrderid.indexOf(',') > 0) {
                request.Item.ids = $scope.JobOrderInfo.JobOrderid;
                request.Item.id = 0;
            }

            $http({
                method: 'POST',
                url: "MfgjoborderService/Search",
                data: request
            }).success(function (result) {

                if (typeof (request.Item.ids) != 'undefined') {
                    $scope.JobOrderList = result;
                    $scope.ShowJobOrderList = true;
                }
                else {
                    $scope.ShowSearch = false;
                    $scope.DisplayJobOrder(result[0].id);
                    $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
                    $scope.CalculateTotal();
                    if ($scope.JobOrder.Mfgjoborderitems.length > 0) {
                        $scope.ItemDetails.sno = $scope.JobOrder.Mfgjoborderitems[$scope.JobOrder.Mfgjoborderitems.length - 1].sno + 1;
                    }
                }
            }).error(function (result) {
            });
        };


        $scope.SupplierChanged = function () {
            if (typeof ($scope.JobOrder.suppliercompanyid) != 'undefined' ||
                $scope.JobOrder.suppliercompanyid != null) {
                $scope.JobOrder.supplierid = $scope.SupplierCompany.find(function (item) {
                    return item.id == $scope.JobOrder.suppliercompanyid;
                }).id;
            }
        }

        $scope.Save = function (movenext) {



            $scope.JobOrder.isdirectpurchase = true;

            if ($scope.JobOrder.workorderno == null || $scope.JobOrder.workorderno == 0) {
                $scope['setFocusworkorderno']();
                $rootScope.Toaster.Info("Please select Work Order no");
                return;
            }
            if ($scope.JobOrder.joborderdate == null || $scope.JobOrder.joborderdate == "") {
                $scope['setFocusjoborderdate']();
                $rootScope.Toaster.Info("Please select Job Order Date");
                return;
            }
            if ($scope.JobOrder.status == null || $scope.JobOrder.status == "") {
                $scope['setFocusstatusid']();
                $rootScope.Toaster.Info("Please select Job Order Date");
                return;
            }

            //if ($scope.JobOrder.total == null || $scope.JobOrder.total == 0) {
            //    $rootScope.Toaster.Info("Please enter bom value");
            //    return;
            //}
            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;
            var request = {
                "Item": $scope.JobOrder,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgjoborderService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Saved Successfully.!");
                    $scope.DisplayJobOrder(result.id);
                    $scope.Navigation.save = false;
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }

        $scope.DisplayJobOrder = function (id) {
            var request = {
                "Item": { 'id': id },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgjoborderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ShowSearch = false;
                $scope.Navigation.SetMode('EDIT');
                $scope.JobOrder = result[0];
                $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
                if ($scope.JobOrder.Mfgjoborderitems.length > 0) {
                    $scope.ItemDetails.sno = $scope.JobOrder.Mfgjoborderitems[$scope.JobOrder.Mfgjoborderitems.length - 1].sno + 1;
                }
                $scope.CalculateTotal();
                $scope.Navigation.save = true;
            }).error(function (result) {
            });

        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {
                $scope.JobOrder.Mfgjoborderitems.splice($scope.JobOrder.Mfgjoborderitems.indexOf(item), 1);
                if ($scope.JobOrder.Mfgjoborderitems.length == 0) {
                    $scope.ItemDetails.sno = 0;
                }
                else {
                    $scope.ItemDetails.sno = $scope.JobOrder.Mfgjoborderitems[$scope.JobOrder.Mfgjoborderitems.length - 1].sno;
                }
                $scope.CalculateTotal();
            }
            if (action == "Copy") {
                var newitem = (JSON.parse(JSON.stringify(item)));
                newitem.id = 0;
                newitem.aqty = 0;
                newitem.rqty = 0;
                newitem.brqty = 0;
                newitem.cqty = 0;
                newitem.oqty = 0;
                newitem.sno = $scope.JobOrder.Mfgjoborderitems[$scope.JobOrder.Mfgjoborderitems.length - 1].sno;
                while (true) {
                    var exist = $scope.JobOrder.Mfgjoborderitems.find(function (sitem) {
                        return sitem.sno == newitem.sno;
                    });

                    if (exist && exist != null) {

                        newitem.sno = parseFloat(newitem.sno);
                        if (isNaN(newitem.sno))
                            newitem.sno = 0;
                        newitem.sno++;
                    }
                    else {
                        break;
                    }

                }

                newitem.jobreasonid = null;
                $scope.JobOrder.Mfgjoborderitems.push(newitem);
                $scope.ItemDetails.sno = $scope.JobOrder.Mfgjoborderitems[$scope.JobOrder.Mfgjoborderitems.length - 1].sno;
                $scope.CalculateTotal();
            }
            else if (action == "Edit") {
                if (typeof ($scope.ItemDetails.IsEditItem) != 'undefined' &&
                    $scope.ItemDetails.IsEditItem) {
                    $rootScope.Confirm('Editing', 'you will loose the current editing item', function () {
                        $scope.$apply(function () {
                            $scope.EditItem(item);
                            $scope.ItemDetails.IsEditItem = true;
                        });
                    });
                }
                else {
                    $scope.EditItem(item);
                    $scope.ItemDetails.IsEditItem = true;
                }

            }
            else if (action == "EditJobOrder") {
                $scope.JobOrderInfo = { 'JobOrderid': item.id };
                $scope.InitializeJobOrderList(true);
            }
            else if (action == "DeleteJobOrder") {
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
                                    url: "MfgjoborderService/Delete",
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
            $scope.JobOrder.Mfgjoborderitems.splice($scope.JobOrder.Mfgjoborderitems.indexOf(item), 1);
            $scope.CalculateTotal();

            $scope.ItemDetails = item;
            var bomid = item.bomid;
            var colourid = item.colourid;
            var bomcategoryid = item.bomcategoryid;
            $scope.categoryChange();
            $scope.ItemDetails.bomid = bomid;
            $scope.bomChanged();
            $scope.ItemDetails.bomcategoryid = null;
            $scope.ItemDetails.bomid = null;
            $scope.ItemDetails.colourid = null;
            $timeout(function () {
                $scope.ItemDetails.bomcategoryid = bomcategoryid;
                $scope.ItemDetails.bomid = bomid;
                $scope.ItemDetails.colourid = colourid;
            }, 10);

        }


        $scope.Search = function () {
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgjoborderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ConfigSearchGrid();
                $scope.sgrid.UpdateData(result);
            }).error(function (result) {
            });
        }


        $scope.Print = function (joborderid) {
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "joborder.pdf";
                link.href = 'MfgjoborderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (joborderid) != 'undefined' ? joborderid : $scope.JobOrder.id);
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }


        $scope.SelectAll = function (items, selected) {
            $.each(items, function (k, v) {
                v.selected = selected;
            });
        }

        $scope.ConfigSearchGrid = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('searchgridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.BuyingCompany);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 100, $scope.SupplierList);
            igrid.AddSimpleColumn('workorderno', 'WO No', 80);
            igrid.AddSimpleColumn('joborderno', 'Job No', 80);
            igrid.AddSimpleColumn('sourcejobno', 'SOURCE Job No', 80);
            igrid.AddSimpleColumn('jobcardno', 'Card No', 80);
            igrid.AddFormatedColumn('joborderdate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('fromlocationid', 'Location', 100, $scope.WarehouseLocations);

            //igrid.AddSimpleColumn('stagename', 'Stage', 80);

            igrid.AddCustomColumn('stagename', 'Stage', 120, function (item) {
                return item.stagename + '(' + (item.inwardjob ? 'INWARD' : 'OUTWARD') + ')';
            });

            igrid.AddReferenceColumn('status', 'Status', 100, $scope.StatusList);
            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Manage'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>&nbsp;"
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Print'/>"

            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Manage')
                        $scope.ManageSearchItem('EditJobOrder', args.item);
                    else if ($(e.target).val() == 'Delete') {
                        $scope.ManageSearchItem('DeleteJobOrder', args.item);
                    } else if ($(e.target).val() == 'Print') {
                        $scope.Print(args.item.id);
                    }
                });
            });
            igrid.Initialize();
            $scope.sgrid = igrid;
        }

        $scope.ShowBOMFabricList = function (Mfgworkorderitems) {
            $scope.JobOrder.mfgworkorder.Mfgbomitems = [];
            $.each(Mfgworkorderitems, function (k, v) {
                $scope.JobOrder.mfgworkorder.Mfgbomitems.push({ 'bomid': v.fabricid, 'colourid': v.colourid, 'outqty': v.outqty });
                $scope.JobOrder.mfgworkorder.Mfgbomitems.push({ 'bomid': v.efabricid, 'colourid': v.efabriccolourid, 'inqty': v.inqty });
            })
        }

        $scope.ConfigSearchGrid();

        $scope.ChangeQty = function (option) {
            if (option == 'All') {
                $.each($scope.JobOrder.Mfgjoborderitems, function (k, v) {
                    if ($scope.JobOrder.inwardjob) {
                        if ($scope.JobOrder.corestageid == 2 || $scope.JobOrder.corestageid == 3)
                            v.rqty = v.oqty - v.aqty;
                        else
                            v.rqty = v.oqty;
                    }
                    else
                        v.oqty = v.aqty;
                })
            }
            else {
                $.each($scope.JobOrder.Mfgjoborderitems, function (k, v) {
                    if ($scope.JobOrder.inwardjob) {
                        v.rqty = 0;
                    }
                    else
                        v.oqty = 0;
                })
            }
            $scope.CalculateTotal();
        }

        $scope.RowReturnChanged = function (row, type) {
            row.sellingprice = $scope.RoundPrice(row.sellingprice);
            row.gross = $scope.GetNumber(row.sellingprice) * $scope.GetNumber(row.rqty);
            row.tax = (row.gross * row.taxpercentage / 100).round(4);
            row.amount = Math.round(row.gross + row.tax);
            $scope.CalculateTotal();
        }

        $scope.RowChanged = function (row, type) {
            row.sellingprice = $scope.RoundPrice(row.sellingprice);
            row.gross = $scope.GetNumber(row.sellingprice) * $scope.GetNumber(row.oqty);
            row.tax = (row.gross * row.taxpercentage / 100).round(4);
            row.amount = Math.round(row.gross + row.tax);
            $scope.CalculateTotal();
        }


        $scope.PrintBarcode = function () {
            var result = [];

            var supplier = $scope.SupplierCompany.find(function (item) {
                return item.id == $scope.JobOrder.supplierid
            });

            if (supplier == undefined) supplier = {};
            $.each($scope.JobOrder.Mfgjoborderitems, function (k, v) {

                if (v.selected) {
                    var rqty = parseFloat(v.rqty);
                    if (isNaN(rqty)) rqty = 0;

                    if (rqty > 0) {
                        var bom = $scope.BOMAllList.find(function (item) {
                            return item.id == v.bomid;
                        });

                        var colour = bom.colourlist.find(function (item) {
                            return item.id == v.colourid;
                        });

                        var pcolour = $scope.AllColorList.find(function (item) {
                            return item.id = v.colourid;
                        })

                        if (typeof (colour) == 'undefined') colour = {};
                        if (typeof (pcolour) == 'undefined') pcolour = {};

                        result.push({
                            'sno': v.sno, 'id': v.id, 'invoiceid': $scope.JobOrder.id, 'colourcode': colour.name, 'suppliername': supplier.name,
                            'colourid': v.colourid, 'bomcode': bom.bommastercode, 'bomname': bom.name, 'bomtitle': bom.title, 'suppliercode': supplier.code,
                            'bominwardcode': v.bominwardcode, 'display': bom.name + '/' + colour.name, 'pqty': 1, 'bomid': v.bomid,
                            'joborderno': $scope.JobOrder.joborderno,
                            'joborderdate': $scope.JobOrder.joborderdate,
                            'jobcardno': $scope.JobOrder.jobcardno,
                            'inwardeddate': $scope.JobOrder.inwardeddate,
                            'colourname': pcolour.name, 'qty': v.qty
                        });
                    }
                }
            });
            if (result.length > 0)
                $scope.ChooseBarcode(result);
        }

        $scope.JobChanged = function (item) {
            var jobcharge = $scope.JobCharges.find(function (sitem) {
                return sitem.id == item.jobchargeid;
            });
            item.supportbominvoice = false;
            if (jobcharge && jobcharge != null &&
                jobcharge.additionalinfo &&
                jobcharge.additionalinfo.mfgconfig) {
                item.supportbominvoice = jobcharge.additionalinfo.mfgconfig.supportbominvoice;
                item.sellingprice = jobcharge.additionalinfo.mfgconfig.charge;

                var taxitem = $scope.purchaseTaxActual.find(function (pitem) {
                    return (pitem.id == jobcharge.additionalinfo.mfgconfig.taxid);
                });

                if (taxitem || taxitem != null) {
                    taxitem.taxsplit.findTax = $scope.findTax;
                    var taxconfig = taxitem.taxsplit.findTax(item.sellingprice);
                    if (taxconfig.cgst == 0) {
                        taxconfig = { 'cgst': 0, 'sgst': 0 };
                    }
                    item.taxpercentage = taxconfig.cgst + taxconfig.sgst;
                }

                if ($scope.JobOrder.inwardjob && $scope.JobOrder.corestageid == -1) {
                    item.gross = $scope.GetNumber(item.sellingprice) * $scope.GetNumber(item.rqty);
                }
                else {
                    item.gross = $scope.GetNumber(item.sellingprice) * $scope.GetNumber(item.oqty);
                }

                item.tax = (item.gross * item.taxpercentage / 100).round(4);
                item.amount = Math.round(item.gross + item.tax);
                $scope.CalculateTotal();
            }
        }

        $scope.ChooseBarcode = function (barcodeList) {

            var requestitem = { 'items': barcodeList };
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
                    url: "MfgjoborderService/PrintBarcode",
                    data: srequest
                }).success(function (result) {
                    if (result != null && result.length > 0) {
                        $rootScope.Sent2Printer(result.join(';'), printdata);
                    }
                });
            });
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.JobOrder', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.JobOrder = value;
        };

        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings) {
                $scope.mfg_mode = $rootScope.UserInfo.appsettings.mfg_mode;
                $scope.IsSampleBased = $scope.mfg_mode == 6;



                if (typeof ($location.search().id) != 'undefined' &&
                    $location.search().id.length > 0) {
                    $timeout(function () {
                        $scope.DisplayJobOrder($location.search().id);
                    }, 100);
                }
                else if (typeof ($location.search().workorderid) != 'undefined' &&
                    $location.search().workorderid.length > 0) {

                    var request = {
                        "Item": {
                            'workflowid': $location.search().workflowid,
                            'workorderid': $location.search().workorderid,
                            'stageid': $location.search().stageid,
                            'corestageid': $location.search().corestageid,
                            'joborderid': $location.search().joborderid,
                            'inwardjob': $location.search().inwardjob
                        },
                        "securitytoken": "String content"
                    };

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "MfgjoborderService/CreateStage",
                        data: srequest
                    }).success(function (result) {

                        $timeout(function () {
                            $scope.ShowSearch = false;
                            $scope.Navigation.SetMode('EDIT');
                            $scope.JobOrder = result;
                            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
                            if ($scope.JobOrder.Mfgjoborderitems.length > 0) {
                                $scope.ItemDetails.sno = $scope.JobOrder.Mfgjoborderitems[$scope.JobOrder.Mfgjoborderitems.length - 1].sno + 1;
                            }
                            $scope.CalculateTotal();
                            if (result.mfgworkorder && result.mfgworkorder.onlybom && !$scope.IsSampleBased) {
                                $scope.JobOrder.onlybom = true;
                                $scope.ShowBOMFabricList(result.mfgworkorder.Mfgworkorderitems);
                            }
                        }, 100);

                    }).error(function (result) {
                    });
                }


            } else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 10);
            }
        }

        $timeout(function () {
            $scope.LoadAppSetting();
        }, 10);

    }]);
