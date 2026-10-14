
var app = angular.module('InfinityLab');

app.controller('ProductGroupController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {


        $scope.Entity = {
            isactive: true,
            createdby: "",
            modifiedby: "",
            version: "",
            id: "",
            createdon: "",
            modifiedon: "",
            code: "",
            name: "",
            searchmode: "",
            materialenable: "",
            materialname: "",
            patternenable: "",
            patternname: "",
            styleenable: "",
            stylename: "",
            sizeenable: "",
            sizename: "",
            typeenable: "",
            typename: "",
            colourenable: "",
            colourname: ""
        };

        $scope.ProductGroup = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.ProductGroup = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.PricingMode = [{ 'id': 1, 'name': 'Barcode Based' }, { 'id': 2, 'name': 'Item Based' }];
        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            code: "",
            name: ""
        }

        $scope.Clear = function () {
            $scope.ProductGroup = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.Save = function () {

            var request = {
                "Item": $scope.ProductGroup,
                "securitytoken": "String content"
            }

            //  var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ProductgroupService/Save",
                data: request
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("Products Saved Successfully.!");
                    $scope.ProductGroup = (JSON.parse(JSON.stringify($scope.Entity)));

                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

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
                                    url: "ProductgroupService/Delete",
                                    data: request
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
                if (item.brands == null)
                    item.brands = [];

                $scope.ShowSearch = false;
                $scope.ProductGroup = item;
                
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            //    var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ProductgroupService/Search",
                data: request
            }).success(function (result) {

                $scope.SearchResult = result;
            }).error(function (result) {
            });


        }
        $timeout(function () {
            $scope.setFocusfirst();
        }, 10);
    }]);




