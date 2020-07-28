# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.10.0] - 2020-07-28

**Changed**: 
- Removed *ConfigsProvider* 
- Removed *IConfigsContainer* 

## [0.9.0] - 2020-07-09

**Changed**: 
- Renamed observable collections getters to properties
- Now the observable collections don't take the collection reference directly in the constructor but rather the lambda to get it. This allows for async setups with observable collections.

## [0.8.1] - 2020-07-08

- Added implicit operator to convert *ObservableField* to it's defined generic type
- Added the *InvokeObserve* to allow the subscribed method to be invoked in the same time when it's marked to be observed

## [0.8.0] - 2020-07-07

- Added *ObservableField*

**Changed**: 
- Renamed *ListUpdateType* to *ObservableUpdateType* to have the same enum shared between all observable collections

## [0.7.0] - 2020-04-19

**Changed**: 
- Now *ObservableDictionary* has the same parity functionality as the *IdList* and keeps the reference of the managed dictionary so it can be used in other external logic

## [0.6.1] - 2020-04-18

- Added the possibility for *ObservableDictionary* to be instantiated with a list in the constructor

## [0.6.0] - 2020-04-18

- Added the possibility for *ObservableDictionary* to be instantiated with a dictionary in the constructor

**Changed**: 
- Now *IdList* is assessed with a this.[] operator instead of a *Get* & *Set* direct call

## [0.5.0] - 2020-03-07

- Added *EnumSelector<T>* & it's property drawer to allow to serialize enums in GameObjects/ScriptableObjects as strings and not the enum value. This will prevent corrupted data when enum values are changed or removed
- Added sample example for an *EnumSelector<T>* use case

## [0.4.0] - 2020-02-26

- Added *ISingleConfigContainer*
- Added possibility to add single containers to *ConfigsProvider*

## [0.3.0] - 2020-02-25

- Added *ObservableDictionary*
- Added KeyValuePairTypes
- Added Vector3 extensions

## [0.2.0] - 2020-01-20

- Added *IdList* to help wrapping the access to a list of structs by a defined generic id

**Changed**: 
- Removed *IConfig*. Now the *ConfigsProvider* has an idResolver *Func* to resolve the map id to the config when adding configs to the provider

## [0.1.2] - 2020-01-09

**Fixed**: 
- Added missing meta files

## [0.1.1] - 2020-01-09

**Changed**: 
- Now *Configs* is *ConfigsProvider*
- Now *IConfigs* is *IConfigsProvider*

## [0.1.0] - 2020-01-06

- Initial submission for package distribution
