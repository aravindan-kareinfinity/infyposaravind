var app = angular.module('InfinityLab');

app.controller('StockBinboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;


        $scope.Entity = {
            name: "",
            printingname: "",
            categoryid: "",
            uomid: "",
            colorid: "",
            width: "",
            taxid: ""
        };


        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
        });

        $scope.CurrentNode = { 'binlist': [], supportallocation: false };
        $scope.Clipboard = {};
        $scope['bintree'] = $scope['bintree'] || {};
        $scope['bintree'].onCurrentNodeChange = function (node) {
            $scope.CurrentNode = node;
            if (node.supportallocation) {
                $scope.ShowList();
            }
            else {
                if ($scope.sgrid != null)
                    $scope.sgrid.destroy();
                $scope.sgrid = null;
            }
        }

        $scope.CurrentAvailableStock = [];
        $scope.CurrentStock = 0;
        $scope.CurrentBinStock = {};
        $scope.CurrentAllocated = 0;
        $scope.Searchavailablestock = function () {
            restService.Get('MfgbinstoragestockService/SearchAvailableStock/' + $scope.uan, true).then(function (results) {
                $scope.CurrentAvailableStock = results.items;
                $scope.CurrentStock = results.stockqty;
                $scope.CurrentAllocated = results.allocated;
            });
        }

        $scope.Add2Bin = function () {
            request = {
                "Item": {
                    'items': $scope.CurrentAvailableStock,
                    'storage': $scope.CurrentBinStock,
                    'qty': $scope.qty,
                    'uan': $scope.uan,
                    'mfgbinstoragecode': $scope.mfgbinstoragecode
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgbinstoragestockService/Add2Bin",
                data: srequest
            }).success(function (result) {
                var qty = 0;
                $.each(result, function (k, v) {
                    qty += v.iqty;
                    $scope.CurrentAvailableStock.find(function (item) {
                        return item.stockid == v.stockid
                    }).allocated += v.iqty;
                });
                $scope.uan = '';
                $scope.qty = '';
                $scope.CurrentStock = 0;
                $scope.CurrentAllocated = 0;
                $scope.setFocusuan();
                $rootScope.Toaster.Info(qty.toString() + " added to bin - " + $scope.mfgbinstoragecode);
            }).error(function (result) {
            });
        }

        $scope.RemovefromBin = function () {
            request = {
                "Item": {
                    'items': $scope.CurrentAvailableStock,
                    'storage': $scope.CurrentBinStock,
                    'qty': $scope.qty, 'uan': $scope.uan
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgbinstoragestockService/RemovefromBin",
                data: srequest
            }).success(function (result) {
                $scope.CurrentAvailableStock.find(function (item) {
                    return item.stockid == result.stockid
                }).allocated -= $scope.qty;
                $scope.CurrentAllocated -= $scope.qty;
                $rootScope.Toaster.Info($scope.qty.toString() + " added to bin - " + $scope.mfgbinstoragecode);
            }).error(function (result) {
            });
        }

        $scope.Searchbinstock = function () {
            restService.Get('MfgbinstoragestockService/SearchBinStock/' + $scope.CurrentNode.id + "/" + $scope.mfgbinstoragecode, true).then(function (results) {
                $scope.CurrentBinStock = results;
                if ($scope.CurrentBinStock.stocklist != null && $scope.CurrentBinStock.stocklist.length > 0)
                    $scope.ShowStockList();
                else if ($scope.currentgridresult != 'ShowList')
                    $scope.ShowList();
            });
        }

        $scope.currentgridresult = null;
        $scope.ShowStockList = function () {
            $scope.currentgridresult = 'ShowStockList';
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('product', 'Description', 250);
            igrid.AddSimpleColumn('uid', 'ID', 100);
            igrid.AddSimpleColumn('designname', 'Design', 100);
            igrid.AddSimpleColumn('sizegroup', 'Size Group', 80);
            igrid.AddSimpleColumn('sizename', 'Size', 80);
            igrid.AddSimpleColumn('colourname', 'Colour', 80);
            igrid.AddSimpleColumn('stock', 'Stock', 80);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Outward'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Damaged'/>"

            igrid.AddButtonColumn('action', 'ACTION', 150, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Outward') {
                        
                    }
                    else if ($(e.target).val() == 'Damaged') {
                        
                    }
                });
            });

            igrid.Initialize();
            $scope.sgrid = igrid;
            $scope.sgrid.UpdateData($scope.CurrentBinStock.stocklist);
        }

        $scope.ShowList = function () {
            currentgridresult = 'ShowList';
            if ($scope.sgrid != null)
                $scope.sgrid.destroy();
            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 25);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('id', 'ID', 50);
            igrid.AddSimpleColumn('row', 'Row', 80);
            igrid.AddSimpleColumn('column', 'Column', 80);
            igrid.AddSimpleColumn('code', 'Code', 80);
            igrid.AddSimpleColumn('maxqty', 'Qty', 80);
            igrid.AddSimpleColumn('width', 'Width', 80);
            igrid.AddSimpleColumn('height', 'Height', 80);

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Change'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Change') {
                        $scope.EditBinItem(args.item);
                    }
                    else if ($(e.target).val() == 'Delete') {
                        if (typeof ($scope.bintree.currentNode) == 'undefined')
                            return;
                        var currentnode = $scope.bintree.currentNode;

                        $scope.bintreedata[0].DeletedBinList = $scope.bintreedata[0].DeletedBinList || [];
                        if (typeof (args.item.id) != 'undefined' && args.item.id > 0) {
                            $scope.bintreedata[0].DeletedBinList.push(args.item.id);
                            currentnode.binlist.splice(currentnode.binlist.indexOf(args.item), 1);
                            $scope.sgrid.UpdateData(currentnode.binlist);
                        }
                    }
                });
            });

            igrid.Initialize();
            $scope.sgrid = igrid;
            if (typeof ($scope.bintree.currentNode) == 'undefined')
                return;

            var currentnode = $scope.bintree.currentNode;
            $scope.sgrid.UpdateData(currentnode.binlist);
        }

        $scope.LoadConfiguration = function () {
            restService.Get('ReferencelistService/BINHIERARCHY/' + $scope.Binstorage.locationid, true).then(function (results) {
                if (results.length == 0) {

                    var name = $scope.SearchRef($scope.Binstorage.locationid, $scope.Location);
                    $scope.bintreedata =
                        [
                            {
                                "name": name, "id": 0,
                                "referencelocationid": $scope.Binstorage.locationid,
                                "stylecollapsed": 'location_collapsed',
                                "styleexpanded": 'location_expanded',
                                "stylenormal": 'location_normal',
                                "supportchild": true,
                                "supportallocation": false,
                                "level": 0,
                                "nodetype": "location",
                                "children": [],
                                "isactive": true,
                                "version": 1
                            }
                        ];


                }
                else {
                    var name = $scope.SearchRef($scope.Binstorage.locationid, $scope.Location);
                    $scope.bintreedata =
                        [
                            {
                                "name": name, "id": results[0].id,
                                "referencelocationid": $scope.Binstorage.locationid,
                                "stylecollapsed": 'location_collapsed',
                                "styleexpanded": 'location_expanded',
                                "stylenormal": 'location_normal',
                                "supportchild": true,
                                "supportallocation": false,
                                "level": 0,
                                "isactive": true,
                                "version": results[0].version,
                                "nodetype": "location",
                                "children": results[0].children
                            }
                        ];
                }
                $scope.AssignParent($scope.bintreedata[0], null);
            });
        }

        
        $scope.ErrorMessage = "";

        $scope.AssignParent = function (node, parentnode) {
            node.parentnode = parentnode;
            if (typeof (node.children) != 'undefined') {
                $.each(node.children, function (k, v) {
                    $scope.AssignParent(v, node);
                })
            }
        }

        $scope.RemoveParent = function (node) {
            node.parentnode = null;
            if (typeof (node.children) != 'undefined') {
                $.each(node.children, function (k, v) {
                    $scope.RemoveParent(v)
                })
            }
        }

        $scope.Save = function () {
            $scope.RemoveParent($scope.bintreedata[0]);
            var item = (JSON.parse(JSON.stringify($scope.bintreedata[0])));
            request = {
                "Item": item,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ReferencelistService/SaveBinStorage",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $scope.bintreedata[0].id = result.id;
                    $scope.bintreedata[0].DeletedBinList = [];
                    $scope.bintreedata[0].version = result.version;
                    $scope.bintreedata[0].referencelocationid = result.referencelocationid;
                    $scope.bintreedata[0].children = result.children;
                    $scope.AssignParent($scope.bintreedata[0], null);
                    $rootScope.Toaster.Info("Saved Successfully.!");
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            }).error(function (result) {
            });

        }


        $scope.bintreedata = [];
        
        $scope.panelheight = { 'height': '400px' };
        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 100) + 'px';
            }
        }, 100);


    }]);