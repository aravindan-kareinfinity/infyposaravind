
var app = angular.module('InfinityLab');

app.controller('UniqueSequenceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;


        $scope.Entity = {
            suffix: "",
            prefix: "",
            fromno: "",
            tono:""
        };


        $scope.EntityItem = {
            sequenceno: "",
            sequenceindex: 0,
            version: 1,
            isactive: true,
            used: false,
            usedreferencename :''
        };

        $scope.UniqueSequence = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.UniqueSequence = (JSON.parse(JSON.stringify($scope.Entity)));
        }
       
        $scope.ErrorMessage = "";
        
        $scope.New = function () {
            $scope.UniqueSequence = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.Delete = function () {

            var request = {
                "Items": this.igrid.GetSelectedRows(),
                "securitytoken": "String content"
            }

            if (request.Items.length == 0) return;

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "UniquesequanceService/DeleteAll",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Unique Sequence Deleted Successfully.!");
            }).error(function (result) {
                $rootScope.Toaster.Error(result);
            });
        }

        $scope.Save = function () {
            var request = {
                "Items": $scope.NewList,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "UniquesequanceService/SaveAll",
                data: srequest
            }).success(function (result) {
                    $rootScope.Toaster.Info("Unique Sequence Saved Successfully.!");
            }).error(function (result) {
                $rootScope.Toaster.Error(result);
            });
        }

        $scope.NewList = [];
        $scope.Generate = function () {

            var length = $scope.UniqueSequence.fromno.toString().length;
            var start = parseInt($scope.UniqueSequence.fromno);
            var end = parseInt($scope.UniqueSequence.tono);
            
            if (isNaN(start) || isNaN(end)) return;

            var list = [];
            for (; start <= end; start++) {
                var sequence = $scope.UniqueSequence.prefix +
                    start.toString().padStart(length, '0') +
                    $scope.UniqueSequence.suffix;

                var item = (JSON.parse(JSON.stringify($scope.EntityItem)));
                item.sequenceno = sequence;
                item.sequenceindex = start;
                list.push(item);
            }
            $scope.NewList = list;
            $scope.SearchResult = list;
            $scope.UniqueSequenceConfig();
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
                                    url: "UniquesequanceService/Delete",
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
            else if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.UniqueSequence = item;
                $scope.Navigation.SetMode('EDIT');
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.UniqueSequence,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "UniquesequanceService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.UniqueSequenceConfig();
            }).error(function (result) {
            });

        }

        $scope.SearchResult = [];

        $scope.UniqueSequenceConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.AddSimpleColumn('sequenceindex', 'No', 150);
            igrid.AddSimpleColumn('sequenceno', 'Sequence No', 150);

            var column = igrid.AddCustomColumn('used', 'Used', 50, function (item, col) {
                return item.used ? '*' : '';
            });
            igrid.AddSimpleColumn('usedreferencename', 'Details', 200);

            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }


    }]);