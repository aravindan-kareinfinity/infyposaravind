var app = angular.module('InfinityLab');
app.controller('MFGReportsController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', '$stateParams', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, $stateParams, ngDialog) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.Permission = UserDetails.Info;
        $scope.IsPending = $stateParams.type && $stateParams.type == 'PENDING';
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Purchase = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.Purchase = results;
        });

        $scope.Sales = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.Sales = results;
        });

        $scope.SalesCounter = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.SalesCounter = results;
        });

        $scope.Classification = restService.Get('ReferencelistService/CLASSIFICATION', true).then(function (results) {
            $scope.Classification = results;
        });
        $scope.ProductDivision = restService.Get('ReferencelistService/PRODUCTDIVISION', true).then(function (results) {
            $scope.ProductDivision = results;
        });

        $scope.DeliveryLocations = restService.Get('ReferencelistService/DELIVERYLOCATION', true).then(function (results) {
            $scope.DeliveryLocations = results;
        });


        $scope.Fabrics = restService.Get('ReferencelistService/FABRIC', true).then(function (results) {
            $scope.Fabrics = results;
        });
        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });
        $scope.city = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.city = results;
        });
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });
        $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
        });
        $scope.WHLocation = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WHLocation = results;
        });
        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.AllLocation = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.AllLocation = results;
        });
        $scope.Product = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Product = results;
            results.push({ 'id': -2, 'name': 'ALL COMBO PRODUCTS' });
        });
        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.Type = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.Type = results;
        });
        $scope.Transports = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transports = results;
        });
        $scope.Colour = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colour = results;
            $scope.AllColorList = results;
        });
        $scope.CustomerCategory = restService.Get('ReferencelistService/CUSTOMERCATEGORY', true).then(function (results) {
            $scope.CustomerCategory = results;
        });
        $scope.ProductGroups = restService.Get('ReferencelistService/PRODUCTGROUPS', true).then(function (results) {
            $scope.ProductGroups = results;
        });
        $scope.DepartmentList = restService.Get('ReferencelistService/MFRDEPARTMENT', true).then(function (results) {
            $scope.DepartmentList = results;
        });
        $scope.BOMAllList = restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMAllList = results;
        });
        $scope.BOMCategories = restService.Get('ReferencelistService/FULL-BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategories = results;
        });
        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
            var exist = false;
            $.each($scope.Sizes, function (k, v) {
                if (v.id == -2)
                    exist = true;
            });
            if (!exist) {
                $scope.Sizes.push({ id: -2, name: 'Jump' });
                $scope.Sizes.push({ id: -3, name: 'Cut' });
            }

            $scope.SizeGroupList = restService.Get('ReferencelistService/FULL-SIZEGROUP', true).then(function (results) {
                $scope.SizeGroupList = results;
                $scope.FullSizeGroupList = results;
                $.each(results, function (k, v) {
                    if (v.additionalinfo &&
                        v.additionalinfo.sizegroups &&
                        v.additionalinfo.sizegroups.length > 0) {
                        v.possizelist = [];
                        v.possizeids = [];
                        $.each(v.additionalinfo.sizegroups, function (kk, vv) {
                            var size = $scope.Sizes.find(function (item) {
                                return item.id == vv
                            });
                            if (size && size != null) {
                                v.possizelist.push(size.name);
                                v.possizeids.push(size.id);
                            }
                        });
                        v.possizelist = v.possizelist.join(',');
                    }
                })
            });
        });

        $scope.ROLStockMode = [
            { id: 0, name: 'All' },
            { id: 1, name: 'Shortage' },
            { id: 2, name: 'Excess' }
        ];

        $scope.ComboType = [
            { id: 0, name: 'All' },
            { id: 3, name: 'Merged Items' },
            { id: 4, name: 'Created Combo' },
            { id: 9, name: 'Orphan Combo' }
        ];

        $scope.CoreStages = [
            { id: 1, name: 'Cutting' },
            { id: 2, name: 'Tailoring' },
            { id: 3, name: 'Process/Packing' }];

        $scope.DeliveryModes = [{
            id: 'All', name: 'All'
        },
        {
            id: 'Internal', name: 'Internal'
        },
        {
            id: 'External', name: 'External'
        }];


        $scope.SoModes = [
            { id: 0, name: 'All' },
            { id: 1, name: 'Pending' },
            { id: 2, name: 'Planned' },
            { id: 3, name: 'Cancelled' },
            { id: 4, name: 'Billed' }];

        $scope.ReportTypes = [{ 'id': 0, 'name': 'WORK ORDER' },
        { 'id': 1, 'name': 'GRN' },
        { 'id': 2, 'name': 'STOCK' },
        { 'id': 3, 'name': 'SALES' }];

        $scope.TransactionTypes = [
            { 'id': -1, 'name': 'All' },
            { 'id': 0, 'name': 'Opening' },
            { 'id': 1, 'name': 'Purchase' },
            { 'id': 2, 'name': 'Purchase Return' },
            { 'id': 3, 'name': 'Transfer In' },
            { 'id': 4, 'name': 'Transfer Out' },
            { 'id': 5, 'name': 'WorkOrder Consumption' },
            { 'id': 6, 'name': 'JobOrder Consumption' },
            { 'id': 7, 'name': 'JobOrder Stage Consumption' },
            { 'id': 8, 'name': 'JobOrder Outward' },
            { 'id': 9, 'name': 'WorkOrder Outward' },
            { 'id': 10, 'name': 'Journal' },
            { 'id': 11, 'name': 'Internal Consumption' },
            { 'id': 12, 'name': 'External Consumption' }
        ];

        $scope.ReportMode = [{ 'id': 0, 'name': 'Summary' },
        { 'id': 1, 'name': 'Detail' }];


        $scope.PackslipSearchModes = [{ 'id': 1, 'name': 'Billed' }, { 'id': 2, 'name': 'All' }, { 'id': 3, 'name': 'Unbilled' }];

        $scope.POSearchStatus = [{ 'id': 0, 'name': 'All' },
        { 'id': 1, 'name': 'Pending' },
        { 'id': 2, 'name': 'Completed' }];

        $scope.STOSearchStatus = [
            { 'id': -1, 'name': 'Requested' },
            { 'id': 0, 'name': 'Pending' },
            { 'id': 1, 'name': 'Inwarded' },
            { 'id': 2, 'name': 'Inwarded with Changes' },
            { 'id': 3, 'name': 'Pending Inward Bills' },
            { 'id': 9, 'name': 'Rejected' },
            { 'id': 10, 'name': 'All' }];

        $scope.PaymentModes = [{ 'id': 0, 'name': 'Inter Bank Transfer' },
        { 'id': 1, 'name': 'Cheque/DD' },
        { 'id': 2, 'name': 'Cash' },
        { 'id': 3, 'name': 'Multiple Cheque/DD' },
        { 'id': 4, 'name': 'Card' }];

        $scope.StockAge = [{ 'id': 1, 'name': '>Holding Period' },
        { 'id': 30, 'name': '>30 days' },
        { 'id': 45, 'name': '>45 days' },
        { 'id': 60, 'name': '>60 days' },
        { 'id': 90, 'name': '>90 days' },
        { 'id': 120, 'name': '>120 days' },
        { 'id': 180, 'name': '>180 days' },
        { 'id': 270, 'name': '>270 days' },
        { 'id': 360, 'name': '>360 days' }];

        $scope.igrid = null;

        $scope.ModeofDelivery = [{ 'id': 1, 'name': 'Lorry' }, { 'id': 2, 'name': 'Hand Delivery' }];

        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });
        $scope.Pattern = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Pattern = results;
        });
        $scope.Style = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Style = results;
        });
        $scope.Division = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Division = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.lrcount = 0;
        $scope.totalcount = 0;
        $scope.lrbundels = 0;
        $scope.lrtotalvalue = 0;
        $scope.discount = 0;
        $scope.tax = 0;
        $scope.amount = 0;
        $scope.totalqty = 0;
        $scope.ShowStockView = false;
        $scope.SearchCondition = {
            mode: 1,
            withlr: false,
            fromdate: null,
            todate: null,
            fromlrdate: null,
            tolrdate: null,
            companyid: 0,
            supplierid: 0,
            productid: 0,
            brandid: 0,
            materialid: 0,
            patternid: 0,
            styleid: 0,
            sizeid: 0,
            sectionid: 0,
            designid: "",
            showdetail: false,
            summary: true,
            cancelled: false,
            barcode: "",
            packageno: "",
            pendinginward: "",
            modeofdelivery: 0,
            lrno: "",
            lrentryno: '',
            invoiceno: "",
            returnno: "",
            product: false,
            brand: false,
            material: false,
            pattern: false,
            style: false,
            size: false,
            design: '',
            supplier: false,
            section: false,
            productgroup: false,
            productgroupid: 0,
            paymentdate: new Date()

        };

        $scope.ShowFilter = true;
        $scope.ChangeReport = function () {
            $scope.SearchResult = [];
            $scope.BomMasterResult = [];
            $scope.LRInvoiceResult = [];
            $scope.DebitnoteResult = [];
            $scope.PurchaseEntryResult = [];
            $scope.PurchaseListResult = [];
            $scope.SectionwisePurchaseResult = [];
            $scope.SectionwisePurchaseRetResult = [];
            $scope.SupplierPaymentResult = [];
            $scope.PurchaseHSNResult = [];
            $scope.POReturnResult = [];
            $scope.OutwardResult = [];
            $scope.BarcodeResult = [];
            $scope.StockResult = [];
            $scope.AsOnStockResult = [];
            $scope.OutwardPendingResult = [];
            $scope.StockAnalysisResult = [];
            $scope.StockMovementResult = [];
        }

        $scope.SearchResult = [];
        $scope.ComponentPOList = [];
        $scope.BomMasterResult = [];
        $scope.BOMConsumptionList = [];
        $scope.WOResult = [];
        $scope.SKUResult = [];
        $scope.OutwardList = [];
        $scope.InwardList = [];
        $scope.InwardDetailList = [];
        $scope.StockList = [];
        $scope.BOMList = [];

        $scope.CategoryChanged = function () {
            var list = [];
            $.each($scope.BOMAllList, function (k, v) {
                if (v.categoryid == $scope.SearchCondition.categoryid)
                    list.push(v);
            });
            $scope.BOMList = list;
        }

        $scope.ComponentChanged = function () {
            var prditem = $scope.BOMAllList.find(function (item) {
                return item.id == $scope.SearchCondition.bomid;
            });
            if (prditem && prditem != null)
                $scope.SetColourList(prditem);
        }

        $scope.SetColourList = function (prditem) {
            $scope.SearchCondition.colourid = null;
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

        $scope.SearchWOGrn = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgworkordergrnService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.ConfigureSearchGrid();
            }).error(function (result) {
            });
        }

        $scope.SearchComponentPO = function () {
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ComponentPOService/Report",
                data: srequest
            }).success(function (result) {
                $scope.ComponentPOList = result;
                $scope.ComponentPOConfig();
            });
        };


        $scope.SearchComponentPurchase = function () {
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMinvoiceService/Report",
                data: srequest
            }).success(function (result) {
                $scope.ComponentPurchaseList = result;
                $scope.ComponentPurchaseConfig();
            });
        };

        $scope.SearchJOSummary = function () {
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            request.Item.showonlypending = $scope.IsPending;

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgjoborderService/Report",
                data: srequest
            }).success(function (result) {
                $scope.JOSummaryList = result;
                $scope.JOSummaryConfig();
            });
        }

        $scope.SearchWOSummary = function () {


            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }


            request.Item.showonlypending = $scope.IsPending;

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgworkorderService/Report",
                data: srequest
            }).success(function (result) {
                $scope.WOSummaryList = result;
                $scope.WOSummaryConfig();
            });
        }


        $scope.ComponentGRN = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                if (typeof ($scope.SearchCondition.lrid) == 'undefined' || $scope.SearchCondition.lrid == 0) {
                    $scope.SearchResult = result;
                }

                else {
                    $scope.SearchResult = result.Items;
                    $scope.SearchCondition.lrid = 0;
                }
                $scope.ComponentGRNConfig();
            }).error(function (result) {
            });

        }

        $scope.DeleteBOMTransaction = function () {
            var request = {
                "Items": $scope.igrid.GetSelectedRows(),
                "securitytoken": "String content"
            }
            if (request.Items.length == 0) return;
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BomstocktransactionService/StockRemoveTransaction",
                data: srequest
            }).success(function (result) {

            }).error(function (result) {
            });
        }

        $scope.SearchStock = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BomstocktransactionService/StockReport",
                data: srequest
            }).success(function (result) {

                $scope.BomStockResult = result;
                $scope.BomStockConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchBOMMaster = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMMasterService/Search",
                data: srequest
            }).success(function (result) {

                $scope.BomMasterResult = result;
                $scope.BomMasterConfig();
            }).error(function (result) {
            });

        }

        $scope.SearchBOMConsumption = function () {
            var request = {
                "Item": {
                    'supplierid': $scope.SearchCondition.supplierid,
                    'entryno': $scope.SearchCondition.entryno,
                    'entrydate': $scope.SearchCondition.entrydate,
                    'expectedate': $scope.SearchCondition.expectedate,
                    'locationid': $scope.SearchCondition.locationid,
                    'statusid': $scope.SearchCondition.statusid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMConsumptionService/Search",
                data: srequest
            }).success(function (result) {
                $scope.BOMConsumptionList = result;
                $scope.BOMConsumptionConfig();
            });
        };

        $scope.SearchPendingWO = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            request.Item.showonlypending = true;

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgworkorderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.WOResult = result;
                $scope.WOConfig();
            }).error(function (result) {
            });

        }

        $scope.SearchSKUDetail = function () {
            $scope.SearchCondition.includedetail = true;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            $http({
                method: 'POST',
                url: "MfgdesignService/Search",
                data: request
            }).success(function (result) {

                $scope.SKUResult = result;
                $scope.SKUConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchOutward = function () {

            $scope.SearchCondition.validatetolocation = true;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Search",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.OutwardList = result;
                    $scope.BOMOutwardConfig();
                }
            }).error(function (result) {
            });
        }

        $scope.SearchInward = function () {

            $scope.SearchCondition.validatetolocation = true;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Search",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.InwardList = result;
                    $scope.BOMInwardConfig();
                }
            }).error(function (result) {
            });
        }

        $scope.SearchInwardDetail = function () {

            $scope.SearchCondition.validatetolocation = true;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMBundlesService/Search",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.InwardDetailList = result[0];
                    $scope.BOMInwardDetailConfig();
                }
            }).error(function (result) {
            });

        }

        $scope.Customers = restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.Customers = results;
        });

        $scope.SearchSKUSales = function () {
            $scope.SearchCondition.validatetolocation = true;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "SalesorderService/SearchSizeGroupSales",
                data: srequest
            }).success(function (result) {
                $scope.SKUSalesConfig(result);
            }).error(function (result) {
            });
        }

        $scope.AddPlanSelectedDesigns = function () {
            var rows = $scope.sgrid.GetSelectedRows();
            $.each(rows, function (rindex, item) {
                $.each($scope.StockList.sizenames, function (colIndex, v) {
                    var soqty = item.so[colIndex] > 0 ? item.so[colIndex] : 0;
                    if (item.stk && item.stk.sk) {
                        var stqty = item.stk.sk[colIndex] > 0 ? item.stk.sk[colIndex] : 0;
                        if (soqty > 0 && stqty > 0) {
                            if (typeof (item.pn) == 'undefined' || item.pn == null)
                                item.pn = (JSON.parse(JSON.stringify($scope.defaultplan)));
                            var pn = item.pn[colIndex];
                            var increase = 0;
                            if (stqty >= (soqty - pn)) {
                                increase = (soqty - pn);
                            }
                            else {
                                increase = stqty;
                            }
                            item.stk.sk[colIndex] -= increase;
                            item.pn[colIndex] += increase;
                            item.tpn += increase;
                        }
                    }
                });
            });

            $scope.sgrid.UpdateData($scope.StockList.salesorder, '_id');
            $scope.UpdatePlanSummary();
        }

        $scope.SavePlanSelectedDesigns = function () {
            var planlist = [];
            var rows = $scope.sgrid.GetSelectedRows();
            $.each(rows, function (rindex, item) {
                if (item.pn && item.pn != null) {
                    $.each($scope.StockList.sizeids, function (colIndex, v) {
                        var planitem = {
                            'sono': item.sono,
                            'designcode': item.designcode,
                            'customerid': item.customerid,
                            'sid': v
                        };

                        if (item.pn[colIndex] && item.pn[colIndex] > 0) {
                            var pnqty = item.pn[colIndex] > 0 ? item.pn[colIndex] : 0;
                            planitem.pqty = pnqty;
                            planlist.push(planitem);
                        }
                    });
                }
            });

            var request = {
                "Items": planlist,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SaveDesignwisePlan",
                data: srequest
            }).success(function (result) {
                if (result) {

                }
            }).error(function (result) {
            });
        }

        $scope.ClosePendingPOitems = function () {
            if ($scope.igrid && $scope.igrid != null) {
                var planlist = [];
                var rows = $scope.igrid.GetSelectedRows();
                $.each(rows, function (rindex, item) {
                    if (item.pqty && item.pqty > 0) {
                        planlist.push({ 'sid': item.id, 'pqty': item.pqty });
                    }
                });

                var request = {
                    "Items": planlist,
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "MfgReportService/ClosePendingPOitems",
                    data: srequest
                }).success(function (result) {
                    if (result) {
                        var rows = $scope.igrid.GetSelectedRows();
                        $.each(rows, function (rindex, item) {
                            if (item.pqty && item.pqty > 0) {
                                item.cqty += item.pqty;
                                item.pqty = 0;
                            }
                        });
                        $scope.ComponentPOConfig();
                    }
                }).error(function (result) {
                });
            }
        }

        $scope.RemovePlanSelectedDesigns = function () {
            var rows = $scope.sgrid.GetSelectedRows();
            $.each(rows, function (rindex, item) {
                $.each($scope.StockList.sizenames, function (colIndex, v) {
                    if (item.pn && item.pn[colIndex] && item.pn[colIndex] > 0) {
                        var plqty = item.pn[colIndex] > 0 ? item.pn[colIndex] : 0;
                        if (plqty > 0) {
                            item.stk.sk[colIndex] += plqty;
                            item.pn[colIndex] -= plqty;
                            item.tpn -= plqty;
                        }

                        if (item.opn && item.opn != null) {
                            item.pn = (JSON.parse(JSON.stringify(item.opn)));
                            $.each(item.pn, function (pk, pv) {
                                var plqty = item.pn[colIndex] > 0 ? item.pn[colIndex] : 0;
                                if (plqty > 0) {
                                    item.stk.sk[colIndex] -= plqty;
                                    item.tpn += plqty;
                                }
                            })
                        }
                    }
                });
            });

            $scope.sgrid.UpdateData($scope.StockList.salesorder, '_id');
            $scope.UpdatePlanSummary();
        }

        $scope.PrintStockSummary = function () {

            $scope.SearchCondition.printpdf = true;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BomstocktransactionService/StockReport",
                data: srequest
            }).success(function (result) {
                if (result && result != null && result.length == 16) {
                    $rootScope.Download("MfgReportService/ImagePDFGeneration/" + result, 'images.pdf', 'application/pdf');
                }
            }).error(function (result) {
            });
        }

        $scope.DownloadSOPDF = function (infygrid, field) {
            var rows = infygrid.GetSelectedRows();
            if (rows && rows.length > 0) {
                $rootScope.Download('SalesorderService/PrintPlan/' + $scope.UserInfo.OrganizationID + '/' + $scope.qEncodeURIComponent('~' + rows[0][field]), 'salesorder.pdf', 'application/pdf');
            }
        }

        $scope.qEncodeURIComponent = function (value) {
            if (value == undefined || value == '')
                value = '-';
            if (value.indexOf('.') >= 0)
                value = value.replaceAll('.', '~dot~');
            if (value.indexOf('/') >= 0)
                value = value.replaceAll('/', '~sls~');
            if (value.indexOf('*') >= 0)
                value = value.replaceAll('*', '~str~');
            return encodeURIComponent(value);
        }


        $scope.SearchDesignwiseStock4Planner = function () {
            $scope.SearchCondition.ShowSO = true;
            $scope.SearchCondition.ShowWIP = true;

            $scope.SearchCondition.possizelist = null;
            if ($scope.SearchCondition.sizegroupid &&
                $scope.SearchCondition.sizegroupid > 0) {
                var sizegroup = $scope.SizeGroupList.find(function (item) {
                    return item.id == $scope.SearchCondition.sizegroupid;
                });
                if (sizegroup && sizegroup != null) {
                    $scope.SearchCondition.possizelist = sizegroup.possizelist;
                }
            }

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchDesignwiseStock4Planner",
                data: srequest
            }).success(function (result) {
                $scope.StockList = result;
                $scope.DesignStock4PlannerConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchDesignwiseStock = function () {

            $scope.SearchCondition.possizelist = null;
            if ($scope.SearchCondition.sizegroupid &&
                $scope.SearchCondition.sizegroupid > 0) {
                var sizegroup = $scope.SizeGroupList.find(function (item) {
                    return item.id == $scope.SearchCondition.sizegroupid;
                });
                if (sizegroup && sizegroup != null) {
                    $scope.SearchCondition.possizelist = sizegroup.possizelist;
                }
            }

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchDesignwiseStock",
                data: srequest
            }).success(function (result) {
                $scope.StockList = result;
                $scope.DesignStockConfig();
            }).error(function (result) {
            });
        }


        $scope.selectallchanged = function () {
            $.each($scope.Location, function (k, v) {
                v.selected = $scope.selectall;
            })
        }

        $scope.SearchLocationwiseStock = function () {


            var SearchCondition = (JSON.parse(JSON.stringify($scope.SearchCondition)));
            SearchCondition.locations = [];
            $.each($scope.Location, function (k, v) {
                if (v.selected)
                    SearchCondition.locations.push(v.id);
            })

            SearchCondition.possizelist = null;
            if (SearchCondition.sizegroupid &&
                SearchCondition.sizegroupid > 0) {
                var sizegroup = $scope.SizeGroupList.find(function (item) {
                    return item.id == SearchCondition.sizegroupid;
                });
                if (sizegroup && sizegroup != null) {
                    SearchCondition.possizelist = sizegroup.possizelist;
                }
            }

            request = {
                "Item": SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchLocationwiseStock",
                data: srequest
            }).success(function (result) {
                $scope.StockList = result;
                $scope.DesignLocationStockConfig();
            }).error(function (result) {
            });
        }

        $scope.SearchSKUStock = function () {

            $scope.SearchCondition.validatetolocation = true;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SalesorderService/SearchSizeSummary",
                data: srequest
            }).success(function (result) {
                $scope.SOList = result;
                $scope.SOListConfig();

            }).error(function (result) {
            });
        }

        $scope.SearchUIDStock = function () {

            $scope.SearchCondition.validatetolocation = true;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchUIDStock",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.SearchResult = result;
                    $scope.UIDStockConfig();
                }
                else {
                    $rootScope.Toaster.Error("No result found!");
                }
            }).error(function (result) {
                $rootScope.Toaster.Error("No result found!");
            });
        }

        $scope.SearchTailoringDetail = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "MfgtailoringService/SearchSummary",
                data: srequest
            }).success(function (result) {
                if ($.isArray(result) && result.length > 0) {
                    $scope.TailoringConfig(result);
                }
            }).error(function (result) {
            });
        }

        $scope.SearchPlannerStock = function (item, field) {
            if (typeof (item) == 'undefined') {
                if (typeof (field) != 'undefined')
                    return ''
                return null;
            }
            if (typeof (field) != 'undefined')
                return item[field];
            return item;
        }

        $scope.PlannSummary = null;
        $scope.DesignStock4PlannerConfig = function () {
            if (typeof ($scope.StockList.sizenames) == 'undefined' || $scope.StockList.sizenames == null)
                return;
            $scope.PlannSummary = null;
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            $.each($scope.StockList.salesorder, function (k, v) {
                v.stk = $scope.StockList.stock.find(function (fitem) {
                    return fitem.name == v.designcode;
                });
                v.tso = 0;
                v.tpn = 0;
                v.tsk = 0;
                if (v.stk && v.stk != null && v.stk.sk && v.stk.sk != null) {
                    $.each(v.stk.sk, function (kk, vv) {
                        v.tsk += vv;
                    })
                }
                if (v.stk && v.stk != null && v.stk.sk && v.stk.sk != null) {
                    $.each(v.so, function (kk, vv) {
                        v.tso += vv;
                    })
                }
                if (v.pn && v.pn != null) {
                    v.opn = (JSON.parse(JSON.stringify(v.pn)));
                    $.each(v.pn, function (kk, vv) {
                        v.tpn += vv;
                    })
                }
            });

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 5);
            igrid.options.showHeaderRow = true;
            var groupColumns = [];
            igrid.AddSimpleColumn('sono', 'SO No', 80);
            igrid.AddReferenceColumn('customerid', 'Customer', 100, $scope.Customers);
            igrid.AddReferenceColumn('cityid', 'City', 100, $scope.city);
            igrid.AddSimpleColumn('designcode', 'Design', 80);


            var column = igrid.AddCustomColumn('plan', 'Plan', 60,
                function (item, col) {

                    if (item.tpn && item.tpn > 0) {
                        if (item.tso == item.tpn)
                            return 'Full';
                        else
                            return 'Partial';
                    } else if (item.tsk && item.tsk > 0) {

                        return 'Possible';

                    }
                    else {
                        return 'None';
                    }
                });
            column.enablefilter = true;
            column.sortable = true;
            column.filtertype = 'reference';
            column.getValue = function (item, col) {

                if (item.tpn && item.tpn > 0) {
                    if (item.tso == item.tpn)
                        return 'Full';
                    else
                        return 'Partial';
                } else if (item.tsk && item.tsk > 0) {

                    return 'Possible';

                }
                else {
                    return 'None';
                }
            }

            if ($scope.SearchCondition.showdetail) {
                var column = igrid.AddCustomColumn('colour', 'Colour', 40,
                    function (item, col) {
                        return $scope.SearchPlannerStock(item, 'colour');
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.filtertype = 'reference';
                column.getValue = function (item, col) {
                    return $scope.SearchPlannerStock(item, 'colour');
                }
                var column = igrid.AddCustomColumn('barcode', 'Barcode', 40,
                    function (item, col) {
                        return $scope.SearchPlannerStock(item, 'barcode');
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.filtertype = 'reference';
                column.getValue = function (item, col) {
                    return $scope.SearchPlannerStock(item, 'barcode');
                }

                var column = igrid.AddCustomColumn('brandid', 'Brand', 40,
                    function (item, col) {
                        var brandid = $scope.SearchPlannerStock(item, 'brandid');
                        return $scope.SearchRef(brandid, $scope.Brand, '');
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.filtertype = 'reference';
                column.getValue = function (item, col) {
                    var brandid = $scope.SearchPlannerStock(item, 'brandid');
                    return $scope.SearchRef(brandid, $scope.Brand, '');
                }

                var column = igrid.AddCustomColumn('productid', 'Product', 40,
                    function (item, col) {
                        var productid = $scope.SearchPlannerStock(item, 'productid');
                        return $scope.SearchRef(productid, $scope.Product, '');
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.filtertype = 'reference';
                column.getValue = function (item, col) {
                    var productid = $scope.SearchPlannerStock(item, 'productid');
                    return $scope.SearchRef(productid, $scope.Product, '');
                }

                var column = igrid.AddCustomColumn('fabricid', 'Fabric', 40,
                    function (item, col) {
                        var fabricid = $scope.SearchPlannerStock(item, 'fabricid');
                        return $scope.SearchRef(fabricid, $scope.Fabrics, '');
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.filtertype = 'reference';
                column.getValue = function (item, col) {
                    var fabricid = $scope.SearchPlannerStock(item, 'fabricid');
                    return $scope.SearchRef(fabricid, $scope.Fabrics, '');
                }
            }

            groupColumns.push(
                {
                    id: 'fabric',
                    name: 'Fabric',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = [];

            $.each($scope.StockList.sizenames, function (k, v) {
                var column = igrid.AddCustomColumn('so_' + k, v, 40,
                    function (item, col) {
                        if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                            return item.so[col.Index];
                        return '';
                    });
                column.Index = k;
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                        return item.so[col.Index];
                    return 0;
                }
            });
            groupColumns.push(
                {
                    id: 'Salesorder',
                    name: 'Sales Order',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = [];
            $.each($scope.StockList.sizenames, function (k, v) {
                var column = igrid.AddCustomColumn('stock_' + k, v, 40,
                    function (item, col) {
                        return item.stk && item.stk.sk && item.stk.sk[col.Index] > 0 ? item.stk.sk[col.Index] : '';
                    });
                column.Index = k;
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    return item.stk && item.stk.sk && item.stk.sk[col.Index] > 0 ? item.stk.sk[col.Index] : 0;
                }
            });
            groupColumns.push(
                {
                    id: 'Stock',
                    name: 'Stock',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            igrid.columns = [];
            var defaultPlan = [];
            $.each($scope.StockList.sizenames, function (k, v) {
                defaultPlan.push(0);
                var column = igrid.AddCustomColumn('rem_' + k, v, 40,
                    function (item, col) {
                        var plan = 0;
                        if (item.pn && item.pn != null && item.pn[col.Index] > 0)
                            plan = item.pn[col.Index];
                        return plan == 0 ? '' : plan;
                    });
                column.Index = k;
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.editor = Slick.Editors.Text;
                column.defaultPlan = defaultPlan;
                column.getValue = function (item, col) {
                    var plan = 0;
                    if (item.pn && item.pn != null && item.pn[col.Index] > 0)
                        plan = item.pn[col.Index];
                    return plan;
                }

                column.setValue = function (item, col, newvalue, callback) {
                    if (typeof (item.pn) == 'undefined' || item.pn == null)
                        item.pn = (JSON.parse(JSON.stringify(col.defaultPlan)));

                    var lastValue = col.getValue(item, col);
                    var newvalue = parseFloat(newvalue);
                    if (isNaN(newvalue))
                        newvalue = 0;
                    if (newvalue > 0 || lastValue != newvalue) {
                        if (newvalue > item.so[col.Index]) {
                            $rootScope.Toaster.Error("Can't allocate more than the order qty");
                            if (event.stopPropagation) {
                                event.stopPropagation();
                            }
                            return false;
                        }
                        if (lastValue > newvalue) {
                            var decrease = lastValue - newvalue;
                            var stock = item.stk;
                            if (stock != null) {
                                stock.sk[col.Index] += decrease;
                                item.pn[col.Index] -= decrease;
                                item.tsk += decrease;
                                item.tpn -= decrease;
                            }
                        }
                        else {
                            var increase = newvalue - lastValue;
                            var stock = item.stk;
                            if (stock != null && stock.sk[col.Index] >= increase) {
                                stock.sk[col.Index] -= increase;
                                item.pn[col.Index] += increase;
                                item.tsk -= increase;
                                item.tpn += increase;
                            }
                            else {
                                $rootScope.Toaster.Error("Stock not available");
                                if (event.stopPropagation) {
                                    event.stopPropagation();
                                }
                                return false;
                            }
                        }

                        $timeout(function () {
                            $scope.sgrid.UpdateData($scope.StockList.salesorder, '_id');
                            $scope.UpdatePlanSummary();
                        }, 10);
                    }
                }
            });
            $scope.defaultplan = defaultPlan;
            groupColumns.push(
                {
                    id: 'planning',
                    name: 'Planning',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            //igrid.columns = [];
            //$.each($scope.StockList.sizenames, function (k, v) {
            //    var column = igrid.AddCustomColumn('wip_' + k, v, 40,
            //        function (item, col) {
            //            if (item.wip && item.wip[col.Index] && item.wip[col.Index] > 0)
            //                return item.wip[col.Index];
            //            return '';
            //        });
            //    column.Index = k;
            //    column.enablefilter = true;
            //    column.sortable = true;
            //    column.cssClass = 'rightalign';
            //    column.total = true;
            //    column.filtertype = 'number';
            //    column.getValue = function (item, col) {
            //        if (item.wip && item.wip[col.Index] && item.wip[col.Index] > 0)
            //            return item.wip[col.Index];
            //        return 0;
            //    }
            //});
            //groupColumns.push(
            //    {
            //        id: 'wip',
            //        name: 'WIP',
            //        headerCssClass: 'text-center',
            //        columns: igrid.columns
            //    });

            igrid.columns = groupColumns;
            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.setCellCSS('');
            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData().getItem(row);
                rowdata.style = '';
                if (rowdata.tpn && rowdata.tpn > 0) {
                    if (rowdata.tso == rowdata.tpn)
                        rowdata.style = 'igridbigrowswithgreen';
                    else
                        rowdata.style = 'igridbigrowswithdarkgreen';
                } else if (rowdata.tsk && rowdata.tsk > 0) {
                    if (rowdata.tsk >= rowdata.tso)
                        rowdata.style = '';
                    else
                        rowdata.style = 'igridbigrowswithmark';
                }
                else {
                    rowdata.style = 'igridbigrowswithred';
                }
                return { 'cssClasses': rowdata.style };
            }

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $scope.UpdatePlanSummary();
            };


            igrid.grid.onDblClick.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var field = args.grid.getColumns()[args.cell].id;
                if (field == 'sono' || field == 'customerid' || field == 'cityid' || field == 'designcode' ||
                    field == 'productid' || field == 'brandid' || field == 'fabricid') {
                    var selectedIndexes = [];
                    for (var i = 0; i < args.grid.getDataLength(); i++) {
                        if (args.grid.getDataItem(i)[field] == item[field]) {
                            selectedIndexes.push(i);
                        }
                    }
                    args.grid.setSelectedRows(selectedIndexes);
                    $timeout(function () {
                        args.grid.invalidateRows(selectedIndexes);
                        args.grid.render();
                    }, 10);
                }
            });

            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.StockList.salesorder, '_id');
        }

        $scope.PlannSummary = null;
        $scope.UpdatePlanSummary = function () {
            var rows = $scope.sgrid.GetSelectedRows();
            var summaryList = [];
            if (rows.length == 0) {
                summary = null;
            }
            else {
                var summary = { 'Ordered': 0, 'Planned': 0 };
                $.each(rows, function (k, v) {

                    var summaryitem = summaryList.find(function (item) {
                        return item.customerid == v.customerid;
                    });
                    if (typeof (summaryitem) == 'undefined' || summaryitem == null) {
                        summaryitem = { 'customerid': v.customerid, 'Ordered': 0, 'Planned': 0 };
                        summaryList.push(summaryitem);
                    }
                    if (v.so && v.so != null) {
                        $.each(v.so, function (kk, vv) {
                            summary.Ordered += vv;
                            summaryitem.Ordered += vv;
                        })
                    }
                    if (v.pn && v.pn != null) {
                        $.each(v.pn, function (kk, vv) {
                            summary.Planned += vv;
                            summaryitem.Planned += vv;
                        })
                    }
                });
            }
            $timeout(function () {
                $scope.PlannSummary = summary;
                if ($scope.summarygrid != null)
                    $scope.summarygrid.destroy();
                if (summaryList.length > 0) {
                    var igrid = new InfyGrid('summarygrid', '425px', '100%', false, false, true, 310);
                    var groupColumns = [];
                    igrid.AddReferenceColumn('customerid', 'Customer', 120, $scope.Customers);
                    igrid.AddSimpleColumn('Ordered', 'Order', 50).total = true;
                    igrid.AddSimpleColumn('Planned', 'Plan', 50).total = true;
                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.summarygrid = igrid;
                    $scope.summarygrid.UpdateData(summaryList);
                }
            }, 10);
        }

        $scope.DesignStockConfig = function () {
            if (typeof ($scope.StockList.sizenames) == 'undefined' || $scope.StockList.sizenames == null)
                return;

            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            var groupColumns = [];
            igrid.AddSimpleColumn('name', 'Design', 80);
            igrid.AddSimpleColumn('colour', 'Colour', 80);
            igrid.AddSimpleColumn('barcode', 'Barcode', 50);
            if ($scope.SearchCondition.ShowDetail) {
                igrid.AddReferenceColumn('brandid', 'Brand', 100, $scope.Brand);
                igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
                igrid.AddSimpleColumn('racknumber', 'Rack', 50);

                if (!$scope.SearchCondition.ShowFabric) {
                    igrid.AddReferenceColumn('fabricid', 'Fabric', 100, $scope.Fabrics);
                    groupColumns.push(
                        {
                            id: 'Filter',
                            name: 'Filter',
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                }
                else {
                    groupColumns.push(
                        {
                            id: 'Filter',
                            name: 'Filter',
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });

                    igrid.columns = [];
                    igrid.AddReferenceColumn('fabricid', 'Name', 100, $scope.Fabrics);
                    igrid.AddSimpleColumn('fabricstock', 'Stock', 50);
                    igrid.AddSimpleColumn('forecast', 'Forecast', 50);

                    groupColumns.push(
                        {
                            id: 'Fabric',
                            name: 'Fabric',
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                }
            }
            else {
                groupColumns.push(
                    {
                        id: 'Filter',
                        name: 'Filter',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            if ($scope.SearchCondition.ShowROL) {
                igrid.columns = [];
                igrid.AddSimpleColumn('bommin', 'BOM(min)', 50);
                igrid.AddSimpleColumn('bommax', 'BOM(max)', 50);
                igrid.AddSimpleColumn('fgromin', 'FG(min)', 50);
                igrid.AddSimpleColumn('fgromax', 'FG(max)', 50);

                groupColumns.push(
                    {
                        id: 'Reorder',
                        name: 'Reorder Levels',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            if ($scope.SearchCondition.ShowPrice) {

                igrid.columns = [];
                $.each(['Rate 1', 'Rate 2', 'MRP 1', 'MRP 2'], function (k, v) {
                    var column = igrid.AddCustomColumn('price_' + k, v, 50,
                        function (item, col) {
                            if (item.price != null && item.price[col.Index] > 0)
                                return item.price[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.price != null && item.price[col.Index] > 0)
                            return item.price[col.Index];
                        return 0;
                    }
                });
                groupColumns.push(
                    {
                        id: 'Price',
                        name: 'Price',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });

            }

            igrid.columns = [];
            $.each($scope.StockList.sizenames, function (k, v) {
                var column = igrid.AddCustomColumn('stock_' + k, v, 40,
                    function (item, col) {
                        if (item.stock[col.Index] > 0)
                            return item.stock[col.Index];
                        return '';
                    });
                column.Index = k;
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.stock[col.Index] > 0)
                        return item.stock[col.Index];
                    return 0;
                }
            });

            var column = igrid.AddCustomColumn('stock_total', 'Total', 40,
                function (item, col) {
                    if (item.stock) {
                        var stk = 0;
                        $.each(item.stock, function (k, v) {
                            stk += v;
                        })
                        return stk;
                    }
                    return '';
                });
            column.enablefilter = true;
            column.sortable = true;
            column.cssClass = 'rightalign';
            column.total = true;
            column.filtertype = 'number';
            column.getValue = function (item, col) {
                if (item.stock) {
                    var stk = 0;
                    $.each(item.stock, function (k, v) {
                        stk += v;
                    })
                    return stk;
                }
                return 0;
            }

            groupColumns.push(
                {
                    id: 'Stock',
                    name: 'Stock',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            if ($scope.SearchCondition.ShowPurchase) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('pur_' + k, v, 40,
                        function (item, col) {
                            if (item.pur && item.pur[col.Index] && item.pur[col.Index] > 0)
                                return item.pur[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.pur && item.pur[col.Index] && item.pur[col.Index] > 0)
                            return item.pur[col.Index];
                        return 0;
                    }
                });
                groupColumns.push(
                    {
                        id: 'pur',
                        name: 'Purchase',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            if ($scope.SearchCondition.ShowSales) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('sal_' + k, v, 40,
                        function (item, col) {
                            if (item.sal && item.sal[col.Index] && item.sal[col.Index] > 0)
                                return item.sal[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.sal && item.sal[col.Index] && item.sal[col.Index] > 0)
                            return item.sal[col.Index];
                        return 0;
                    }
                });
                groupColumns.push(
                    {
                        id: 'sal',
                        name: 'Sales',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            if ($scope.SearchCondition.ShowSO) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('so_' + k, v, 40,
                        function (item, col) {
                            if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                                return item.so[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                            return item.so[col.Index];
                        return 0;
                    }
                });

                var column = igrid.AddCustomColumn('so_total', 'Total', 40,
                    function (item, col) {
                        if (item.so) {
                            var soqty = 0;
                            $.each(item.so, function (k, v) {
                                soqty += v;
                            });
                            return soqty;
                        }
                        return '';
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.so) {
                        var soqty = 0;
                        $.each(item.so, function (k, v) {
                            soqty += v;
                        });
                        return soqty;
                    }
                    return 0;
                }

                groupColumns.push(
                    {
                        id: 'Salesorder',
                        name: 'Sales Order',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            if ($scope.SearchCondition.ShowWIP) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('wip_' + k, v, 40,
                        function (item, col) {
                            if (item.wip && item.wip[col.Index] && item.wip[col.Index] > 0)
                                return item.wip[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.wip && item.wip[col.Index] && item.wip[col.Index] > 0)
                            return item.wip[col.Index];
                        return 0;
                    }
                });
                groupColumns.push(
                    {
                        id: 'wip',
                        name: 'WIP',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            if ($scope.SearchCondition.ShowPacking) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('pck_' + k, v, 40,
                        function (item, col) {
                            if (item.pck && item.pck[col.Index] && item.pck[col.Index] > 0)
                                return item.pck[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.pck && item.pck[col.Index] && item.pck[col.Index] > 0)
                            return item.pck[col.Index];
                        return 0;
                    }
                });

                var column = igrid.AddCustomColumn('packing_total', 'Total', 40,
                    function (item, col) {
                        if (item.pck) {
                            var soqty = 0;
                            $.each(item.pck, function (k, v) {
                                soqty += v;
                            });
                            return soqty;
                        }
                        return '';
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    if (item.pck) {
                        var soqty = 0;
                        $.each(item.pck, function (k, v) {
                            soqty += v;
                        });
                        return soqty;
                    }
                    return 0;
                }
                groupColumns.push(
                    {
                        id: 'pck',
                        name: 'Pack',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            if ($scope.SearchCondition.ShowRemaining) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('rem_' + k, v, 40,
                        function (item, col) {
                            var stock = 0;
                            var so = 0;
                            if (item.stock && item.stock[col.Index] && item.stock[col.Index] > 0)
                                stock = item.stock[col.Index];
                            if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                                so = item.so[col.Index];
                            if (stock - so == 0)
                                return '';
                            return stock - so;
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        var stock = 0;
                        var so = 0;
                        if (item.stock && item.stock[col.Index] && item.stock[col.Index] > 0)
                            stock = item.stock[col.Index];
                        if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                            so = item.so[col.Index];
                        return stock - so;
                    }
                });

                var column = igrid.AddCustomColumn('rem_total', 'Total', 40,
                    function (item, col) {
                        var soqty = 0;
                        if (item.stock) {
                            $.each(item.stock, function (k, v) {
                                soqty += v;
                            });
                        }

                        if (item.so) {
                            $.each(item.so, function (k, v) {
                                soqty -= v;
                            });
                        }
                        if (soqty == 0) return '';
                        return soqty;
                    });
                column.enablefilter = true;
                column.sortable = true;
                column.cssClass = 'rightalign';
                column.total = true;
                column.filtertype = 'number';
                column.getValue = function (item, col) {
                    var soqty = 0;
                    if (item.stock) {
                        $.each(item.stock, function (k, v) {
                            soqty += v;
                        });
                    }
                    if (item.so) {
                        $.each(item.so, function (k, v) {
                            soqty -= v;
                        });
                    }
                    return soqty;
                }

                groupColumns.push(
                    {
                        id: 'remaining',
                        name: 'Remaining',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }
            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.StockList.items);
        }


        $scope.DesignLocationStockConfig = function () {
            if (typeof ($scope.StockList.sizenames) == 'undefined' || $scope.StockList.sizenames == null)
                return;

            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            var groupColumns = [];
            igrid.AddSimpleColumn('name', 'Design', 80);
            igrid.AddSimpleColumn('colour', 'Colour', 80);
            igrid.AddSimpleColumn('barcode', 'Barcode', 50);
            igrid.AddReferenceColumn('brandid', 'Brand', 100, $scope.Brand);
            igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
            igrid.AddReferenceColumn('fabricid', 'Fabric', 100, $scope.Fabrics);
            groupColumns.push(
                {
                    id: 'Filter',
                    name: 'Filter',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });


            $.each($scope.Location, function (kk, vv) {

                if (vv.selected) {


                    igrid.columns = [];

                    $.each($scope.StockList.sizenames, function (k, v) {
                        var column = igrid.AddCustomColumn('stock_' + k, v, 40,
                            function (item, col) {
                                var stocklist = item.stock.find(function (fitem) {
                                    return fitem.locationid == col.Locationid;
                                });
                                if (stocklist && stocklist != null) {
                                    if (stocklist.stock[col.Index] > 0)
                                        return stocklist.stock[col.Index];
                                }
                                return '';
                            });
                        column.Index = k;
                        column.Locationid = vv.id;
                        column.enablefilter = true;
                        column.sortable = true;
                        column.cssClass = 'rightalign';
                        column.total = true;
                        column.filtertype = 'number';
                        column.getValue = function (item, col) {
                            var stocklist = item.stock.find(function (fitem) {
                                return fitem.locationid == col.Locationid;
                            });
                            if (stocklist && stocklist != null) {
                                if (stocklist.stock[col.Index] > 0)
                                    return stocklist.stock[col.Index];
                            }
                            return 0;
                        }
                    });
                    groupColumns.push(
                        {
                            id: 'Stock-' + kk,
                            name: vv.name,
                            headerCssClass: 'text-center',
                            columns: igrid.columns
                        });
                }
            })


            if ($scope.SearchCondition.ShowSO) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('so_' + k, v, 40,
                        function (item, col) {
                            if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                                return item.so[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                            return item.so[col.Index];
                        return 0;
                    }
                });
                groupColumns.push(
                    {
                        id: 'Salesorder',
                        name: 'Sales Order',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }

            if ($scope.SearchCondition.ShowWIP) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('wip_' + k, v, 40,
                        function (item, col) {
                            if (item.wip && item.wip[col.Index] && item.wip[col.Index] > 0)
                                return item.wip[col.Index];
                            return '';
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        if (item.wip && item.wip[col.Index] && item.wip[col.Index] > 0)
                            return item.wip[col.Index];
                        return 0;
                    }
                });
                groupColumns.push(
                    {
                        id: 'wip',
                        name: 'WIP',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }
            if ($scope.SearchCondition.ShowRemaining) {
                igrid.columns = [];
                $.each($scope.StockList.sizenames, function (k, v) {
                    var column = igrid.AddCustomColumn('rem_' + k, v, 40,
                        function (item, col) {
                            var stock = 0;
                            var so = 0;
                            if (item.stock) {
                                $.each(item.stock, function (k, v) {
                                    stock += v.stock[col.Index];
                                })
                            }
                            if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                                so = item.so[col.Index];
                            if (stock - so == 0)
                                return '';
                            return stock - so;
                        });
                    column.Index = k;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        var stock = 0;
                        var so = 0;
                        if (item.stock) {
                            $.each(item.stock, function (k, v) {
                                stock += v.stock[col.Index];
                            })
                        }
                        if (item.so && item.so[col.Index] && item.so[col.Index] > 0)
                            so = item.so[col.Index];
                        return stock - so;
                    }
                });
                groupColumns.push(
                    {
                        id: 'remaining',
                        name: 'Remaining',
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });
            }
            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.StockList.designs);
        }

        $scope.UIDStockConfig = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            var id = parseInt($scope.SearchCondition.reporttypeid);
            if (isNaN(id)) id = 0;
            if (id == 0) {
                igrid.AddFormatedColumn('wodate', 'WO Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('workorderno', 'WO No', 80);
                igrid.AddSimpleColumn('uid', 'UID', 80);
                igrid.AddSimpleColumn('size', 'Size', 80);
                igrid.AddFormatedColumn('ordered', 'Ordered', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('delivered', 'Delivered', 80, 'N2', 0).total = true;
            }
            if (id == 1) {
                igrid.AddFormatedColumn('invoicedate', 'GRN Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('wodate', 'WO Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('workorderno', 'WO No', 80);
                igrid.AddSimpleColumn('entryno', 'GRN Entry No', 80);
                igrid.AddSimpleColumn('uid', 'UID', 80);
                igrid.AddSimpleColumn('size', 'Size', 80);
                igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 0).total = true;
            }
            if (id == 2) {
                igrid.AddSimpleColumn('locationname', 'Location', 80);
                igrid.AddFormatedColumn('invoicedate', 'GRN Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('wodate', 'WO Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('workorderno', 'WO No', 80);
                igrid.AddSimpleColumn('entryno', 'GRN Entry No', 80);
                igrid.AddSimpleColumn('uid', 'UID', 80);
                igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 0).total = true;
            }
            if (id == 3) {
                igrid.AddSimpleColumn('locationname', 'Location', 80);
                igrid.AddSimpleColumn('billno', 'Bill No', 80);
                igrid.AddFormatedColumn('billdate', 'Bill Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('invoicedate', 'GRN Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('wodate', 'WO Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('workorderno', 'WO No', 80);
                igrid.AddSimpleColumn('entryno', 'GRN Entry No', 80);
                igrid.AddSimpleColumn('uid', 'UID', 80);
                igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 0).total = true;
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.SearchResult);
        }

        $scope.sgrid = null;
        $scope.ConfigureSearchGrid = function () {
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('searchgridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            if (typeof ($scope.SearchCondition.locationid) == 'undefined' || $scope.SearchCondition.locationid == null ||
                $scope.SearchCondition.locationid == '' || $scope.SearchCondition.locationid < 0) {
                igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.AllLocation);
            }
            if (typeof ($scope.SearchCondition.receivinglocationid) == 'undefined' || $scope.SearchCondition.receivinglocationid == null ||
                $scope.SearchCondition.receivinglocationid == '' || $scope.SearchCondition.receivinglocationid < 0) {
                igrid.AddReferenceColumn('receivinglocationid', 'Received At', 100, $scope.AllLocation);
            }
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('entryno', 'Entry No', 80);
            igrid.AddSimpleColumn('workorderno', 'WO.No', 80);
            igrid.AddFormatedColumn('workorderdate', 'WO.Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddFormatedColumn('invoicedate', 'Inv.Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 150, $scope.Supplier);
            igrid.AddSimpleColumn('design', 'Design', 80);
            igrid.AddSimpleColumn('product', 'Product', 80);
            igrid.AddFormatedColumn('totalqty', 'Qty', 80, 'N2', 0).total = true;
            igrid.AddFormatedColumn('total', 'Amount', 80, 'N2', 0).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.SearchResult);
        }


        $scope.ComponentPurchaseConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            if ($scope.SearchCondition.mode == 0) {
                igrid.AddSimpleColumn('name', 'Name', 80);
                igrid.AddSimpleColumn('bomcode', 'Code', 80);
                igrid.AddSimpleColumn('shade', 'Shade', 80);
                igrid.AddSimpleColumn('colour', 'Colour', 80);
                igrid.AddFormatedColumn('OrderQty', 'Ordered Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('buyingqty', 'Buying Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('buyingprice', 'Average Rate', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('amount', 'Amount', 100, 'N2', 2);
            }
            else {
                igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
                igrid.AddFormatedColumn('entrydate', 'Entry Date', 100, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('suppliername', 'Supplier', 80);
                igrid.AddSimpleColumn('cityname', 'City', 80);
                igrid.AddSimpleColumn('name', 'Name', 80);
                igrid.AddSimpleColumn('bomcode', 'Code', 80);
                igrid.AddSimpleColumn('shade', 'Shade', 80);
                igrid.AddSimpleColumn('colour', 'Colour', 80);
                igrid.AddSimpleColumn('width', 'Width', 80);
                igrid.AddSimpleColumn('ponumber', 'PO Number', 80);
                igrid.AddFormatedColumn('OrderQty', 'Ordered Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('buyingqty', 'Buying Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('buyingprice', 'Buying Rate', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('amount', 'Amount', 100, 'N2', 2);
            }
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.ComponentPurchaseList);
        }

        $scope.ComponentPOItemSelected = 0;
        $scope.ComponentPOConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            if ($scope.SearchCondition && $scope.SearchCondition.mode == 1) {
                igrid.AddSimpleColumn('pono', 'Order No', 80);
                igrid.AddFormatedColumn('podate', 'Order Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('pobookdate', 'Booking Date', 80, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('duedate', 'Due Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('suppliername', 'Supplier', 80);
                igrid.AddSimpleColumn('cityname', 'City', 80);
                igrid.AddSimpleColumn('name', 'Name', 80);
                igrid.AddSimpleColumn('design', 'Design', 80);
                igrid.AddSimpleColumn('shade', 'Shade', 80);
                igrid.AddSimpleColumn('colour', 'Colour', 80);
                igrid.AddSimpleColumn('width', 'Width', 80);
                igrid.AddSimpleColumn('supplierbrand', 'S.Brand', 80);
                igrid.AddSimpleColumn('supplierdesign', 'S.Design', 80);
                igrid.AddFormatedColumn('oqty', 'Ordered Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rate', 'Rate', 100, 'N2', 2);
                igrid.AddFormatedColumn('amount', 'Amount', 100, 'N2', 2);
                igrid.AddFormatedColumn('rqty', 'Received Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('cqty', 'Cancelled Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('pqty', 'Pending Qty', 100, 'N2', 2).total = true;
            }
            else {
                igrid.AddSimpleColumn('pono', 'Order No', 80);
                igrid.AddFormatedColumn('podate', 'Order Date', 100, 'dd-MM-yyyy');
                igrid.AddReferenceColumn('supplierid', 'Supplier Name', 150, $scope.Supplier);
                igrid.AddReferenceColumn('locationid', 'Order Location', 100, $scope.AllLocation);
                igrid.AddReferenceColumn('deliverylocationid', 'Delivery Location', 100, $scope.AllLocation);
                igrid.AddFormatedColumn('expectedate', 'Due Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('tolerance', 'Qty Tolerance %', 100);
                igrid.AddReferenceColumn('agentid', 'Agent Name', 100, $scope.Agent);
                igrid.AddReferenceColumn('approvedby', 'Approved By', 100, $scope.Employee);
                igrid.AddFormatedColumn('oqty', 'Ordered Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('rqty', 'Received Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('cqty', 'Cancelled Qty', 100, 'N2', 2).total = true;
                igrid.AddFormatedColumn('pqty', 'Pending Qty', 100, 'N2', 2).total = true;
            }

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.ComponentPOList, '_id');
            if ($scope.SearchCondition && $scope.SearchCondition.mode == 1) {
                igrid.onSelectedRangesChanged = function (range, rowmodel) {
                    $timeout(function () {
                        $scope.ComponentPOItemSelected = range.length;
                    }, 10);
                };
            }
        }

        $scope.ComponentGRNConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddReferenceColumn('suppliercompanyid', 'Supplier', 170, $scope.SupplierCompany);
            igrid.AddSimpleColumn('invoiceentryno', 'Entry No', 100);
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 100);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 100, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('noofbundle', 'Bundles', 80);
            igrid.AddFormatedColumn('baseamount', 'Base', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax', 120, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalamount', 'Total', 150, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.BomMasterResult);

        }

        $scope.BomMasterConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddSimpleColumn('printingname', 'Printing Name', 190);
            igrid.AddReferenceColumn('uomid', 'UOM', 100, $scope.UOMList);
            igrid.AddReferenceColumn('colorid', 'Color', 100, $scope.ColorList);
            igrid.AddReferenceColumn('taxid', 'Tax', 100, $scope.TaxList);
            igrid.AddSimpleColumn('width', 'Width', 100);


            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.BomMasterResult);
        }

        $scope.BomStockConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.BomStockResult);

            if ($scope.SearchCondition.mode == 0) {
                if (typeof ($scope.SearchCondition.companyid) == 'undefined' ||
                    $scope.SearchCondition.companyid == null || $scope.SearchCondition.companyid == '') {
                    igrid.AddReferenceColumn('companyid', 'Company', 150, $scope.Company);
                }
                if (typeof ($scope.SearchCondition.locationid) == 'undefined' ||
                    $scope.SearchCondition.locationid == null || $scope.SearchCondition.locationid == '') {
                    igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.AllLocation);
                }
                var column = igrid.AddCustomColumn('bomid', 'Component', 100, function (item) {
                    var bom = $scope.BOMAllList.find(function (k) {
                        return k.id == item.bomid;
                    });
                    if (bom && bom != null) return bom.title;
                    return '-';
                });

                igrid.AddReferenceColumn('colourid', 'Color', 100, $scope.AllColorList);
                igrid.AddSimpleColumn('colourcode', 'Shade', 80);
                //igrid.AddSimpleColumn('width', 'Width', 80);

                if ($scope.SearchCondition.reorderlevel) {
                    igrid.AddFormatedColumn('closing', 'Stock', 80, 'N2', 2);
                    igrid.AddFormatedColumn('poqty', 'PO', 80, 'N2', 2);
                    igrid.AddFormatedColumn('romin', 'Min', 80, 'N2', 2);
                    igrid.AddFormatedColumn('romax', 'Max', 80, 'N2', 2);
                    igrid.AddFormatedColumn('buyingrate', 'Accepted Rate', 80, 'N2', 2);
                    igrid.AddFormatedColumn('landingcost', 'Cost', 80, 'N2', 2);

                    var column = igrid.AddCustomColumn('closingvlaue', 'Cost Value', 100, function (item) {
                        return Math.round(item.landingcost * item.closing);
                    });
                    column.total = true;
                    column.enablefilter = true;
                    column.sortable = true;
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.filtertype = 'number';
                    column.getValue = function (item, col) {
                        return Math.round(item.landingcost * item.closing);
                    }
                } else {
                    igrid.AddFormatedColumn('opening', 'Opening', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('inward', 'Inward', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('outward', 'Outward', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('closing', 'Closing', 80, 'N2', 2).total = true;
                    if ($scope.SearchCondition.stockvalue) {
                        igrid.AddFormatedColumn('stockvalue', 'Cost', 80, 'N2', 2).total = true;
                    }
                }
            }
            else {

                igrid.AddFormatedColumn('transactedon', 'Date', 80, 'dd-MM-yyyy')
                if (typeof ($scope.SearchCondition.companyid) == 'undefined' ||
                    $scope.SearchCondition.companyid == null || $scope.SearchCondition.companyid == '') {
                    igrid.AddReferenceColumn('companyid', 'Company', 150, $scope.Company);
                }
                if (typeof ($scope.SearchCondition.locationid) == 'undefined' ||
                    $scope.SearchCondition.locationid == null || $scope.SearchCondition.locationid == '') {
                    igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.AllLocation);
                }

                igrid.AddReferenceColumn('supplier', 'Supplier', 100, $scope.Supplier);
                igrid.AddReferenceColumn('otherlocationid', 'Agnt Location', 150, $scope.AllLocation);

                igrid.AddReferenceColumn('reftype', 'Type', 100, $scope.TransactionTypes);
                var column = igrid.AddCustomColumn('bomid', 'Component', 100, function (item) {
                    var bom = $scope.BOMAllList.find(function (k) {
                        return k.id == item.bomid;
                    });
                    if (bom && bom != null) return bom.title;
                    return '-';
                });
                igrid.AddReferenceColumn('colourid', 'Color', 100, $scope.AllColorList);
                igrid.AddSimpleColumn('colourcode', 'Shade', 80);
                //igrid.AddSimpleColumn('width', 'Width', 80);
                igrid.AddSimpleColumn('refno', 'Detail', 100);
                igrid.AddFormatedColumn('opening', 'Opening', 80, 'N2', 2);
                igrid.AddFormatedColumn('inward', 'Inward', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('outward', 'Outward', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('closing', 'Closing', 80, 'N2', 2);

            }
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            if ($scope.SearchCondition.reorderlevel && $scope.SearchCondition.mode == 0 &&
                $scope.SearchCondition.rolmode && $scope.SearchCondition.rolmode > 0) {
                var filterrows = [];
                $.each($scope.BomStockResult, function (k, v) {
                    if ($scope.SearchCondition.rolmode == 1 && v.romin > 0
                        && v.closing < v.romin) {
                        filterrows.push(v);
                    } else if ($scope.SearchCondition.rolmode == 2 && v.romax > 0 && v.closing > v.romax) {
                        filterrows.push(v);
                    }
                })
                $scope.BomStockResult = filterrows;
            }
            $scope.igrid.UpdateData($scope.BomStockResult, '_id');
        }

        $scope.SKUSalesConfig = function (result) {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;
            if ($scope.SearchCondition.showdetail) {
                igrid.AddFormatedColumn('billdate', 'Bill Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('billno', 'Bill No', 80);
                igrid.AddSimpleColumn('billso', 'SO', 80);
            }
            igrid.AddSimpleColumn('customer', 'Customer', 150);
            igrid.AddSimpleColumn('city', 'City', 100);
            igrid.AddSimpleColumn('productname', 'Product', 150);
            igrid.AddSimpleColumn('brand', 'Brand', 100);
            igrid.AddSimpleColumn('groupkey', 'Design', 100);
            igrid.AddSimpleColumn('colourname', 'Colour', 100);
            igrid.AddFormatedColumn('salesqty', 'Billed', 80, 'N0', 2).total = true;

            $.each(result.sizenames, function (k, v) {
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

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData(result.items, '_id');
        }

        $scope.SOListConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 30);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sono', 'Order No', 100);
            igrid.AddFormatedColumn('sodate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('customername', 'Customer', ($scope.Mode == 'LIST') ? 150 : 300);
            igrid.AddSimpleColumn('agentname', 'Agent', 150);
            igrid.AddSimpleColumn('cityname', 'City', 100);
            igrid.AddSimpleColumn('salesmen', 'Salesmen', 100);
            igrid.AddSimpleColumn('productname', 'Product', 150);
            igrid.AddSimpleColumn('brand', 'Brand', 100);
            if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.sizegroup)
                igrid.AddSimpleColumn('sizegroup', 'Size/Set', 100).commafilter = true;
            if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.brand)
                igrid.AddSimpleColumn('brandname', 'Brand', 100).commafilter = true;
            if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.style)
                igrid.AddSimpleColumn('stylename', 'Style', 100).commafilter = true;
            if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.design)
                igrid.AddSimpleColumn('designid', 'Design', 100).commafilter = true;
            if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.colour)
                igrid.AddSimpleColumn('colourname', 'Colour', 100).commafilter = true;
            if ($rootScope.UserInfo.appsettings.saleorder && $rootScope.UserInfo.appsettings.saleorder.sleeve)
                igrid.AddSimpleColumn('sleevename', 'Sleeve', 100).commafilter = true;
            igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('plannedqty', 'Planed', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('cancelledqty', 'Cancelled', 80, 'N0', 2).total = true;
            igrid.AddFormatedColumn('billedqty', 'Billed', 80, 'N0', 2).total = true;

            $.each($scope.SOList.sizelist, function (k, v) {
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

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SOList.items, '_id');
        }

        $scope.BOMConsumptionConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('entryno', 'Entry No', 80);
            igrid.AddFormatedColumn('entrydate', 'Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 200, $scope.Supplier);
            igrid.AddReferenceColumn('locationid', 'Order Location', 100, $scope.AllLocation);
            igrid.AddReferenceColumn('deliverylocationid', 'Delivery Location', 100, $scope.AllLocation);
            igrid.AddFormatedColumn('expectedate', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('tolerance', 'Qty Tolerance %', 100);
            igrid.AddReferenceColumn('agentid', 'Agent Name', 100, $scope.Agent);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.BOMConsumptionList);


        }

        $scope.WOConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('companyid', 'Company', 100, $scope.Company);
            igrid.AddSimpleColumn('workorderno', 'WO No', 80);
            igrid.AddReferenceColumn('suppliercompanyid', 'Supplier', 100, $scope.SupplierCompany);
            igrid.AddFormatedColumn('wodate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.WarehouseLocations);
            igrid.AddReferenceColumn('issuedby', 'Issued', 100, $scope.Employee);
            igrid.AddSimpleColumn('product', 'Products', 100);
            igrid.AddSimpleColumn('design', 'Design', 100);
            igrid.AddSimpleColumn('totalqty', 'Qty', 100);
            igrid.AddSimpleColumn('total', 'Cost', 100);

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.WOResult);


        }

        $scope.JOSummaryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('joborderno', 'JO No', 80);
            igrid.AddFormatedColumn('joborderdate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('workorderno', 'WO No', 80);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 100, $scope.Supplier);
            igrid.AddFormatedColumn('wodate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.AllLocation);
            if ($scope.SearchCondition.onlybom) {
                igrid.AddSimpleColumn('product', 'Fabric', 100);
                igrid.AddSimpleColumn('jobworkname', 'Work', 100);
            }
            else {
                igrid.AddSimpleColumn('product', 'Products', 100);
                igrid.AddSimpleColumn('designitemname', 'Design', 100);
            }

            igrid.AddSimpleColumn('colourname', 'Colour', 100);
            igrid.AddSimpleColumn('stage', 'Stage', 100);
            igrid.AddSimpleColumn('issuedqty', 'Issued', 100);
            igrid.AddSimpleColumn('receivedqty', 'Received', 100);
            igrid.AddSimpleColumn('wasteqty', 'Wastage', 100);
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.JOSummaryList);
        }

        $scope.WOSummaryConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('workorderno', 'WO No', 80);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 100, $scope.Supplier);
            igrid.AddFormatedColumn('wodate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expectdate', 'Due On', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.AllLocation);
            igrid.AddSimpleColumn('jobcardno', 'Job Card', 80);
            igrid.AddSimpleColumn('lotno', 'Lot #', 80);

            var groupColumns = [];
            groupColumns.push(
                {
                    id: 'workorder',
                    name: 'Work Order',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = [];
            igrid.AddSimpleColumn('product', 'Products', 100);
            igrid.AddSimpleColumn('designitemname', 'Design', 100);
            igrid.AddSimpleColumn('colourname', 'Colour', 100);
            groupColumns.push(
                {
                    id: 'product',
                    name: 'Product',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });
            igrid.columns = [];

            var column = igrid.AddSimpleColumn('fabricqty', 'Fabric', 70);
            column.cssClass = 'rightalign';
            column.total = true;
            column = igrid.AddSimpleColumn('cfabricqty', 'Consumption', 70);
            column.cssClass = 'rightalign';
            column.total = true;
            column = igrid.AddSimpleColumn('totalqty', 'Order', 70);
            column.cssClass = 'rightalign';
            column.total = true;

            column = igrid.AddSimpleColumn('grnqty', 'GRN', 70);
            column.cssClass = 'rightalign';
            column.total = true;

            column = igrid.AddSimpleColumn('scutratio', 'O.Ratio', 70);
            column.cssClass = 'rightalign';

            column = igrid.AddSimpleColumn('acutratio', 'M.Ratio', 70);
            column.cssClass = 'rightalign';

            var column = igrid.AddCustomColumn('ccutratio', 'A.Ratio', 70, function (item, col) {
                if (item.grnqty > 0) {
                    if (item.cfabricqty > 0) {
                        return (item.cfabricqty / item.grnqty).toFixed(2);
                    }
                    else if (item.fabricqty > 0) {
                        return (item.fabricqty / item.grnqty).toFixed(2);
                    }
                }
                return '';
            });
            column.cssClass = 'rightalign';
            column.getValue = function (item, col) {
                if (item.grnqty > 0) {
                    if (item.cfabricqty > 0) {
                        return (item.cfabricqty / item.grnqty).round(2);
                    }
                    else if (item.fabricqty > 0) {
                        return (item.fabricqty / item.grnqty).round(2);
                    }
                }
                return 0;
            }

            groupColumns.push(
                {
                    id: 'quantity',
                    name: 'Qty',
                    headerCssClass: 'text-center',
                    columns: igrid.columns
                });

            igrid.columns = [];

            $scope.system_possizelist = $rootScope.UserInfo.appsettings.system_possizelist;
            if (!($scope.system_possizelist && $scope.system_possizelist != '' && $scope.system_possizelist.length > 0)) {
                var sizelist = [];
                $.each($scope.WOSummaryList, function (k, v) {
                    $.each(v.sizes, function (kk, vv) {

                        var exist = sizelist.find(function (item) {
                            return item == vv.name;
                        })
                        if (typeof (exist) == 'undefined' || exist == null) {
                            sizelist.push(vv.name);
                        }

                    });
                });
                $scope.system_possizelist = sizelist.join(',');
            }

            $.each($scope.system_possizelist.split(','), function (k, v) {

                var column = igrid.AddCustomColumn('size-o-' + k, 'O', 40, function (item, col) {
                    var value = item.sizes.find(function (item) {
                        return item.name == col.internal;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return '';

                    return value.qty;
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var value = item.sizes.find(function (item) {
                        return item.name == col.internal;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return 0;

                    return value.qty;
                }

                var column = igrid.AddCustomColumn('size-r-' + k, 'R', 40, function (item, col) {
                    var value = item.sizes.find(function (item) {
                        return item.name == col.internal;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return '';

                    return value.gqty;
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var value = item.sizes.find(function (item) {
                        return item.name == col.internal;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return 0;

                    return value.gqty;
                }

                var column = igrid.AddCustomColumn('size-d-' + k, 'D', 40, function (item, col) {
                    var value = item.sizes.find(function (item) {
                        return item.name == col.internal;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return '';

                    return value.qty - value.gqty;
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var value = item.sizes.find(function (item) {
                        return item.name == col.internal;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return 0;

                    return value.qty - value.gqty;
                }

                groupColumns.push(
                    {
                        id: 'size_' + v,
                        name: v,
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });

                igrid.columns = [];
            });

            var stagelist = [];
            $.each($scope.WOSummaryList, function (k, v) {
                $.each(v.stages, function (kk, vv) {

                    if (vv.stage != null && vv.stage != '') {
                        var exist = stagelist.find(function (item) {
                            return item.corestageid == vv.corestageid && item.stage == vv.stage;
                        });

                        if (typeof (exist) == 'undefined' || exist == null) {
                            stagelist.push({ 'corestageid': vv.corestageid, 'stage': vv.stage });
                        }
                    }
                });
            });


            $.each(stagelist, function (k, v) {

                var column = igrid.AddCustomColumn('Grp-Val-' + k, 'I - ' + v.stage, 100, function (item, col) {
                    var value = item.stages.find(function (item) {
                        return item.corestageid == col.internal.corestageid &&
                            item.stage == col.internal.stage;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return '';

                    return value.issuedqty;
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var value = item.stages.find(function (item) {
                        return item.corestageid == col.internal.corestageid &&
                            item.stage == col.internal.stage;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return 0;

                    return value.issuedqty;
                }

                var column = igrid.AddCustomColumn('Grp-Val-' + k, 'R - ' + v.stage, 100, function (item, col) {
                    var value = item.stages.find(function (item) {
                        return item.corestageid == col.internal.corestageid &&
                            item.stage == col.internal.stage;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return '';

                    return value.issuedqty;
                });
                column.internal = v;
                column.cssClass = 'rightalign';
                column.total = true;
                column.getValue = function (item, col) {
                    var value = item.stages.find(function (item) {
                        return item.corestageid == col.internal.corestageid &&
                            item.stage == col.internal.stage;
                    });
                    if (typeof (value) == 'undefined' || value == null)
                        return 0;

                    return value.receivedqty;
                }

                groupColumns.push(
                    {
                        id: 'stage_' + v.stage,
                        name: v.stage,
                        headerCssClass: 'text-center',
                        columns: igrid.columns
                    });

                igrid.columns = [];
            });
            igrid.columns = groupColumns;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.WOSummaryList);
        }


        $scope.SKUConfig = function () {

            $scope.system_possizelist = $rootScope.UserInfo.appsettings.system_possizelist;
            $scope.SizeIndexList = [];
            if ($scope.system_possizelist && $scope.system_possizelist != null) {
                $scope.SizeIndexList = $scope.system_possizelist.split(',');
            }

            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddSimpleColumn('name', 'Name', 100);
            igrid.AddReferenceColumn('supplierid', 'Supplier', 150, $scope.Supplier);
            igrid.AddReferenceColumn('productid', 'Product', 120, $scope.Product);
            igrid.AddReferenceColumn('fabricid', 'Fabric', 120, $scope.Fabrics);
            igrid.AddSimpleColumn('mfgyear', 'Year', 50);
            igrid.AddSimpleColumn('mfgmonth', 'Month', 50);
            igrid.AddSimpleColumn('colour', 'Colour', 120);

            $.each(slist, function (k, v) {
                var column = igrid.AddCustomColumn('size_' + k, v, 30, function (item, col) {

                    return '';
                });
                column.cssClass = 'rightalign';
                column.size = v;
                column.sortable = true;
                column.total = true;
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
            })


            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SKUResult);
        }

        $scope.BOMOutwardConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddFormatedColumn('packedon', 'Entry Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('fromdepartmentid', 'From department', 100, $scope.DepartmentList);
            igrid.AddReferenceColumn('todepartmentid', 'To department', 100, $scope.DepartmentList);
            igrid.AddReferenceColumn('fromlocationid', 'From Location', 100, $scope.WHLocation);
            igrid.AddReferenceColumn('tolocationid', 'To Location', 100, $scope.AllLocation);
            igrid.AddReferenceColumn('fromdepartmentid', 'From department', 100, $scope.DepartmentList);
            igrid.AddSimpleColumn('qty', 'QTY', 50);
            igrid.AddReferenceColumn('packedby', 'Packed By', 150, $scope.Employee);

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.OutwardList);
        }

        $scope.BOMInwardConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddFormatedColumn('receivedon', 'Received Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('fromdepartmentid', 'From department', 100, $scope.DepartmentList);
            igrid.AddReferenceColumn('todepartmentid', 'To department', 100, $scope.DepartmentList);
            igrid.AddReferenceColumn('fromlocationid', 'From Location', 100, $scope.WHLocation);
            igrid.AddReferenceColumn('tolocationid', 'To Location', 100, $scope.AllLocation);
            igrid.AddReferenceColumn('fromdepartmentid', 'From department', 100, $scope.DepartmentList);
            igrid.AddSimpleColumn('qty', 'QTY', 50);
            igrid.AddReferenceColumn('receivedby', 'Received By', 150, $scope.Employee);

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.InwardList);
        }



        $scope.TailoringConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('code', 'MachineNo', 100);
            igrid.AddReferenceColumn('employeeid', 'Employee', 100, $scope.Employee);
            igrid.AddFormatedColumn('issuedon', 'Issued Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('receivedon', 'Received Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('designid', 'Design No', 100);

            igrid.AddSimpleColumn('qty', 'QTY', 50);
            igrid.AddReferenceColumn('receivedby', 'Approved By', 150, $scope.Employee);

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.InwardList);
        }

        $scope.ReportList = null;
        $scope.SearchWorkorderSampleStatus = function () {

            if ($scope.ReportList == null) {
                restService.Get('ConfigurationService/ReportList?id=WORKORDER', true).then(function (results) {
                    $scope.ReportList = results;
                });
            }

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchWorkorderSampleStatus",
                data: srequest
            }).success(function (result) {

                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.AddFormatedColumn('wodate', 'WO Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('workorderno', 'WO No', 100);

                igrid.AddSimpleColumn('division', 'Division', 100);
                igrid.AddSimpleColumn('entryno', 'Sample Code', 100);
                igrid.AddSimpleColumn('samplename', 'Sample name', 100);
                igrid.AddSimpleColumn('customername', 'Customer', 100);
                igrid.AddSimpleColumn('productform', 'Form', 100);
                igrid.AddSimpleColumn('designno', 'Design', 100);

                igrid.AddFormatedColumn('outqty', 'Mfg Issue', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('inqty', 'Mfg Received', 80, 'N2', 0).total = true;
                igrid.AddSimpleColumn('sono', 'S.O No', 100);
                igrid.AddFormatedColumn('sodue', 'Due', 50, 'N0', 0);
                igrid.AddSimpleColumn('status', 'Status', 100);
                igrid.AddFormatedColumn('oqty', 'SO Received', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('rqty', 'SO Delivered', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('balance', 'Balance', 80, 'N0', 0).total = true;
                igrid.AddFormatedColumn('amount', 'SO Value', 80, 'N2', 0).total = true;
                igrid.AddSimpleColumn('sobookno', 'Booking No', 100);
                igrid.AddSimpleColumn('billno', 'Bill No', 100);
                igrid.AddFormatedColumn('price', 'Price', 50, 'N4', 4);
                igrid.AddReferenceColumn('preparedby', 'Prepared', 100, $scope.Employee);
                igrid.AddReferenceColumn('agentid', 'Agent', 100, $scope.Employee);
                igrid.AddButtonColumn('Print', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' class='btn btn-danger btn-xs'><i class='fa fa-print' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
                igrid.Initialize();
                igrid.grid.onClick.subscribe(function (e) {
                    var cell = igrid.grid.getCellFromEvent(e);
                    var item = igrid.grid.getDataItem(cell.row);
                    var command = igrid.grid.getColumns()[cell.cell].id;
                    if (command == 'Print') {
                        $scope.$apply(function () {
                            $scope.PrintWorkOrderStatus_WO(item.mfgworkorderid);
                        });
                    }
                });
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result);

            }).error(function (result) {
            });
        }

        $scope.PrintWorkOrderStatus_WO = function (workorderid) {
            $timeout(function () {

                var url = 'MfgworkorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + workorderid;
                if ($scope.ReportList.length == 1) {
                    $rootScope.Download(url, 'application.pdf', 'application/pdf');
                }
                else {
                    var dialog = ngDialog.open({
                        template: 'ChooseTemplate',
                        data: {
                            'scope': $scope
                        },
                        width: '500px',
                        controller: ['$scope', '$timeout', function ($scope, $timeout) {
                            $scope.ReportList = $scope.ngDialogData.scope.ReportList;
                            $scope.Selected = function () {
                                $scope.closeThisDialog({
                                    'templateid': $scope.templateid
                                });
                            };
                        }],
                        className: 'ngdialog-theme-plain'
                    });
                    dialog.closePromise.then(function (data) {
                        {
                            if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                                url += '/' + data.value.templateid;
                                $rootScope.Download(url, 'application.pdf', 'application/pdf');
                            }
                        }
                    });
                }


                //var link = document.createElement('a');
                //link.download = "workorder.pdf";
                //link.href = 'MfgworkorderService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (workorderid) != 'undefined' ? workorderid : $scope.WorkOrder.id);
                //document.body.appendChild(link);
                //link.click();
                //$timeout(function () {
                //    link.remove();
                //}, 100);
            }, 10);
        }

        $scope.SearchPSComboReport = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PackingslipService/ComboReport",
                data: srequest
            }).success(function (result) {

                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.AddFormatedColumn('packedon', 'Packed Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('packingno', 'Packing No', 100);

                if ($scope.SearchCondition.auditactiontype == 9) {
                    igrid.AddSimpleColumn('designid', 'Design', 100);
                    igrid.AddSimpleColumn('product', 'Product', 100);
                    igrid.AddSimpleColumn('colour', 'Colour', 100);
                    igrid.AddSimpleColumn('size', 'Size', 100);
                    igrid.AddFormatedColumn('qty', 'Planned', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('packqty', 'Packed', 80, 'N2', 0).total = true;
                }
                else {
                    igrid.AddSimpleColumn('customer', 'Customer', 100);
                    igrid.AddSimpleColumn('product', 'Product', 100);
                    igrid.AddSimpleColumn('itemdesigncode', 'Design', 100);
                    igrid.AddSimpleColumn('barcode', 'Combo', 100);

                    var column = igrid.AddCustomColumn('mbarcode', 'Merged', 100, function (item) {
                        if (item.barcode == item.mbarcode) return '';
                        return item.barcode;
                    });
                    column.getValue = function (item, col) {
                        if (item.barcode == item.mbarcode) return '';
                        return item.barcode;
                    };
                    var column = igrid.AddCustomColumn('mqty', 'M.Qty', 100, function (item) {
                        if (item.barcode == item.mbarcode) return '';
                        return item.mqty;
                    });
                    column.getValue = function (item, col) {
                        if (item.barcode == item.mbarcode) return 0;
                        return item.mqty;
                    };
                    igrid.AddSimpleColumn('colour', 'Colour', 100);
                    igrid.AddSimpleColumn('size', 'Size', 100);
                    igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 0).total = true;

                    igrid.AddFormatedColumn('price', 'Price', 80, 'N2', 0);

                    igrid.AddSimpleColumn('sono', 'SO No', 100);
                    igrid.AddSimpleColumn('billno', 'Bill No', 100);
                    igrid.AddReferenceColumn('packedby', 'Employee', 100, $scope.Employee);
                    igrid.AddSimpleColumn('createdby', 'Created By', 100);
                    igrid.AddSimpleColumn('modifiedby', 'Modifed By', 100);
                }

                igrid.options.autoEdit = true;
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result);

            }).error(function (result) {
            });
        }


        $scope.SearchMfgUANBundleReport = function () {
            $scope.SearchCondition.customfields = [];
            $.each($scope.CustomFields, function (k, v) {
                if (v.selected)
                    $scope.SearchCondition.customfields.push(v);
            })
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchMfgUANBundleReport",
                data: srequest
            }).success(function (result) {
                if (result) {

                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;

                    igrid.AddReferenceColumn('prddivisionid', 'Division', 100, $scope.ProductDivision);
                    igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
                    igrid.AddReferenceColumn('classid', 'Class', 100, $scope.Classification);
                    igrid.AddSimpleColumn('design', 'Design', 200);
                    igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
                    igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.Sizes);

                    $.each($scope.SearchCondition.customfields, function (k, v) {
                        var column = igrid.AddCustomColumn(v.field.cfname, v.display, 100, function (item, col) {
                            return item.customfields[col.index];
                        });
                        column.index = k;
                    })

                    igrid.AddSimpleColumn('uid', 'UAN', 200);
                    igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);
                    igrid.AddSimpleColumn('code', 'Pack Code', 200);
                    igrid.AddFormatedColumn('packedon', 'Issued Date', 80, 'dd-MM-yyyy');
                    igrid.AddFormatedColumn('receivedon', 'Received Date', 80, 'dd-MM-yyyy');
                    var column = igrid.AddCustomColumn('status', 'Status', 100, function (item, col) {
                        if (item.status < 0)
                            return 'Requested';
                        if (item.status == 0)
                            return 'Pending';
                        if (item.status == 9)
                            return 'Rejected';
                        return 'Completed';
                    });
                    igrid.AddReferenceColumn('fromlocationid', 'From', 100, $scope.AllLocation);
                    igrid.AddReferenceColumn('tolocationid', 'To', 100, $scope.AllLocation);
                    igrid.AddReferenceColumn('packedby', 'Issued By', 100, $scope.Employee);
                    igrid.AddReferenceColumn('receivedby', 'Received By', 100, $scope.Employee);
                    igrid.AddFormatedColumn('requestqty', 'Requested', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('issuedqty', 'Transfered', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('acceptedqty', 'Inwarded', 80, 'N2', 2).total = true;
                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData(result);

                }
            }).error(function (result) {
            });
        }

        $scope.SearchAsOnStockTransaction = function (barcodehistory) {

            $scope.SearchConditionStkTrans.fields = [];
            $.each($scope.AnalyserOptions, function (k, v) {
                if (v.selected)
                    $scope.SearchConditionStkTrans.fields.push(v.id);
            });
            $scope.StockTransactionResult = [];
            $scope.SearchConditionStkTrans.history = barcodehistory;
            request = {
                "item": $scope.SearchConditionStkTrans,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "AnalyserService/SearchAsOnMfgStock",
                data: srequest
            }).success(function (result) {

                if (barcodehistory) {
                    $scope.StockTransactionResult = result;
                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var modes = [{ 'id': 'OP', 'name': 'Opening' },
                    { 'id': 'PU', 'name': 'Purchase' },
                    { 'id': 'PR', 'name': 'PurchaseReturn' },
                    { 'id': 'TI', 'name': 'TransferIn' },
                    { 'id': 'TO', 'name': 'TransferOut' },
                    { 'id': 'SL', 'name': 'Sales' },
                    { 'id': 'AU', 'name': 'Audit' },
                    { 'id': 'MS', 'name': 'Opening' },
                    { 'id': 'JN', 'name': 'Journal' },
                    { 'id': 'DM', 'name': 'Damage' }
                    ]

                    var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
                    igrid.options.showHeaderRow = true;
                    igrid.EnableSerial($scope.StockTransactionResult);
                    igrid.AddFormatedColumn('ason', 'Date', 100, 'dd-MM-yyyy');
                    igrid.AddSimpleColumn('username', 'User', 80);
                    igrid.AddSimpleColumn('mode', 'Transaction', 80);

                    var column = igrid.AddFormatedColumn('ob', 'Opening', 80, 'N', 2);
                    column.cssClass = 'rightalign';

                    column = igrid.AddFormatedColumn('ti', 'Stock In', 80, 'N', 2);
                    column.cssClass = 'rightalign';
                    column.total = true;

                    column = igrid.AddFormatedColumn('to', 'Stock Out', 80, 'N', 2);
                    column.cssClass = 'rightalign';
                    column.total = true;

                    column = igrid.AddFormatedColumn('cb', 'Clsoing', 80, 'N', 2);
                    column.cssClass = 'rightalign';

                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData($scope.StockTransactionResult);
                }
                else {
                    $scope.StockTransactionResult = result.mfgitems;
                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '470px', '100%', true, false, true, 0);
                    igrid.options.showHeaderRow = true;
                    igrid.EnableSerial($scope.StockTransactionResult);

                    igrid.AddFormatedColumn('opening', 'Opening', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('PU', 'Purchase', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('MS', 'Direct Stock', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('PR', 'P.Return', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('TI', 'Transer In', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('TO', 'Transfer Out', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('SL', 'Sales', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('AU', 'Audit', 80, 'N', 2).total = true;
                    igrid.AddFormatedColumn('closing', 'Clsoing', 80, 'N', 2).total = true;

                    igrid.AddReferenceColumn('prddivisionid', 'Division', 100, $scope.ProductDivision);
                    igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
                    igrid.AddReferenceColumn('classid', 'Class', 100, $scope.Classification);
                    igrid.AddSimpleColumn('design', 'Design', 200);
                    igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
                    igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.Sizes);
                    igrid.AddSimpleColumn('uid', 'UAN', 200);
                    igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);

                    $.each(result.display, function (k, v) {
                        var option = $scope.AnalyserOptions.find(function (item) {
                            return item.id == v;
                        });

                        var column = igrid.AddCustomColumn('col_' + k,
                            option.name, 80, function (item, col) {
                                return item.values[col.internal];
                            });
                        column.internal = k;
                    });

                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData($scope.StockTransactionResult);
                }

            }).error(function (result) {
            });
        }

        $scope.SearchMfgUANSales = function () {
            $scope.SearchCondition.customfields = [];
            $.each($scope.CustomFields, function (k, v) {
                if (v.selected)
                    $scope.SearchCondition.customfields.push(v);
            })
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchUANSalesReport",
                data: srequest
            }).success(function (result) {
                if (result) {

                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;

                    igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.AllLocation);
                    igrid.AddReferenceColumn('prddivisionid', 'Division', 100, $scope.ProductDivision);
                    igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
                    igrid.AddReferenceColumn('classid', 'Class', 100, $scope.Classification);
                    igrid.AddSimpleColumn('design', 'Design', 200);
                    igrid.AddSimpleColumn('hsn', 'HSN', 200);
                    igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
                    igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.Sizes);

                    $.each($scope.SearchCondition.customfields, function (k, v) {
                        var column = igrid.AddCustomColumn(v.field.cfname, v.display, 100, function (item, col) {
                            return item.customfields[col.index];
                        });
                        column.index = k;
                    })
                    igrid.AddSimpleColumn('uid', 'UAN', 200);
                    igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);
                    igrid.AddSimpleColumn('customer', 'Customer', 200);
                    igrid.AddSimpleColumn('category', 'Category', 200);
                    igrid.AddSimpleColumn('billno', 'Bill No', 200);
                    igrid.AddFormatedColumn('billdate', 'Bill Date', 80, 'dd-MM-yyyy');
                    igrid.AddSimpleColumn('sono', 'SO NO', 200);
                    var ecomentry = result.find(function (item) {
                        return item.morderid &&
                            item.morderid != null &&
                            item.morderid != '';
                    });
                    if (ecomentry && ecomentry != null) {
                        igrid.AddSimpleColumn('morderid', 'ECOM ID', 200);
                        igrid.AddSimpleColumn('channel', 'CHANNEL', 200);
                        igrid.AddSimpleColumn('einvoicecode', 'ECOM INV', 200);
                        igrid.AddSimpleColumn('pos', 'POS', 200);
                    }
                    igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('taxpercentage', 'Tax %', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('price', 'Price', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('additionaldiscount', 'Discount', 80, 'N2', 0).total = true;
                    var column = igrid.AddCustomColumn('Taxable', 'Taxable', 100, function (item, col) {
                        var cellresult = item.receivable - item.tax;
                        if (cellresult && cellresult != null) return $filter('IndianNumber')(cellresult, 2);
                        return '';
                    });
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        return item.receivable - item.tax;
                    }

                    column = igrid.AddCustomColumn('SGST', 'SGST', 100, function (item, col) {
                        if (!item.isinterstatesale) {
                            return $filter('IndianNumber')(item.tax / 2, 2);
                        }
                        return '';
                    });
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        if (!item.isinterstatesale) {
                            return (item.tax / 2).round(2);
                        }
                        return 0;
                    }

                    column = igrid.AddCustomColumn('CGST', 'CGST', 100, function (item, col) {
                        if (!item.isinterstatesale) {
                            return $filter('IndianNumber')(item.tax / 2, 2);
                        }
                        return '';
                    });
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        if (!item.isinterstatesale) {
                            return (item.tax / 2).round(2);
                        }
                        return 0;
                    }

                    column = igrid.AddCustomColumn('IGST', 'IGST', 100, function (item, col) {
                        if (item.isinterstatesale) {
                            return $filter('IndianNumber')(item.tax, 2);
                        }
                        return '';
                    });
                    column.cssClass = 'rightalign';
                    column.total = true;
                    column.getValue = function (item, col) {
                        if (item.isinterstatesale) {
                            return (item.tax).round(2);
                        }
                        return 0;
                    }


                    igrid.AddFormatedColumn('receivable', 'Net', 80, 'N2', 0).total = true;
                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData(result);

                }
            }).error(function (result) {
            });
        }

        $scope.SearchMfgUANSOReport = function () {
            $scope.SearchCondition.customfields = [];
            $.each($scope.CustomFields, function (k, v) {
                if (v.selected)
                    $scope.SearchCondition.customfields.push(v);
            })
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchUANSOReport",
                data: srequest
            }).success(function (result) {
                if (result) {

                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;

                    igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.AllLocation);
                    igrid.AddReferenceColumn('prddivisionid', 'Division', 100, $scope.ProductDivision);
                    igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
                    igrid.AddReferenceColumn('classid', 'Class', 100, $scope.Classification);
                    igrid.AddSimpleColumn('design', 'Design', 200);
                    igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
                    igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.Sizes);

                    $.each($scope.SearchCondition.customfields, function (k, v) {
                        var column = igrid.AddCustomColumn(v.field.cfname, v.display, 100, function (item, col) {
                            return item.customfields[col.index];
                        });
                        column.index = k;
                    })

                    igrid.AddSimpleColumn('uid', 'UAN', 200);
                    igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);

                    igrid.AddSimpleColumn('customer', 'Customer', 200);
                    igrid.AddSimpleColumn('sono', 'SO NO', 200);
                    igrid.AddSimpleColumn('sobookno', 'PO NO', 200);
                    igrid.AddFormatedColumn('podate', 'SO Date', 80, 'dd-MM-yyyy');
                    igrid.AddFormatedColumn('expecteddate', 'Expected Date', 80, 'dd-MM-yyyy');
                    igrid.AddFormatedColumn('orderqty', 'Order Qty', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('billedqty', 'Billed Qty', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('rejecteddqty', 'Rejected Qty', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('cancelledqty', 'Cancelled Qty', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('price', 'Cost', 80, 'N2', 0).total = true;

                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData(result);

                }
            }).error(function (result) {
            });
        }

        $scope.SearchMfgUANPOReport = function () {
            $scope.SearchCondition.customfields = [];
            $.each($scope.CustomFields, function (k, v) {
                if (v.selected)
                    $scope.SearchCondition.customfields.push(v);
            })
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchUANPOReport",
                data: srequest
            }).success(function (result) {
                if (result) {

                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;

                    igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.AllLocation);
                    igrid.AddReferenceColumn('prddivisionid', 'Division', 100, $scope.ProductDivision);
                    igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
                    igrid.AddReferenceColumn('classid', 'Class', 100, $scope.Classification);
                    igrid.AddSimpleColumn('design', 'Design', 200);
                    igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
                    igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.Sizes);

                    $.each($scope.SearchCondition.customfields, function (k, v) {
                        var column = igrid.AddCustomColumn(v.field.cfname, v.display, 100, function (item, col) {
                            return item.customfields[col.index];
                        });
                        column.index = k;
                    })

                    igrid.AddSimpleColumn('uid', 'UAN', 200);
                    igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);

                    igrid.AddSimpleColumn('supplier', 'Supplier', 200);
                    igrid.AddSimpleColumn('pono', 'PO NO', 200);
                    igrid.AddFormatedColumn('podate', 'PO Date', 80, 'dd-MM-yyyy');
                    igrid.AddFormatedColumn('expecteddate', 'Expected Date', 80, 'dd-MM-yyyy');
                    igrid.AddFormatedColumn('expirydate', 'Expiry Date', 80, 'dd-MM-yyyy');
                    igrid.AddFormatedColumn('orderqty', 'Order Qty', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('receivedqty', 'Received Qty', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('cancelledqty', 'Cancelled Qty', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('price', 'Cost', 80, 'N2', 0).total = true;

                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData(result);

                }
            }).error(function (result) {
            });
        }

        $scope.SearchMfgUANInvoice = function () {
            $scope.SearchCondition.customfields = [];
            $.each($scope.CustomFields, function (k, v) {
                if (v.selected)
                    $scope.SearchCondition.customfields.push(v);
            })
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchMfgUANInvoice",
                data: srequest
            }).success(function (result) {
                if (result) {

                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;

                    igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.AllLocation);
                    igrid.AddReferenceColumn('prddivisionid', 'Division', 100, $scope.ProductDivision);
                    igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
                    igrid.AddReferenceColumn('classid', 'Class', 100, $scope.Classification);
                    igrid.AddSimpleColumn('design', 'Design', 200);
                    igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
                    igrid.AddReferenceColumn('sizeid', 'Size', 100, $scope.Sizes);

                    $.each($scope.SearchCondition.customfields, function (k, v) {
                        var column = igrid.AddCustomColumn(v.field.cfname, v.display, 100, function (item, col) {
                            return item.customfields[col.index];
                        });
                        column.index = k;
                    })

                    igrid.AddSimpleColumn('uid', 'UAN', 200);
                    igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);
                    igrid.AddSimpleColumn('supplier', 'Supplier', 200);
                    igrid.AddSimpleColumn('entryno', 'GRN No', 200);
                    igrid.AddFormatedColumn('entrydate', 'GRN Date', 89, 'dd-MM-yyyy');
                    igrid.AddSimpleColumn('pono', 'PO NO', 200);
                    igrid.AddFormatedColumn('podate', 'PO Date', 80, 'dd-MM-yyyy');
                    igrid.AddSimpleColumn('invoiceno', 'Invoice No', 200);
                    igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 80, 'dd-MM-yyyy');
                    igrid.AddFormatedColumn('orderqty', 'Order Qty', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('grnqty', 'Bill Qty', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('actqty', 'GRN Qty', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('buyingprice', 'Cost', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('grnvalue', 'GRN Value', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('debitqty', 'Debit Qty', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('debitvalue', 'Debit Value', 80, 'N2', 0).total = true;

                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData(result);

                }
            }).error(function (result) {
            });
        }

        $scope.SearchMfgUANStock = function () {
            $scope.SearchCondition.customfields = [];
            $.each($scope.CustomFields, function (k, v) {
                if (v.selected)
                    $scope.SearchCondition.customfields.push(v);
            })
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgReportService/SearchMfgUANStock",
                data: srequest
            }).success(function (result) {
                if (result) {

                    if ($scope.igrid != null)
                        $scope.igrid.destroy();

                    var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                    igrid.options.showHeaderRow = true;

                    igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.AllLocation);
                    igrid.AddReferenceColumn('prddivisionid', 'Division', 100, $scope.ProductDivision);
                    igrid.AddReferenceColumn('productid', 'Product', 100, $scope.Product);
                    igrid.AddReferenceColumn('classid', 'Class', 100, $scope.Classification);
                    igrid.AddSimpleColumn('design', 'Design', 100);
                    igrid.AddReferenceColumn('colourid', 'Colour', 100, $scope.AllColorList);
                    igrid.AddReferenceColumn('sizeid', 'Size', 60, $scope.Sizes);

                    $.each($scope.SearchCondition.customfields, function (k, v) {
                        var column = igrid.AddCustomColumn(v.field.cfname, v.display, 60, function (item, col) {
                            return item.customfields[col.index];
                        });
                        column.index = k;
                    })

                    igrid.AddSimpleColumn('uid', 'UAN', 100);
                    igrid.AddFormatedColumn('mrp', 'MRP', 80, 'N2', 2);
                    igrid.AddFormatedColumn('stock', 'Stock', 80, 'N2', 2).total = true;
                    igrid.AddFormatedColumn('so', 'Commited', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('sto', 'Transit', 80, 'N2', 0).total = true;
                    var column = igrid.AddCustomColumn('possible', 'Possible', 80, function (item, col) {
                        return item.stock - (item.so + item.sto);
                    });
                    column.getValue = function (item, col) {
                        return item.stock - (item.so + item.sto);
                    }
                    column.total = true;
                    column.cssClass = 'rightalign';
                    igrid.EnableTotal();
                    igrid.Initialize();
                    $scope.igrid = igrid;
                    $scope.igrid.UpdateData(result);

                }
            }).error(function (result) {
            });
        }

        $scope.SearchMfgIncentive = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "IncentivesService/MfgIncentive",
                data: srequest
            }).success(function (result) {

                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.AddSimpleColumn('product', 'Product', 200);

                igrid.AddFormatedColumn('percentage', '%', 80, 'N2', 2);
                igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('sales', 'Sales', 80, 'N2', 0).total = true;
                igrid.AddFormatedColumn('incentive', 'Incentive', 80, 'N2', 0).total = true;
                igrid.EnableTotal();
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result);

            }).error(function (result) {
            });
        }

        $scope.ShowPackingSlipSOSummary = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var newsolist = [];

            $.each($scope.PackingSlipList, function (k, v) {

                if (v.sopendingqty == 0 && v.pqty == 0) return;

                var soitem = newsolist.find(function (item) {
                    return item.sono == v.sono;
                });
                if (soitem == undefined || soitem == null) {
                    soitem = v;
                    soitem.list = [];
                    soitem.noofpack = 1;
                    newsolist.push(soitem);
                }
                else {
                    soitem.packingno += ',' + v.packingno;
                    soitem.noofpack += 1;
                }
                soitem.list.push(v);
            });

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sono', 'SO No', 100);
            igrid.AddFormatedColumn('sopendingqty', 'SO Qty', 100, 'N0', 0).total = true;
            igrid.AddSimpleColumn('packingno', 'Packing No', 200);
            igrid.AddFormatedColumn('pqty', 'Unbilled Pack Qty', 100, 'N0', 0).total = true;
            igrid.AddFormatedColumn('noofpack', 'No Of Package', 100, 'N0', 0).total = true;

            var column = igrid.AddCustomColumn('UnpackedSO', 'Unpacked SO', 100,
                function (item, col) {
                    return item.sopendingqty - item.pqty;
                });
            column.enablefilter = true;
            column.sortable = true;
            column.cssClass = 'rightalign';
            column.total = true;
            column.filtertype = 'number';
            column.getValue = function (item, col) {
                return item.sopendingqty - item.pqty;
            }

            igrid.AddButtonColumn('Print', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Print' class='btn btn-danger btn-xs'><i class='fa fa-print' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.EnableTotal();
            igrid.options.autoEdit = true;
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Print') {
                    if (item.sopendingqty - item.pqty <= 0) {
                        $rootScope.Toaster.Error("Don't have unpacked so!");
                        return;
                    }
                    $scope.$apply(function () {
                        $rootScope.Download('SalesorderService/PrintUnpackedSO/' + $scope.UserInfo.OrganizationID + '/' +
                            $scope.qEncodeURIComponent(item.sono), 'salesorder.pdf', 'application/pdf');
                    });
                }
            });

            $scope.igrid = igrid;
            $scope.igrid.UpdateData(newsolist, 'id');

        }

        $scope.PackingSlipList = null;
        $scope.ShowPackingSummary = false;
        $scope.SearchPackingSlip = function () {

            $scope.ShowPackingSummary = false;
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: $scope.SearchCondition.modeid == 1 ? "PackingslipService/ItemReport" : "PackingslipService/SummaryReport",
                data: srequest
            }).success(function (result) {
                $scope.ShowPackingSummary = $scope.SearchCondition.modeid != 1;
                $scope.PackingSlipList = result;
                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
                igrid.options.showHeaderRow = true;
                igrid.AddFormatedColumn('packedon', 'Packed Date', 80, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('packingno', 'Packing No', 100);
                igrid.AddSimpleColumn('customer', 'Customer', 100);

                if ($scope.SearchCondition.modeid == 1) {
                    igrid.AddSimpleColumn('product', 'Product', 100);
                    igrid.AddSimpleColumn('itemdesigncode', 'Design', 100);
                    igrid.AddSimpleColumn('barcode', 'Barcode', 100);
                    igrid.AddSimpleColumn('colour', 'Colour', 100);
                    igrid.AddSimpleColumn('size', 'Size', 100);
                    igrid.AddFormatedColumn('qty', 'Qty', 80, 'N2', 0).total = true;
                    igrid.AddFormatedColumn('price', 'Price', 80, 'N2', 0);
                }
                else {
                    igrid.AddFormatedColumn('packqty', 'Pack Qty', 80, 'N0', 0).total = true;
                    igrid.AddFormatedColumn('soqty', 'SO Qty', 80, 'N0', 0).total = true;
                }
                igrid.AddSimpleColumn('sono', 'SO No', 100);
                igrid.AddSimpleColumn('billno', 'Bill No', 100);
                igrid.AddReferenceColumn('packedby', 'Employee', 100, $scope.Employee);
                igrid.AddSimpleColumn('createdby', 'Created By', 100);
                igrid.AddSimpleColumn('modifiedby', 'Modifed By', 100);

                igrid.EnableTotal();
                igrid.options.autoEdit = true;
                igrid.Initialize();
                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result);

            }).error(function (result) {
            });
        }

        $scope.BOMInwardDetailConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddReferenceColumn('bomcategoryid', 'BOM Category', 150, $scope.BOMCategories);

            var column = igrid.AddCustomColumn('bomid', 'Component', 100, function (item) {
                var bom = $scope.BOMAllList.find(function (k) {
                    return k.id == item.bomid;
                });
                if (bom && bom != null) return bom.title;
                return '-';
            });


            igrid.AddSimpleColumn('qty', 'QTY', 120).total = true;

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.InwardDetailList.items);
        }


        $scope.exportExcel = function (tableid, title, removeactioncolumn) {
            if (tableid == "lrentryreport")
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'LR ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'lrinvoicereport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'INVOICE ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchaseentry')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE ENTRY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchasehsn')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE HSN REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchaseretsum')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE RETURN SUMMARY REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'purchaseretsumcancel')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PURCHASE RETURN CANCEL REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));
            if (tableid == 'inwardreport')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'OUTWARD REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.locationid, $scope.Location) + " ) ");
            if (tableid == 'outward')
                $scope.exportHref = Excel.tableToExcel(tableid, title, removeactioncolumn, 'PENDING OUTWARD REPORT FOR - ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " - " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy'));

            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = title + ".xlsx";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.exporttoXSLX = function (infygrid, subtitle) {
            var title = $rootScope.UserInfo.CompanyName;
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);
            infygrid.ExportXLSX(infygrid.grid, title, subtitle);
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == 'StockReport')
                subtitle = 'Designwise Stock Report';
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

        $scope.CustomFields = [];
        $scope.system_sizegrouplayout = false;
        $scope.system_customfields = false;
        $scope.Appsetting_Company = false;
        $scope.LoadApplicationSetting = function () {
            if ($rootScope.UserInfo &&
                $rootScope.UserInfo.appsettings) {
                $scope.Appsetting_Company = true;
                $scope.system_customfields = $rootScope.UserInfo.appsettings.system_customfields || false;
                $scope.system_sizegrouplayout = $rootScope.UserInfo.appsettings.system_sizegrouplayout || false;
                if ($scope.system_customfields) {
                    restService.Get('ReferencelistService/FULL-CUSTOMFIELDS', true).then(function (results) {
                        $scope.CustomFields = [];
                        $.each(results, function (k, v) {
                            if (v.additionalinfo &&
                                v.additionalinfo.customfield &&
                                v.additionalinfo.customfield.cfbuilt) {
                                $scope.CustomFields.push({
                                    'display': v.name,
                                    'selected': false,
                                    'key': v.additionalinfo.customfield.cfname,
                                    'field': v.additionalinfo.customfield
                                });
                            }
                        });
                    });
                }
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }
        $scope.LoadApplicationSetting();

        var Options =
        {
            Product: 0,
            Brand: 1,
            Size: 2,
            Pattern: 3,
            Material: 4,
            Style: 5,
            Colour: 6,
            Type: 7,
            Section: 8,
            SaleLocation: 9,
            StockLocation: 10,
            Item: 105,
            Design: 11,
            SaleRate: 12,
            PurchaseRate: 13,
            DiscountRate: 14,
            SoldRate: 15,
            SaleRate_Range: 16,
            StockAge_Range: 17,
            PurchaseRate_Range: 18,
            DiscountRate_Range: 19,
            SoldRate_Range: 20,
            LREntryNo: 21,
            InvoiceNo: 22,
            InvDate: 23,
            SaleYear: 24,
            PurchaseYear: 25,
            PurchaseMonth: 26,
            SaleMonth: 27,
            BillNo: 28,
            BillDate: 29,
            Supplier: 30,
            Salesman: 31,
            Supplier_City: 32,
            Barcode: 33,
            HSNCode: 34,
            Tax: 35,
            SaleWeekDay: 36,
            SaleMonthDay: 37,
            SaleMargin_Range: 38,
            RetailMargin_Range: 39,
            Agent: 40,
            Counter: 48,
            Company: 49,
            ProductGroup: 50,
            ReturnReason: 51,
            SaleReason: 52,
            Area: 53,
            Season: 54,
            PurchaseDate: 84,
            Customer_Religion: 93,
            SourceSupplier: 94,
            ProductCode: 98,
            Fit: 99,
            Sleeve: 100,
            TaxPercentage: 102,
            GLNNO: 103,
            Floor: 104,
            StockMarker: 106,
            JobworkSupplier: 110,
            SellingMode: 111,
            BillType: 113,
            PriceTag: 114
        }
        $scope.Options = Options;
        $scope.AnalyserOptions = [
            { 'id': Options.Company, 'name': 'COMPANY' },
            { 'id': Options.Product, 'name': 'PRODUCT' },
            { 'id': Options.Brand, 'name': 'BRAND' },
            { 'id': Options.Size, 'name': 'SIZE' },
            { 'id': Options.Pattern, 'name': 'PATTERN' },
            { 'id': Options.Material, 'name': 'MATERIAL' },
            { 'id': Options.Style, 'name': 'STYLE' },
            { 'id': Options.Colour, 'name': 'COLOUR' },
            { 'id': Options.Type, 'name': 'TYPE' },
            { 'id': Options.Section, 'name': 'SECTION' },
            { 'id': Options.ProductGroup, 'name': 'PRODUCT GROUP' },
            { 'id': Options.ProductCode, 'name': 'PRODUCT CODE' },
            { 'id': Options.Fit, 'name': 'FIT' },
            { 'id': Options.Sleeve, 'name': 'SLEEVE' },
            { 'id': Options.PriceTag, 'name': 'PRICE TAG' },
            { 'id': Options.StockLocation, 'name': 'STOCK LOCATION' },
            { 'id': Options.Design, 'name': 'DESIGN' },
            { 'id': Options.SaleRate, 'name': 'SALE RATE' },
            { 'id': Options.PurchaseRate, 'name': 'PURCHASE RATE' },
            { 'id': Options.DiscountRate, 'name': 'DISCOUNT RATE' },
            { 'id': Options.LREntryNo, 'name': 'LRENTRY NO' },
            { 'id': Options.InvoiceNo, 'name': 'INVOICE NO' },
            { 'id': Options.InvDate, 'name': 'INVOICE DATE' },
            { 'id': Options.PurchaseDate, 'name': 'PURCHASE DATE' },
            { 'id': Options.Agent, 'name': 'AGENT' },
            { 'id': Options.Supplier, 'name': 'SUPPLIER' },
            { 'id': Options.Supplier_City, 'name': 'SUPPLIER CITY' },
            { 'id': Options.Barcode, 'name': 'BARCODE' },
            { 'id': Options.GLNNO, 'name': 'GLN NO' },
            { 'id': Options.HSNCode, 'name': 'HSN CODE' },
            { 'id': Options.TaxPercentage, 'name': 'TAX PERCENTAGE' },
            { 'id': Options.SourceSupplier, 'name': 'SOURCE SUPPLIER' }
        ];

        $scope.AnalyserOptions.sort(function (a, b) {
            var x = a.name.toLowerCase();
            var y = b.name.toLowerCase();
            return x < y ? -1 : x > y ? 1 : 0;
        });

        $scope.AnalyserFilters = [
            { 'id': Options.Company, 'name': 'COMPANY' },
            { 'id': Options.Product, 'name': 'PRODUCT' },
            { 'id': Options.Brand, 'name': 'BRAND' },
            { 'id': Options.Size, 'name': 'SIZE' },
            { 'id': Options.Pattern, 'name': 'PATTERN' },
            { 'id': Options.Material, 'name': 'MATERIAL' },
            { 'id': Options.Style, 'name': 'STYLE' },
            { 'id': Options.Colour, 'name': 'COLOUR' },
            { 'id': Options.Type, 'name': 'TYPE' },
            { 'id': Options.Section, 'name': 'SECTION' },
            { 'id': Options.ProductGroup, 'name': 'PRODUCT GROUP' },
            { 'id': Options.Fit, 'name': 'FIT' },
            { 'id': Options.Sleeve, 'name': 'SLEEVE' },
            { 'id': Options.Design, 'name': 'DESIGN' },
            { 'id': Options.Barcode, 'name': 'BARCODE' },
            { 'id': Options.GLNNO, 'name': 'GLN' },
            { 'id': Options.Supplier, 'name': 'SUPPLIER' }
        ];

        $scope.AnalyserFilters.sort(function (a, b) {
            var x = a.name.toLowerCase();
            var y = b.name.toLowerCase();
            return x < y ? -1 : x > y ? 1 : 0;
        });

        $scope.AddCondition = function (key, value, reflist) {
            var dispvalue = value;
            if (reflist && reflist) {
                var refitem = reflist.find(function (item) {
                    return item.id == value;
                });
                if (refitem && refitem != null)
                    dispvalue = refitem.value;
            }
            if ($scope.SearchConditionStkTrans.conditions == undefined ||
                $scope.SearchConditionStkTrans.conditions == null)
                $scope.SearchConditionStkTrans.conditions = [];

            if ($scope.SearchConditionStkTrans.conditiontype == $scope.Options.Barcode)
                $scope.SearchConditionStkTrans.HasBarcodeFilter = true;

            $scope.SearchConditionStkTrans.conditions.push({ 'option': $scope.SearchConditionStkTrans.conditiontype, 'key': key, 'value': value, 'display': dispvalue });
        }

        $scope.AllSelected = function (array, value) {
            $.each(array, function (k, v) { v.selected = value });
        }

        $scope.RemoveFromArray = function (item, list) {
            if (item.option == $scope.Options.Barcode)
                $scope.SearchConditionStkTrans.HasBarcodeFilter = false;
            list.splice(list.indexOf(item), 1);
        }

    }]);