from flask import Flask, request, jsonify
import numpy as np
import pandas as pd
import tensorflow as tf
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

# Load the model
model = tf.keras.models.load_model('dogModel.h5')

# List of all expected features (86 in your case)
# You need to replace this with your actual feature names in order
all_features = ["Fever", "Nasal Discharge", "Loss of appetite", "Weight Loss", "Lameness", "Breathing Difficulty", "Swollen Lymph nodes", "Lethargy", "Depression", "Coughing", "Diarrhea", "Seizures", "Vomiting", "Eating less than usual", "Excessive Salivation", "Redness around Eye area", "Severe Dehydration", "Pain", "Discomfort", "Sepsis", "WeightLoss", "Tender abdomen", "Increased drinking and urination", "Bloated Stomach", "Yellow gums", "Constipation", "Paralysis", "Wrinkled forehead", "Continuously erect and stiff ears", "Grinning appearance", "Stiff and hard tail", "Stiffness of muscles", "Acute blindness", "Blood in urine", "Hunger", "Cataracts", "Losing sight", "Glucose in urine", "Burping", "blood in stools", "Passing gases", "Eating grass", "Scratching", "Licking", "Itchy skin", "Redness of skin", "Face rubbing", "Loss of Fur", "Swelling of gum", "Redness of gum", "Receding gum", "Bleeding of gum", "Plaque", "Bad breath", "Tartar", "Lumps", "Swelling", "Red bumps", "Scabs", "Irritation", "Dry Skin", "Fur loss", "Red patches", "Heart Complication", "Weakness", "Aggression", "Pale gums", "Coma", "Collapse", "Abdominal pain", "Difficulty Urinating", "Dandruff", "Anorexia", "Blindness", "excess jaw tone", "Urine infection", "Lack of energy", "Smelly", "Neurological Disorders", "Eye Discharge", "Loss of Consciousness", "Enlarged Liver", "lethargy", "Purging", "Bloody discharge", "Wounds"]

# List of all diseases (output classes)
diseases = [
    'Tick fever', 'Distemper', 'Parvovirus', 'Hepatitis', 'Tetanus', 
    'Chronic kidney Disease', 'Diabetes', 'Gastrointestinal Disease', 
    'Allergies', 'Gingitivis', 'Cancers', 'Skin Rashes'
]

@app.route('/predict', methods=['POST'])
def predict():
    try:
        # Get input data
        input_data = request.json
        
        # Create a dictionary with all features set to 0
        features_dict = {feature: 0 for feature in all_features}
        
        # Update with provided features
        for key, value in input_data.items():
            if key in features_dict:
                features_dict[key] = value
        
        # Convert to array in the correct order
        input_array = np.array([features_dict[feature] for feature in all_features]).reshape(1, -1)
        
        # Make prediction
        prediction = model.predict(input_array)
        
        # Get top 3 predictions
        top3_indices = np.argsort(prediction[0])[-3:][::-1]
        top3_diseases = [diseases[i] for i in top3_indices]
        top3_probabilities = [float(prediction[0][i]) for i in top3_indices]
        
        # Prepare response
        response = {
            'predictions': [
                {'disease': disease, 'probability': prob}
                for disease, prob in zip(top3_diseases, top3_probabilities)
            ]
        }
        
        return jsonify(response)
    
    except Exception as e:
        return jsonify({'error': str(e)}), 400

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)



