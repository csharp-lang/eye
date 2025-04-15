using FluentAssertions;

namespace Validator.UnitTests
{
    public class ValidatorTests
    {
        private readonly string[] invalidInput =
        [
            "415. Apple",
            "30432. Something something something",
            "1. Apple",
            "32. Cherry is the best",
            "2. Banana is yellow"
        ];

        private readonly string[] validInput =
        [
            "1. Apple",
            "415. Apple",
            "2. Banana is yellow",
            "32. Cherry is the best",
            "30432. Something something something"
        ];

        [Test]
        public void Validator_Should_Not_Throw_Exception_On_Valid_Input()
        {
            //Act
            Action act = () => Validator.Validate(validInput);

            //Assert
            act.Should().NotThrow();
        }

        [Test]
        public void Validator_Should_Throw_Exception_On_Invalid_Input()
        {
            //Act
            Action act = () => Validator.Validate(invalidInput);

            //Assert
            act.Should().Throw<ApplicationException>().WithMessage("Sorting is wrong. The number of erroneous line is 3");
        }
    }
}