using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Icebreaker.Enums;

namespace Icebreaker
{
    public class SingleMessageSearch : MessageRun
    {
        protected IEnumerable<BodyType> _bodyTypesSearchingFor;
        protected IEnumerable<Ethnicity> _ethnicitiesSearchingFor;
        protected IEnumerable<Gender> _gendersSearchingFor;
        protected IEnumerable<Orientation> _orientationsSearchingFor;
        protected IEnumerable<RelationshipStatus> _relationshipStatuses;
        protected int _minAge;
        protected int _maxAge;
        protected int _minMatchPercentage;
        protected int _maxMatchPercentage;
        protected string _messageText;
        private bool _kidsAllowed;

        public int MinMatchPercentage
        {
            set { _minMatchPercentage = value; }
            get { return _minMatchPercentage; }
        }

        public int MaxMatchPercentage
        {
            set { _maxMatchPercentage = value; }
            get { return _maxMatchPercentage; }
        }

        public string MessageText
        {
            set { _messageText = value; }
            get { return _messageText; }
        }

        public IEnumerable<BodyType> BodyTypesSearchingFor
        {
            get { return _bodyTypesSearchingFor; }
            set { _bodyTypesSearchingFor = value; }
        }

        public IEnumerable<Ethnicity> EthnicitiesSearchingFor
        {
            get { return _ethnicitiesSearchingFor; }
            set { _ethnicitiesSearchingFor = value; }
        }

        public IEnumerable<Gender> GendersSearchingFor
        {
            get { return _gendersSearchingFor; }
            set { _gendersSearchingFor = value; }
        }

        public IEnumerable<Orientation> OrientationsSearchingFor
        {
            get { return _orientationsSearchingFor; }
            set { _orientationsSearchingFor = value; }
        }

        public IEnumerable<RelationshipStatus> RelationshipStatuses
        {
            get { return _relationshipStatuses; }
            set { _relationshipStatuses = value; }
        }

        public int MinAge
        {
            get { return _minAge; }
            set { _minAge = value; }
        }

        public int MaxAge
        {
            get { return _maxAge; }
            set { _maxAge = value; }
        }

        public bool KidsAllowed
        {
            get { return _kidsAllowed; }
            set { _kidsAllowed = value; }
        }

        public SingleMessageSearch()
        {
            
        }

        public SingleMessageSearch(int minAge
            , int maxAge
            , int minMatchPercentage
            , int maxMatchPercentage
            , int numMessagesToSend
            , string messageText
            , bool kidsAllowed
            , IEnumerable<BodyType> bodyTypes
            , IEnumerable<Ethnicity> ethnicities
            , IEnumerable<Gender> genders
            , IEnumerable<Orientation> orientations
            , IEnumerable<RelationshipStatus> relationshipStatuses)
        {
            MinAge = minAge;
            MaxAge = maxAge;
            _minMatchPercentage = minMatchPercentage;
            _maxMatchPercentage = maxMatchPercentage;
            _numMessagesToSend = numMessagesToSend;
            _messageText = messageText;
            KidsAllowed = kidsAllowed;
            BodyTypesSearchingFor = bodyTypes;
            EthnicitiesSearchingFor = ethnicities;
            GendersSearchingFor = genders;
            OrientationsSearchingFor = orientations;
            RelationshipStatuses = relationshipStatuses;
        }
    }
}
